using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements;
using System;
using System.Collections.Generic;

namespace SlipeServer.Scripting.Definitions;

public interface IAccountService
{
    AccountHandle AddAccount(string name, string password, bool allowCaseVariations = false);
    AccountHandle? GetAccount(string username, string? password = null, bool caseSensitive = true);
    AccountHandle? GetAccountByID(int id);
    IEnumerable<AccountHandle> GetAllAccounts();
    IEnumerable<AccountHandle> GetAccountsBySerial(string serial);
    IEnumerable<AccountHandle> GetAccountsByIP(string ip);
    IEnumerable<AccountHandle> GetAccountsByData(string key, string value);
    bool RemoveAccount(AccountHandle account);
    bool SetAccountPassword(AccountHandle account, string password);
    bool SetAccountName(AccountHandle account, string name, bool allowCaseVariations = false);
    LuaValue GetAccountData(AccountHandle account, string key);
    bool SetAccountData(AccountHandle account, string key, LuaValue value);
    Dictionary<string, string?> GetAllAccountData(AccountHandle account);
    bool CopyAccountData(AccountHandle account, AccountHandle fromAccount);
    bool VerifyPassword(AccountHandle account, string password);
    void UpdateSerial(AccountHandle account, string? serial);
    void UpdateIp(AccountHandle account, string? ip);

    AccountHandle GetPlayerAccount(Player player);
    Player? GetAccountPlayer(AccountHandle account);
    bool LogIn(Player player, AccountHandle account, string password);
    bool LogOut(Player player);

    event EventHandler<AccountEventArgs>? AccountCreated;
    event EventHandler<AccountEventArgs>? AccountRemoved;
    event EventHandler<AccountDataChangedEventArgs>? AccountDataChanged;
    event EventHandler<PlayerLoggedInEventArgs>? PlayerLoggedIn;
    event EventHandler<PlayerLoggedOutEventArgs>? PlayerLoggedOut;
}
