using SlipeServer.Server.Loaders.Map.ElementsDefinitions;
using SlipeServer.Server.Loaders.Map.ElementsDefinitions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server.Loaders.Map
{
    using Map = Elements.Grouped.Map;
    public abstract class MapLoaderExceptionBase : Exception
    {
        public MapLoaderExceptionBase(string message) : base("Failed to load map, reason: " + message)
        {

        }
    }

    public class DuplicateMapElementIdException : MapLoaderExceptionBase
    {
        public string Id { get; set; }
        public DuplicateMapElementIdException(string id, string definitionName) : base($"Duplicated id: '{id}' for '{definitionName}'")
        {
            this.Id = id;
        }
    }
    public class InvalidMapElementIdException : MapLoaderExceptionBase
    {
        public InvalidMapElementIdException(bool wasEmpty, string definitionName) : base(wasEmpty ? $"Missing id for '{definitionName}'" : $"Invalid id for '{definitionName}'" )
        {
        }
    }

    public class Resolver<T>
    {
        private readonly Map map;
        private readonly T data;
        private readonly MapLoaderOptions mapLoaderOptions;

        public Resolver(Map map, T data, MapLoaderOptions mapLoaderOptions)
        {
            this.map = map;
            this.data = data;
            this.mapLoaderOptions = mapLoaderOptions;
        }

        public void Resolve<Definition, Produces>(Expression<Func<T, Definition[]?>> selector, Func<Definition, Produces> resolve) where Definition : IdDefinition, IDefinitionName
        {
            if (selector.Body is not MemberExpression memberExpression)
                return;

            if (memberExpression.Member is not PropertyInfo propertyInfo)
                return;

            var value = propertyInfo.GetValue(this.data, null);
            if (value == null)
                return;

            var select = selector.Compile();
            var selectedType = select(this.data);
            if (selectedType == null)
                return;

            try
            {

                foreach (var item in selectedType)
                {
                    var result = resolve(item);
                    if (result != null)
                    {
                        switch (mapLoaderOptions.IdentifiersBehaviour)
                        {
                            case EIdentifiersBehaviour.Ignore:
                                if(item.Id == null || this.map.ContainsKey(item.Id))
                                {
                                    this.map[Guid.NewGuid().ToString()] = result;
                                }
                                else
                                {
                                    this.map[item.Id] = result;
                                }
                                break;
                            case EIdentifiersBehaviour.Throw:
                                if (string.IsNullOrWhiteSpace(item.Id))
                                {
                                    throw new InvalidMapElementIdException(item.Id == string.Empty, item.DefinitionName);
                                }
                                else if (this.map.ContainsKey(item.Id))
                                {
                                    throw new DuplicateMapElementIdException(item.Id, item.DefinitionName);
                                }
                                else
                                {
                                    this.map[item.Id] = result;
                                }
                                break;
                        }
                    }
                }
            }
            catch (MapLoaderExceptionBase)
            {
                throw;
            }
        }
    }
}
