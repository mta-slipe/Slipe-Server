using SlipeServer.Server.Loaders.Map.ElementsDefinitions;
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
    public class Resolver<T>
    {
        private readonly Map map;
        private readonly T data;

        public Resolver(Map map, T data)
        {
            this.map = map;
            this.data = data;
        }

        public void ResolveArray<Definition, Produces>(Expression<Func<T, Definition[]?>> selector, Func<Definition, Produces> resolve) where Definition : IdDefinition
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

            foreach (var item in selectedType)
            {
                var result = resolve(item);
                if (result != null)
                    this.map[item.Id ?? Guid.NewGuid().ToString()] = result;
            }
        }
    }
}
