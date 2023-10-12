using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.CodeAnalysis.Text;
using SlipeServer.Server.Resources;
using SlipeServer.Server.Resources.Features;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Console.ResourceFeatures;

public interface IVerifySourceCodeFeature : IResourceFeature
{
}

internal class VerifySourceCodeFeature : IResourceFeatureApplier<IVerifySourceCodeFeature>
{
    public VerifySourceCodeFeature()
    {

    }

    public void Apply(IVerifySourceCodeFeature feature, Resource resource, Dictionary<string, byte[]> files)
    {
        foreach (var item in files)
        {
            if (!item.Key.EndsWith(".lua"))
                continue;

            var sourceText = SourceText.From(item.Value, item.Value.Length);
            var parseOptions = new LuaParseOptions(LuaSyntaxOptions.Luau);
            var syntaxTree = LuaSyntaxTree.ParseText(sourceText, parseOptions, item.Key);

            var errors = new List<string>();

            foreach (var diagnostic in syntaxTree.GetDiagnostics().Where(x => x.Severity == Loretta.CodeAnalysis.DiagnosticSeverity.Error))
            {
                errors.Add(diagnostic.ToString());
            }
            if(errors.Count > 0)
                throw new System.Exception(string.Join("\n", errors));
        }
    }
}
