using System;

namespace SlipeServer.Scripting.Definitions;

public class AccountEventArgs(AccountHandle account) : EventArgs
{
    public AccountHandle Account { get; } = account;
}
