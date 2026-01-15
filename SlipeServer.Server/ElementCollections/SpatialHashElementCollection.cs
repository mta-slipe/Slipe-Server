using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace SlipeServer.Server.ElementCollections;

public class SpatialHashElementCollection : IElementCollection
{
    private readonly ConcurrentDictionary<CellCoordinate, ConcurrentDictionary<uint, Element>> cells;
    private readonly ConcurrentDictionary<uint, CellCoordinate> elementCells;
    private readonly Vector3 cellSize;
    private int count;

    public int Count => this.count;

    public SpatialHashElementCollection(float cellSizeX = 20f, float cellSizeY = 200f, float cellSizeZ = 200f)
    {
        this.cells = new ConcurrentDictionary<CellCoordinate, ConcurrentDictionary<uint, Element>>();
        this.elementCells = new ConcurrentDictionary<uint, CellCoordinate>();
        this.cellSize = new Vector3(cellSizeX, cellSizeY, cellSizeZ);
    }

    public SpatialHashElementCollection()
        : this(20f, 200f, 200f) { }

    private CellCoordinate GetCellCoordinate(Vector3 position)
    {
        return new CellCoordinate(
            (int)MathF.Floor(position.X / this.cellSize.X),
            (int)MathF.Floor(position.Y / this.cellSize.Y),
            (int)MathF.Floor(position.Z / this.cellSize.Z)
        );
    }

    public void Add(Element element)
    {
        element.PositionChanged += ReInsertElement;
        var cell = GetCellCoordinate(element.Position);

        var cellDict = this.cells.GetOrAdd(cell, _ => new ConcurrentDictionary<uint, Element>());
        cellDict[element.Id.Value] = element;
        this.elementCells[element.Id.Value] = cell;

        Interlocked.Increment(ref this.count);
    }

    public Element? Get(uint id)
    {
        if (this.elementCells.TryGetValue(id, out var cell))
        {
            if (this.cells.TryGetValue(cell, out var cellDict))
            {
                if (cellDict.TryGetValue(id, out var element))
                {
                    return element;
                }
            }
        }
        return null;
    }

    public void Remove(Element element)
    {
        element.PositionChanged -= ReInsertElement;

        if (this.elementCells.TryRemove(element.Id.Value, out var cell))
        {
            if (this.cells.TryGetValue(cell, out var cellDict))
            {
                if (cellDict.TryRemove(element.Id.Value, out _))
                {
                    if (cellDict.IsEmpty && this.cells.TryRemove(cell, out var removedCell))
                    {
                        // Double-check it's still empty after removal (race condition protection)
                        if (!removedCell.IsEmpty)
                        {
                            this.cells.TryAdd(cell, removedCell);
                        }
                    }
                }
            }
            Interlocked.Decrement(ref this.count);
        }
    }

    public IEnumerable<Element> GetAll()
    {
        return this.cells.Values
            .SelectMany(cellDict => cellDict.Values)
            .ToArray();
    }

    public IEnumerable<TElement> GetByType<TElement>(ElementType elementType) where TElement : Element
    {
        return this.cells.Values
            .SelectMany(cellDict => cellDict.Values)
            .Where(element => element.ElementType == elementType)
            .Cast<TElement>()
            .ToArray();
    }

    public IEnumerable<TElement> GetByType<TElement>() where TElement : Element
    {
        return this.GetByType<TElement>(ElementTypeHelpers.GetElementType<TElement>());
    }

    public IEnumerable<Element> GetWithinRange(Vector3 position, float range)
    {
        var minCell = GetCellCoordinate(position - new Vector3(range, range, range));
        var maxCell = GetCellCoordinate(position + new Vector3(range, range, range));
        var rangeSquared = range * range;

        var results = new List<Element>();
        for (int x = minCell.X; x <= maxCell.X; x++)
        {
            for (int y = minCell.Y; y <= maxCell.Y; y++)
            {
                for (int z = minCell.Z; z <= maxCell.Z; z++)
                {
                    var cell = new CellCoordinate(x, y, z);
                    if (this.cells.TryGetValue(cell, out var cellDict))
                    {
                        foreach (var element in cellDict.Values)
                        {
                            var distanceSquared = Vector3.DistanceSquared(position, element.Position);
                            if (distanceSquared <= rangeSquared)
                            {
                                results.Add(element);
                            }
                        }
                    }
                }
            }
        }

        return [.. results];
    }

    public IEnumerable<TElement> GetWithinRange<TElement>(Vector3 position, float range, ElementType elementType) where TElement : Element
    {
        return this.GetWithinRange(position, range)
            .Where(element => element.ElementType == elementType)
            .Cast<TElement>()
            .ToArray();
    }

    public IEnumerable<TElement> GetWithinRange<TElement>(Vector3 position, float range) where TElement : Element
    {
        return GetWithinRange<TElement>(position, range, ElementTypeHelpers.GetElementType<TElement>());
    }

    private void ReInsertElement(Element element, ElementChangedEventArgs<Vector3> args)
    {
        var oldCell = GetCellCoordinate(args.OldValue);
        var newCell = GetCellCoordinate(element.Position);

        if (!oldCell.Equals(newCell))
        {
            var elementId = element.Id.Value;

            if (this.cells.TryGetValue(oldCell, out var oldCellDict))
            {
                if (oldCellDict.TryRemove(elementId, out _))
                {
                    if (oldCellDict.IsEmpty && this.cells.TryRemove(oldCell, out var removedCell))
                    {
                        // Double-check it's still empty after removal (race condition protection)
                        if (!removedCell.IsEmpty)
                        {
                            this.cells.TryAdd(oldCell, removedCell);
                        }
                    }
                }
            }

            var newCellDict = this.cells.GetOrAdd(newCell, _ => new ConcurrentDictionary<uint, Element>());
            newCellDict[elementId] = element;

            this.elementCells[elementId] = newCell;
        }
    }

    private readonly struct CellCoordinate(int x, int y, int z) : IEquatable<CellCoordinate>
    {
        public readonly int X = x;
        public readonly int Y = y;
        public readonly int Z = z;

        public bool Equals(CellCoordinate other) => X == other.X && Y == other.Y && Z == other.Z;
        public override bool Equals(object? obj) => obj is CellCoordinate other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y, Z);
    }
}
