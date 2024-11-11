namespace SlipeServer.Navigation;

public class NavigationNode : IEquatable<NavigationNode>
{
    public uint Id { get; internal set; }
    public Vector3 Position { get; }

    public NavigationNode(Vector3 position)
    {
        this.Position = position;
    }

    public bool Equals(NavigationNode? other) => other != null && this.Id == other.Id;
}

public class NavigationEdge : IEquatable<NavigationEdge>
{
    private static uint _id = 0;

    public uint Id { get; }
    public int CostFactor { get; }

    public NavigationEdge(int costFactor)
    {
        this.Id = Interlocked.Increment(ref _id);
        this.CostFactor = costFactor;
    }

    public bool Equals(NavigationEdge? other) => other != null && this.Id == other.Id;
}

public class NavigationGraph<TNode, TEdge> where TNode: NavigationNode where TEdge: NavigationEdge, IEquatable<TEdge>
{
    private readonly Graph<TNode, TEdge> graph;
    private readonly KdTree<float, TNode> tree;
    private readonly Dictionary<uint, TNode> nodes;

    public NavigationGraph()
    {
        this.graph = new();
        this.tree = new(3, new FloatMath());
        this.nodes = [];
    }

    public TNode AddNode(TNode node)
    {
        var id = this.graph.AddNode(node);
        node.Id = id;
        this.tree.Add([node.Position.X, node.Position.Y, node.Position.Z], node);
        this.nodes[id] = node;
        return node;
    }

    public void Connect(TNode nodeA, TNode nodeB, TEdge edge, bool twoWay = false)
    {
        var distance = (int)((nodeA.Position - nodeB.Position).LengthSquared() * 100.0f); // Multiply by 100 to get 0.01m precision
        var cost = distance * edge.CostFactor;

        this.graph.Connect(nodeA.Id, nodeB.Id, cost, edge);
        if(twoWay)
            this.graph.Connect(nodeB.Id, nodeA.Id, cost, edge);
    }
    
    public void Connect(TEdge edge, bool twoWay, bool loop, params TNode[] nodes)
    {
        for (int i = 0; i < nodes.Length - 1; i++)
        {
            var nodeA = nodes[i];
            var nodeB = nodes[i + 1];
            Connect(nodeA, nodeB, edge, twoWay);
        }

        if (loop)
        {
            Connect(nodes[0], nodes[nodes.Length - 1], edge, twoWay);
        }
    }
    
    public void Connect(TEdge edge, bool twoWay, params TNode[] nodes)
    {
        for (int i = 0; i < nodes.Length - 1; i++)
        {
            var nodeA = nodes[i];
            var nodeB = nodes[i + 1];
            Connect(nodeA, nodeB, edge, twoWay);
        }
    }

    public IEnumerable<TNode> FindPath(TNode nodeA, TNode nodeB, int? depth = null)
    {
        ShortestPathResult result;
        if (depth != null)
        {
            result = this.graph.Dijkstra(nodeA.Id, nodeB.Id, depth.Value);
        } else
        {
            result = this.graph.Dijkstra(nodeA.Id, nodeB.Id);
        }

        if (!result.IsFounded)
            yield break;

        var path = result.GetPath();
        foreach (var nodeId in path)
            yield return this.nodes[nodeId];
    }

    public IEnumerable<TNode> FindPath(Vector3 from, Vector3 to, float? fromRadius = null, float? toRadius = null, int? depth = null)
    {
        var fromNode = this.tree.GetNearestNeighbours([from.X, from.Y, from.Z], 1).FirstOrDefault();
        if (fromNode == null)
            yield break;

        var toNode = this.tree.GetNearestNeighbours([to.X, to.Y, to.Z], 1).FirstOrDefault();
        if (toNode == null)
            yield break;

        if(fromRadius != null && (fromNode.Value.Position - from).Length() > fromRadius)
        {
            yield break;
        }

        if(toRadius != null && (toNode.Value.Position - to).Length() > toRadius)
        {
            yield break;
        }

        foreach (var node in FindPath(fromNode.Value, toNode.Value, depth))
            yield return node;
    }

    public TNode GetNodeById(uint id)
    {
        return this.nodes[id];
    }

    public TNode? GetNearestNode(Vector3 position)
    {
        return this.tree.GetNearestNeighbours([position.X, position.Y, position.Z], 1).FirstOrDefault()?.Value;
    }
}

public class NavigationGraph : NavigationGraph<NavigationNode, NavigationEdge>;
