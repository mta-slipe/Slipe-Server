using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlipeServer.SourceGenerators
{
    internal class LuaEventAttributeSyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> EventClasses { get; } = new List<ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax cds)
            {
                var attributes = cds.ChildNodes()
                    .Where(x => x is AttributeListSyntax)
                    .Select(x => x as AttributeListSyntax)
                    .FirstOrDefault();

                if (attributes != null)
                {
                    if (attributes.Attributes.Any(x => x.Name.ToString() == "LuaEvent"))
                        this.EventClasses.Add(cds);
                }
            }
        }
    }

    [Generator]
    public class LuaEventSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new LuaEventAttributeSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            LuaEventAttributeSyntaxReceiver syntaxReceiver = (LuaEventAttributeSyntaxReceiver)context.SyntaxReceiver;

            foreach (var eventClass in syntaxReceiver.EventClasses)
            {
                var sourceText = SourceText.From(GenerateParsePartialClass(eventClass), Encoding.UTF8);
                context.AddSource($"Generated{eventClass.Identifier.ValueText}", sourceText);
            }
        }

        private string GenerateParsePartialClass(ClassDeclarationSyntax eventClass)
        {
            //System.Diagnostics.Debugger.Launch();
            return $@"using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using SlipeServer.Server.Events;
using SlipeServer.Packets.Definitions.Lua;
using System.ComponentModel.DataAnnotations;

namespace {(eventClass.Parent as NamespaceDeclarationSyntax).Name.ToFullString()}
{{
    public partial class {eventClass.Identifier.ValueText}
    {{
        public partial void Parse(LuaEvent luaEvent)
        {{
            var dictionary = new Dictionary<string, LuaValue>(
                luaEvent.Parameters[0].TableValue.Select(x => 
                    new KeyValuePair<string, LuaValue>(x.Key.StringValue, x.Value)
                )
            );
{GenerateParseMethodBody(eventClass)}
            Validator.ValidateObject(this, new ValidationContext(this), true);
        }}
    }}
}}";
        }

        private string GenerateParseMethodBody(ClassDeclarationSyntax eventClass)
        {
            var builder = new StringBuilder();

            var properties = eventClass.ChildNodes()
                .Where(x => x is PropertyDeclarationSyntax)
                .Select(x => x as PropertyDeclarationSyntax);

            foreach (var property in properties)
            {
                var type = property.Type.ToString();
                var name = property.Identifier.ValueText;

                builder.Append($"            {GetPropertyParser(type, name)}\n");
            }

            var code = builder.ToString();
            return builder.ToString();
        }

        private string GetPropertyParser(string type, string property)
        {
            switch (type)
            {
                case "float":
                    return $@"this.{ property } = dictionary[""{ property }""].FloatValue.HasValue ? dictionary[""{ property }""].FloatValue.Value : (float)dictionary[""{ property }""].DoubleValue.Value;";
                case "float?":
                    return $@"this.{ property } = dictionary.ContainsKey(""{ property }"") ? dictionary[""{ property }""].FloatValue.HasValue ? dictionary[""{ property }""].FloatValue.Value : (float?)dictionary[""{ property }""].DoubleValue.Value : (float?)null;";
                case "double":
                    return $@"this.{ property } = dictionary[""{ property }""].DoubleValue.HasValue ? dictionary[""{ property }""].DoubleValue.Value : (double)dictionary[""{ property }""].FloatValue.Value;";
                case "double?":
                    return $@"this.{ property } = dictionary.ContainsKey(""{ property }"") ? dictionary[""{ property }""].DoubleValue.HasValue ? dictionary[""{ property }""].DoubleValue.Value : (double?)dictionary[""{ property }""].FloatValue.Value : (double?)null;";
                case "int":
                    return $@"this.{ property } = dictionary[""{ property }""].IntegerValue.Value;";
                case "int?":
                    return $@"this.{ property } = dictionary.ContainsKey(""{ property }"") ? dictionary[""{ property }""].IntegerValue.Value : (int?)null;";
                case "uint":
                    return $@"this.{ property } = dictionary[""{ property }""].ElementId.Value;";
                case "uint?":
                    return $@"this.{ property } = dictionary.ContainsKey(""{ property }"") ? dictionary[""{ property }""].ElementId.Value : (uint?)null;";
                case "bool":
                    return $@"this.{ property } = dictionary[""{ property }""].BoolValue.Value;";
                case "bool?":
                    return $@"this.{ property } = dictionary.ContainsKey(""{ property }"") ? dictionary[""{ property }""].BoolValue.Value : (bool?)null;";
                case "string":
                case "string?":
                    return $@"this.{ property } = dictionary.ContainsKey(""{ property }"") ? dictionary[""{ property }""].StringValue : (string)null;";
                case "Vector3":
                    return $@"
            var {property}SubDictionary = new Dictionary<string, LuaValue>(
                dictionary[""{ property }""].TableValue.Select(x => 
                    new KeyValuePair<string, LuaValue>(x.Key.StringValue, x.Value)
                )
            );
            this.{ property } = new Vector3(
                {property}SubDictionary[""X""].FloatValue.Value, 
                {property}SubDictionary[""Y""].FloatValue.Value, 
                {property}SubDictionary[""Z""].FloatValue.Value);";
            case "Vector3?":
                    return $@"
            if (dictionary.ContainsKey(""{ property }""))
            {{
                var {property}SubDictionary = new Dictionary<string, LuaValue>(
                    dictionary[""{ property }""].TableValue.Select(x => 
                        new KeyValuePair<string, LuaValue>(x.Key.StringValue, x.Value)
                    )
                );
                this.{ property } = new Vector3(
                    {property}SubDictionary[""X""].FloatValue.Value, 
                    {property}SubDictionary[""Y""].FloatValue.Value, 
                    {property}SubDictionary[""Z""].FloatValue.Value);
            }}";
                default:
                    return "";
            }
        }
    }
}
