using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Text;

namespace SlipeServer.SourceGenerators
{
    [Generator]
    public class LuaValueSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new LuaEventAttributeSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            LuaEventAttributeSyntaxReceiver syntaxReceiver = (LuaEventAttributeSyntaxReceiver)context.SyntaxReceiver!;

            foreach (var eventClass in syntaxReceiver.EventClasses)
            {
                var sourceText = SourceText.From(GenerateParsePartialClass(eventClass), Encoding.UTF8);
                context.AddSource($"Generated{eventClass.Identifier.ValueText}", sourceText);
            }
        }

        private string GenerateParsePartialClass(ClassDeclarationSyntax eventClass)
        {
            ;
            return $@"using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using SlipeServer.Server.Events;
using SlipeServer.Packets.Definitions.Lua;
using System.ComponentModel.DataAnnotations;

namespace {(eventClass.Parent as NamespaceDeclarationSyntax)!.Name.ToFullString()}
{{
    public partial class {eventClass.Identifier.ValueText}
    {{
        public partial void Parse(LuaValue luaValue)
        {{
            var dictionary = new Dictionary<string, LuaValue>(
                luaValue.TableValue!.Select(x => 
                    new KeyValuePair<string, LuaValue>(x.Key.StringValue!, x.Value)
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
                .Select(x => (x as PropertyDeclarationSyntax)!);

            foreach (var property in properties)
            {
                var type = property.Type.ToString();
                var name = property.Identifier.ValueText;

                builder.Append($"            {GetPropertyParser(type, name)}\n");
            }

            //System.Diagnostics.Debugger.Launch();
            var code = builder.ToString();
            return builder.ToString();
        }

        private string GetPropertyParser(string type, string property)
        {
            var isArray = type.EndsWith("[]");

            string snippet = type.TrimEnd(']').TrimEnd('[') switch
            {
                "float" => LuaEventSourceGeneratorSnippets.GetFloat,
                "float?" => LuaEventSourceGeneratorSnippets.GetOptionalFloat,
                "double" => LuaEventSourceGeneratorSnippets.GetDouble,
                "double?" => LuaEventSourceGeneratorSnippets.GetOptionalDouble,
                "int" => LuaEventSourceGeneratorSnippets.GetInt,
                "int?" => LuaEventSourceGeneratorSnippets.GetOptionalInt,
                "uint" => LuaEventSourceGeneratorSnippets.GetUInt,
                "uint?" => LuaEventSourceGeneratorSnippets.GetOptionalUInt,
                "bool" => LuaEventSourceGeneratorSnippets.GetBool,
                "bool?" => LuaEventSourceGeneratorSnippets.GetOptionalBool,
                "string" => LuaEventSourceGeneratorSnippets.GetString,
                "string?" => LuaEventSourceGeneratorSnippets.GetOptionalString,
                "Vector3" => LuaEventSourceGeneratorSnippets.GetVector3,
                "Vector3?" => LuaEventSourceGeneratorSnippets.GetOptionalVector3,
                _ => type.EndsWith("?") ?
                    LuaEventSourceGeneratorSnippets.GetOptionalLuaValue :
                    LuaEventSourceGeneratorSnippets.GetLuaValue
            };

            if (isArray)
                return LuaEventSourceGeneratorSnippets.FormatForArray(snippet, type, property);

            return LuaEventSourceGeneratorSnippets.Format(snippet, type, property);
        }
    }
}
