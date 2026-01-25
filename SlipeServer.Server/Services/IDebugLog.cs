using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using System.Drawing;

namespace SlipeServer.Server.Services;

public interface IDebugLog
{
    void Output(string message, DebugLevel level = DebugLevel.Information, Color? color = null);
    void OutputTo(Player player, string message, DebugLevel level = DebugLevel.Information, Color? color = null);
    void SetVisible(bool isVisible);
    void SetVisibleTo(Player player, bool isVisible);
}