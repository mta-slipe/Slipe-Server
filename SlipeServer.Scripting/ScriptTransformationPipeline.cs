using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SlipeServer.Scripting;

public interface IScriptTransform
{
    Stream Transform(Stream data, string lang);
}

public sealed class ScriptTransformationPipeline
{
    private readonly List<IScriptTransform> scriptTransforms;

    public ScriptTransformationPipeline(IEnumerable<IScriptTransform> scriptTransforms)
    {
        this.scriptTransforms = scriptTransforms.ToList();
    }

    public void Add(IScriptTransform scriptTransform)
    {
        this.scriptTransforms.Add(scriptTransform);
    }

    public Stream Transform(Stream data, string lang)
    {
        var transformedData = data;

        foreach (var transformation in this.scriptTransforms)
        {
            transformedData = transformation.Transform(transformedData, lang);
            transformedData.Position = 0;
        }

        return transformedData;

    }
}
