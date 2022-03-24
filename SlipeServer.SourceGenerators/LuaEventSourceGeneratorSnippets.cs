namespace SlipeServer.SourceGenerators
{
    public class LuaEventSourceGeneratorSnippets
    {
        public const string GetFloat = @"
            this.__PROPERTY__ = dictionary[""__PROPERTY__""].FloatValue.HasValue ? 
                dictionary[""__PROPERTY__""].FloatValue.Value : 
                (float)dictionary[""__PROPERTY__""].DoubleValue.Value;";

        public const string GetOptionalFloat = @"
            this.__PROPERTY__ = dictionary.ContainsKey(""__PROPERTY__"") ? 
                dictionary[""__PROPERTY__""].FloatValue.HasValue ? 
                    dictionary[""__PROPERTY__""].FloatValue.Value : 
                    (float?)dictionary[""__PROPERTY__""].DoubleValue.Value : 
            (float?)null;";

        public const string GetDouble = @"
            this.__PROPERTY__ = dictionary[""__PROPERTY__""].DoubleValue.HasValue ? 
                dictionary[""__PROPERTY__""].DoubleValue.Value : 
                (double)dictionary[""__PROPERTY__""].FloatValue.Value;";

        public const string GetOptionalDouble = @"
            this.__PROPERTY__ = dictionary.ContainsKey(""__PROPERTY__"") ? 
                dictionary[""__PROPERTY__""].DoubleValue.HasValue ? 
                    dictionary[""__PROPERTY__""].DoubleValue.Value : 
                    (double?)dictionary[""__PROPERTY__""].FloatValue.Value : 
                (double?)null;";

        public const string GetInt = @"
            this.__PROPERTY__ = dictionary[""__PROPERTY__""].IntegerValue.Value;";

        public const string GetOptionalInt = @"
            this.__PROPERTY__ = dictionary.ContainsKey(""__PROPERTY__"") ? 
                dictionary[""__PROPERTY__""].IntegerValue.Value : 
                (int?)null;";

        public const string GetUInt = @"
            this.__PROPERTY__ = dictionary[""__PROPERTY__""].ElementId.Value;";

        public const string GetOptionalUInt = @"
            this.__PROPERTY__ = dictionary.ContainsKey(""__PROPERTY__"") ? 
                dictionary[""__PROPERTY__""].ElementId.Value : 
                (uint?)null;";

        public const string GetBool = @"
            this.__PROPERTY__ = dictionary[""__PROPERTY__""].BoolValue.Value;";

        public const string GetOptionalBool = @"
            this.__PROPERTY__ = dictionary.ContainsKey(""__PROPERTY__"") ? 
                dictionary[""__PROPERTY__""].BoolValue.Value : 
                (bool?)null;";

        public const string GetString = GetOptionalString;
        public const string GetOptionalString = @"
            this.__PROPERTY__ = dictionary.ContainsKey(""__PROPERTY__"") ? 
                dictionary[""__PROPERTY__""].StringValue : 
                (string)null;";

        public const string GetVector3 = @"
            var __PROPERTY__SubDictionary = new Dictionary<string, LuaValue>(
                dictionary[""__PROPERTY__""].TableValue.Select(x => 
                    new KeyValuePair<string, LuaValue>(x.Key.StringValue, x.Value)
                )
            );
            this.__PROPERTY__ = new Vector3(
                __PROPERTY__SubDictionary[""X""].FloatValue.Value, 
                __PROPERTY__SubDictionary[""Y""].FloatValue.Value, 
                __PROPERTY__SubDictionary[""Z""].FloatValue.Value);";

        public const string GetOptionalVector3 = @"
            if (dictionary.ContainsKey(""__PROPERTY__""))
            {
                var __PROPERTY__SubDictionary = new Dictionary<string, LuaValue>(
                    dictionary[""__PROPERTY__""].TableValue.Select(x => 
                        new KeyValuePair<string, LuaValue>(x.Key.StringValue, x.Value)
                    )
                );
                this.__PROPERTY__ = new Vector3(
                    __PROPERTY__SubDictionary[""X""].FloatValue.Value, 
                    __PROPERTY__SubDictionary[""Y""].FloatValue.Value, 
                    __PROPERTY__SubDictionary[""Z""].FloatValue.Value);
            }";

        public const string GetLuaValue = GetOptionalLuaValue;

        public const string GetOptionalLuaValue = @"
            if (dictionary.ContainsKey(""__PROPERTY__""))
            {
                this.__PROPERTY__ = new __TYPE__();
                this.__PROPERTY__.Parse(dictionary[""__PROPERTY__""].TableValue);
            }";

        public static string Format(string source, string type, string property)
        {
            return source
                .Replace("__TYPE__", type.TrimEnd('?'))
                .Replace("__PROPERTY__", property);
        }

        public static string FormatForArray(string source, string type, string property)
        {
            var modifiedSource = source
                .Replace("__TYPE__", type
                    .TrimEnd(']')
                    .TrimEnd('[')
                    .TrimEnd('?'))
                .Replace(@"dictionary[""__PROPERTY__""]", $"tempSub{property}.Value")
                .Replace(@"dictionary.ContainsKey(""__PROPERTY__"")", "true")
                .Replace("this.__PROPERTY__", $"temp{property}")
                .Replace("__PROPERTY__", $"temp{property}");
            var arraySnippet = @$"
            this.__ACTUALPROPERTY__ = new List<__TYPE__>(
                dictionary[""__PROPERTY__""].TableValue.Select(
                (tempSub{property}) =>
                {{
                    {type.TrimEnd(']').TrimEnd('[')} temp{property} = null{(type.EndsWith("?") ? "" : "!")};
                    { modifiedSource }
                    return temp{property};
                }})
            ).ToArray();";
            return arraySnippet
                .TrimEnd('?')
                .Replace("__TYPE__", type
                    .TrimEnd(']')
                    .TrimEnd('[')
                    .TrimEnd('?'))
                .Replace("__ACTUALPROPERTY__", property)
                .Replace("__PROPERTY__", property);
        }
    }
}
