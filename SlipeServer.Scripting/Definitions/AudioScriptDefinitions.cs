using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using System.Collections.Generic;

namespace SlipeServer.Scripting.Definitions;

public class AudioScriptDefinitions(IElementCollection elementCollection)
{
    [ScriptFunctionDefinition("playSoundFrontEnd")]
    public bool PlaySoundFrontEnd(ElementTarget target, int sound)
    {
        var players = GetTargetPlayers(target);
        foreach (var player in players)
            player.PlaySound((byte)sound);
        return true;
    }

    private IEnumerable<Player> GetTargetPlayers(ElementTarget target)
    {
        if (target.Players != null)
            return target.Players;
        if (target.Element is Player player)
            return [player];
        return elementCollection.GetByType<Player>();
    }
}
