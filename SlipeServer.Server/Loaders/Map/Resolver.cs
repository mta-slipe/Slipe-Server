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

        public void ResolveArray<Definition, Produces>(Expression<Func<T, Definition[]>> selector, Func<Definition, Produces> resolve) where Definition : IdDefinition
        {
            var memberExpression = selector.Body as MemberExpression;
            var propertyInfo = memberExpression.Member as PropertyInfo;
            var value = propertyInfo.GetValue(data, null);
            if (value == null)
                return;

            var select = selector.Compile();
            var foo = select(data);
            foreach (var item in foo)
            {
                var result = resolve(item);
                if (result != null)
                    map[item.Id ?? Guid.NewGuid().ToString()] = result;
            }
        }
    }
}
