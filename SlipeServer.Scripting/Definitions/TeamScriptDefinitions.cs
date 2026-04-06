using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using System.Drawing;
using System.Linq;

namespace SlipeServer.Scripting.Definitions;

public class TeamScriptDefinitions(MtaServer server, IElementCollection elementCollection)
{
    [ScriptFunctionDefinition("createTeam")]
    public Team CreateTeam(string teamName, int colorR = 235, int colorG = 221, int colorB = 178)
    {
        var team = new Team(teamName, Color.FromArgb(255, colorR, colorG, colorB)).AssociateWith(server);

        if (ScriptExecutionContext.Current?.Owner != null)
            team.Parent = ScriptExecutionContext.Current.Owner?.DynamicRoot;

        return team;
    }

    [ScriptFunctionDefinition("getTeamColor")]
    public Color GetTeamColor(Team team) => team.Color;

    [ScriptFunctionDefinition("getTeamFriendlyFire")]
    public bool GetTeamFriendlyFire(Team team) => team.IsFriendlyFireEnabled;

    [ScriptFunctionDefinition("getTeamFromName")]
    public Team? GetTeamFromName(string teamName)
    {
        return elementCollection
            .GetByType<Team>()
            .FirstOrDefault(t => t.TeamName == teamName);
    }

    [ScriptFunctionDefinition("getTeamName")]
    public string GetTeamName(Team team) => team.TeamName;

    [ScriptFunctionDefinition("setTeamColor")]
    public bool SetTeamColor(Team team, int colorR, int colorG, int colorB)
    {
        team.Color = Color.FromArgb(255, colorR, colorG, colorB);
        return true;
    }

    [ScriptFunctionDefinition("setTeamFriendlyFire")]
    public bool SetTeamFriendlyFire(Team team, bool friendlyFire)
    {
        team.IsFriendlyFireEnabled = friendlyFire;
        return true;
    }

    [ScriptFunctionDefinition("setTeamName")]
    public bool SetTeamName(Team team, string teamName)
    {
        team.TeamName = teamName;
        return true;
    }
}
