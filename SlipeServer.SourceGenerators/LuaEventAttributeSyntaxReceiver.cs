using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

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
                    if (attributes.Attributes.Any(x => x.Name.ToString() == "LuaValue"))
                        this.EventClasses.Add(cds);
                }
            }
        }
    }
}
