using System.Text;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.CodeAnalysis.Text;

namespace SlipeServer.Scripting.Luau;

internal sealed class LuauToLuaTransform : IScriptTransform
{
    public Stream Transform(Stream data, string lang)
    {
        if (lang != "lua")
            return data;

        SourceText sourceText = SourceText.From(data, Encoding.UTF8);

        var syntaxTree = LuaSyntaxTree.ParseText(sourceText, options: new LuaParseOptions(LuaSyntaxOptions.Luau), path: "script.lua");

        var root = syntaxTree.GetRoot();

        var rewriter = new TypeAnnotationRemover();
        var strippedRoot = rewriter.Visit(root);

        var outData = new MemoryStream();
        var writer = new StreamWriter(outData);
        strippedRoot.WriteTo(writer);
        writer.Flush();
        return outData;
    }

    class TypeAnnotationRemover : LuaSyntaxRewriter
    {
        public override SyntaxNode? Visit(SyntaxNode? node)
        {
            if (node is TypeDeclarationStatementSyntax or TypeBindingSyntax)
                return null;

            return base.Visit(node);
        }
    }
}
