using SlipeServer.Server.Elements;
using System;

namespace SlipeServer.Scripting.Definitions;

public class PlayerLoggedInEventArgs(Player player, AccountHandle previousAccount, AccountHandle account) : EventArgs
{
    public Player Player { get; } = player;
    public AccountHandle PreviousAccount { get; } = previousAccount;
    public AccountHandle Account { get; } = account;
}
