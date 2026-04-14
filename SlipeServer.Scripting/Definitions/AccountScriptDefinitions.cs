using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Scripting.Definitions;

public class AccountScriptDefinitions(IAccountService accountService)
{
    [ScriptFunctionDefinition("addAccount")]
    public AccountHandle AddAccount(string name, string password, bool allowCaseVariations = false)
    {
        return accountService.AddAccount(name, password, allowCaseVariations);
    }

    [ScriptFunctionDefinition("getAccount")]
    public AccountHandle? GetAccount(string username, string? password = null, bool caseSensitive = true)
    {
        return accountService.GetAccount(username, password, caseSensitive);
    }

    [ScriptFunctionDefinition("getAccountByID")]
    public AccountHandle? GetAccountByID(int id)
    {
        return accountService.GetAccountByID(id);
    }

    [ScriptFunctionDefinition("getAccounts")]
    public IEnumerable<AccountHandle> GetAccounts()
    {
        return accountService.GetAllAccounts();
    }

    [ScriptFunctionDefinition("getAccountsBySerial")]
    public IEnumerable<AccountHandle> GetAccountsBySerial(string serial)
    {
        return accountService.GetAccountsBySerial(serial);
    }

    [ScriptFunctionDefinition("getAccountsByIP")]
    public IEnumerable<AccountHandle> GetAccountsByIP(string ip)
    {
        return accountService.GetAccountsByIP(ip);
    }

    [ScriptFunctionDefinition("getAccountsByData")]
    public IEnumerable<AccountHandle> GetAccountsByData(string dataName, string value)
    {
        return accountService.GetAccountsByData(dataName, value);
    }

    [ScriptFunctionDefinition("removeAccount")]
    public bool RemoveAccount(AccountHandle account)
    {
        if (account.IsGuest)
            return false;
        return accountService.RemoveAccount(account);
    }

    [ScriptFunctionDefinition("setAccountPassword")]
    public bool SetAccountPassword(AccountHandle account, string password)
    {
        if (account.IsGuest)
            return false;
        return accountService.SetAccountPassword(account, password);
    }

    [ScriptFunctionDefinition("setAccountName")]
    public bool SetAccountName(AccountHandle account, string name, bool allowCaseVariations = false)
    {
        if (account.IsGuest)
            return false;
        return accountService.SetAccountName(account, name, allowCaseVariations);
    }

    [ScriptFunctionDefinition("getAccountName")]
    public string GetAccountName(AccountHandle account)
    {
        return account.Name;
    }

    [ScriptFunctionDefinition("getAccountID")]
    public int GetAccountID(AccountHandle account)
    {
        return account.Id;
    }

    [ScriptFunctionDefinition("getAccountSerial")]
    public string GetAccountSerial(AccountHandle account)
    {
        return account.Serial ?? string.Empty;
    }

    [ScriptFunctionDefinition("getAccountIP")]
    public string GetAccountIP(AccountHandle account)
    {
        return account.Ip ?? string.Empty;
    }

    [ScriptFunctionDefinition("getAccountType")]
    public string GetAccountType(AccountHandle account)
    {
        return account.IsGuest ? "guest" : "registered";
    }

    [ScriptFunctionDefinition("isGuestAccount")]
    public bool IsGuestAccount(AccountHandle account)
    {
        return account.IsGuest;
    }

    [ScriptFunctionDefinition("getAccountData")]
    public LuaValue GetAccountData(AccountHandle account, string key)
    {
        if (account.IsGuest)
            return account.GuestData.TryGetValue(key, out var guestValue) ? guestValue : LuaValue.Nil;
        return accountService.GetAccountData(account, key);
    }

    [ScriptFunctionDefinition("setAccountData")]
    public bool SetAccountData(AccountHandle account, string key, LuaValue value)
    {
        if (account.IsGuest)
        {
            if (value.IsNil || (value.BoolValue.HasValue && !value.BoolValue.Value))
                account.GuestData.Remove(key);
            else
                account.GuestData[key] = value;
            return true;
        }
        return accountService.SetAccountData(account, key, value);
    }

    [ScriptFunctionDefinition("getAllAccountData")]
    public Dictionary<string, string?> GetAllAccountData(AccountHandle account)
    {
        if (account.IsGuest)
        {
            var result = new Dictionary<string, string?>(System.StringComparer.Ordinal);
            foreach (var (k, v) in account.GuestData)
                result[k] = v.StringValue ?? v.IntegerValue?.ToString() ?? v.DoubleValue?.ToString() ?? v.FloatValue?.ToString() ?? v.BoolValue?.ToString().ToLowerInvariant();
            return result;
        }
        return accountService.GetAllAccountData(account);
    }

    [ScriptFunctionDefinition("copyAccountData")]
    public bool CopyAccountData(AccountHandle account, AccountHandle fromAccount)
    {
        if (fromAccount.IsGuest)
        {
            foreach (var (key, value) in fromAccount.GuestData)
                SetAccountData(account, key, value);
        }
        else
        {
            accountService.CopyAccountData(account, fromAccount);
        }
        return true;
    }

    [ScriptFunctionDefinition("getPlayerAccount")]
    public AccountHandle GetPlayerAccount(Player player)
        => accountService.GetPlayerAccount(player);

    [ScriptFunctionDefinition("getAccountPlayer")]
    public Player? GetAccountPlayer(AccountHandle account)
        => accountService.GetAccountPlayer(account);

    [ScriptFunctionDefinition("logIn")]
    public bool LogIn(Player player, AccountHandle account, string password)
        => accountService.LogIn(player, account, password);

    [ScriptFunctionDefinition("logOut")]
    public bool LogOut(Player player)
        => accountService.LogOut(player);
}
