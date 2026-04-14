using SlipeServer.Server.Elements;
using System;

namespace SlipeServer.Scripting.Definitions;

public class PlayerLoggedOutEventArgs(Player player, AccountHandle previousAccount, AccountHandle guestAccount) : EventArgs
{
    public Player Player { get; } = player;
    public AccountHandle PreviousAccount { get; } = previousAccount;
    public AccountHandle GuestAccount { get; } = guestAccount;
}
