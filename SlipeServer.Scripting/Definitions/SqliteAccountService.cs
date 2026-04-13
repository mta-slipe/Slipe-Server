using Microsoft.Data.Sqlite;
using SlipeServer.Packets.Definitions.Lua;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace SlipeServer.Scripting.Definitions;

public class SqliteAccountService : IAccountService, IDisposable
{
    private readonly SqliteConnection connection;
    private readonly Lock dbLock = new();

    // MTA password format: sha256_hex(64) + type(1) + salt_hex(32) = 97 chars
    // type "0" = plain text, type "1" = MD5 (legacy)
    private static string HashPassword(string plaintext)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var saltHex = Convert.ToHexString(saltBytes).ToLowerInvariant();
        var sha256Hex = ComputeSha256Hex(saltHex + plaintext);
        return sha256Hex + "0" + saltHex;
    }

    private static bool VerifyMtaPassword(string plaintext, string stored)
    {
        if (stored.Length == 97)
        {
            var sha256Hex = stored[..64];
            var type = stored[64];
            var saltHex = stored[65..];
            var input = type == '1'
                ? saltHex + ComputeMd5Hex(plaintext).ToUpperInvariant()
                : saltHex + plaintext;
            return ComputeSha256Hex(input) == sha256Hex;
        }
        // Legacy: plain MD5 (32 hex chars)
        if (stored.Length == 32)
            return ComputeMd5Hex(plaintext).Equals(stored, StringComparison.OrdinalIgnoreCase);
        // Plain text fallback
        return stored == plaintext;
    }

    private static string ComputeSha256Hex(string input)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string ComputeMd5Hex(string input)
    {
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    // Lua type constants: 0=nil, 1=boolean, 3=number, 4=string
    private static (string? value, int luaType) LuaValueToStorable(LuaValue value)
    {
        if (value.IsNil) 
            return (null, 0);
        if (value.BoolValue.HasValue) 
            return (value.BoolValue.Value ? "true" : "false", 1);
        if (value.IntegerValue.HasValue) 
            return (value.IntegerValue.Value.ToString(CultureInfo.InvariantCulture), 3);
        if (value.FloatValue.HasValue) 
            return (value.FloatValue.Value.ToString(CultureInfo.InvariantCulture), 3);
        if (value.DoubleValue.HasValue) 
            return (value.DoubleValue.Value.ToString(CultureInfo.InvariantCulture), 3);
        if (value.StringValue != null) 
            return (value.StringValue, 4);

        return (null, 0);
    }

    private static LuaValue StorableToLuaValue(string storedValue, int luaType)
    {
        return luaType switch
        {
            1 => new LuaValue(storedValue == "true"),
            3 => long.TryParse(storedValue, out var l) ? new LuaValue(l)
                 : double.TryParse(storedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? new LuaValue(d)
                 : LuaValue.Nil,
            _ => new LuaValue(storedValue),
        };
    }

    public SqliteAccountService(string dataSource = "internal.db")
    {
        this.connection = new SqliteConnection($"Data Source={dataSource}");
        this.connection.Open();
        InitializeSchema();
    }

    private void InitializeSchema()
    {
        lock (this.dbLock)
        {
            using var cmd = this.connection.CreateCommand();
            cmd.CommandText = """
                CREATE TABLE IF NOT EXISTS accounts (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT UNIQUE NOT NULL COLLATE NOCASE,
                    ip TEXT NOT NULL DEFAULT '',
                    serial TEXT NOT NULL DEFAULT '',
                    password TEXT NOT NULL DEFAULT '',
                    httppass TEXT NOT NULL DEFAULT ''
                );
                CREATE TABLE IF NOT EXISTS userdata (
                    userid INTEGER NOT NULL,
                    key TEXT NOT NULL,
                    value TEXT NOT NULL DEFAULT '',
                    type INTEGER NOT NULL DEFAULT 4,
                    PRIMARY KEY (userid, key),
                    FOREIGN KEY (userid) REFERENCES accounts(id) ON DELETE CASCADE
                );
                """;
            cmd.ExecuteNonQuery();
        }
    }

    public AccountHandle AddAccount(string name, string password, bool allowCaseVariations = false)
    {
        var hash = HashPassword(password);
        lock (this.dbLock)
        {
            using var cmd = this.connection.CreateCommand();
            cmd.CommandText = "INSERT INTO accounts (name, password) VALUES (@name, @password); SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@password", hash);
            var id = Convert.ToInt32(cmd.ExecuteScalar()!);
            var account = new AccountHandle(id, name, null, null);
            this.AccountCreated?.Invoke(this, new AccountEventArgs(account));
            return account;
        }
    }

    public AccountHandle? GetAccount(string username, string? password = null, bool caseSensitive = true)
    {
        lock (this.dbLock)
        {
            using var cmd = this.connection.CreateCommand();
            cmd.CommandText = caseSensitive
                ? "SELECT id, name, password, serial, ip FROM accounts WHERE name = @name"
                : "SELECT id, name, password, serial, ip FROM accounts WHERE name = @name COLLATE NOCASE";
            cmd.Parameters.AddWithValue("@name", username);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return null;

            var id = reader.GetInt32(0);
            var name = reader.GetString(1);
            var storedHash = reader.GetString(2);
            var serial = NullIfEmpty(reader.GetString(3));
            var ip = NullIfEmpty(reader.GetString(4));

            if (password != null && !VerifyMtaPassword(password, storedHash))
                return null;

            return new AccountHandle(id, name, serial, ip);
        }
    }

    public AccountHandle? GetAccountByID(int id)
    {
        lock (this.dbLock)
        {
            using var cmd = this.connection.CreateCommand();
            cmd.CommandText = "SELECT id, name, serial, ip FROM accounts WHERE id = @id";
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return null;

            return new AccountHandle(
                reader.GetInt32(0),
                reader.GetString(1),
                NullIfEmpty(reader.GetString(2)),
                NullIfEmpty(reader.GetString(3)));
        }
    }

    public IEnumerable<AccountHandle> GetAllAccounts()
    {
        lock (this.dbLock)
        {
            var results = new List<AccountHandle>();
            using var cmd = this.connection.CreateCommand();
            cmd.CommandText = "SELECT id, name, serial, ip FROM accounts";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add(new AccountHandle(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    NullIfEmpty(reader.GetString(2)),
                    NullIfEmpty(reader.GetString(3))));
            }
            return results;
        }
    }

    public IEnumerable<AccountHandle> GetAccountsBySerial(string serial)
    {
        lock (this.dbLock)
        {
            var results = new List<AccountHandle>();
            using var cmd = this.connection.CreateCommand();
            cmd.CommandText = "SELECT id, name, serial, ip FROM accounts WHERE serial = @serial";
            cmd.Parameters.AddWithValue("@serial", serial);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add(new AccountHandle(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    NullIfEmpty(reader.GetString(2)),
                    NullIfEmpty(reader.GetString(3))));
            }
            return results;
        }
    }

    public IEnumerable<AccountHandle> GetAccountsByIP(string ip)
    {
        lock (this.dbLock)
        {
            var results = new List<AccountHandle>();
            using var cmd = this.connection.CreateCommand();
            cmd.CommandText = "SELECT id, name, serial, ip FROM accounts WHERE ip = @ip";
            cmd.Parameters.AddWithValue("@ip", ip);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add(new AccountHandle(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    NullIfEmpty(reader.GetString(2)),
                    NullIfEmpty(reader.GetString(3))));
            }
            return results;
        }
    }

    public IEnumerable<AccountHandle> GetAccountsByData(string key, string value)
    {
        lock (this.dbLock)
        {
            var results = new List<AccountHandle>();
            using var cmd = this.connection.CreateCommand();
            cmd.CommandText = """
                SELECT a.id, a.name, a.serial, a.ip
                FROM accounts a
                INNER JOIN userdata d ON d.userid = a.id
                WHERE d.key = @key AND d.value = @value
                """;
            cmd.Parameters.AddWithValue("@key", key);
            cmd.Parameters.AddWithValue("@value", value);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add(new AccountHandle(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    NullIfEmpty(reader.GetString(2)),
                    NullIfEmpty(reader.GetString(3))));
            }
            return results;
        }
    }

    public bool RemoveAccount(AccountHandle account)
    {
        lock (this.dbLock)
        {
            using var cmd = this.connection.CreateCommand();
            cmd.CommandText = "DELETE FROM accounts WHERE id = @id";
            cmd.Parameters.AddWithValue("@id", account.Id);
            var removed = cmd.ExecuteNonQuery() > 0;
            if (removed)
                this.AccountRemoved?.Invoke(this, new AccountEventArgs(account));
            return removed;
        }
    }

    public bool SetAccountPassword(AccountHandle account, string password)
    {
        var hash = HashPassword(password);
        lock (this.dbLock)
        {
            using var cmd = this.connection.CreateCommand();
            cmd.CommandText = "UPDATE accounts SET password = @password WHERE id = @id";
            cmd.Parameters.AddWithValue("@password", hash);
            cmd.Parameters.AddWithValue("@id", account.Id);
            return cmd.ExecuteNonQuery() > 0;
        }
    }

    public bool SetAccountName(AccountHandle account, string name, bool allowCaseVariations = false)
    {
        lock (this.dbLock)
        {
            using var checkCmd = this.connection.CreateCommand();
            checkCmd.CommandText = allowCaseVariations
                ? "SELECT COUNT(*) FROM accounts WHERE name = @name AND id != @id"
                : "SELECT COUNT(*) FROM accounts WHERE name = @name COLLATE NOCASE AND id != @id";
            checkCmd.Parameters.AddWithValue("@name", name);
            checkCmd.Parameters.AddWithValue("@id", account.Id);
            var count = Convert.ToInt32(checkCmd.ExecuteScalar());
            if (count > 0)
                return false;

            using var cmd = this.connection.CreateCommand();
            cmd.CommandText = "UPDATE accounts SET name = @name WHERE id = @id";
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@id", account.Id);
            var rows = cmd.ExecuteNonQuery();
            if (rows > 0)
                account.Name = name;
            return rows > 0;
        }
    }

    public LuaValue GetAccountData(AccountHandle account, string key)
    {
        lock (this.dbLock)
        {
            using var cmd = this.connection.CreateCommand();
            cmd.CommandText = "SELECT value, type FROM userdata WHERE userid = @id AND key = @key";
            cmd.Parameters.AddWithValue("@id", account.Id);
            cmd.Parameters.AddWithValue("@key", key);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return LuaValue.Nil;
            return StorableToLuaValue(reader.GetString(0), reader.GetInt32(1));
        }
    }

    public bool SetAccountData(AccountHandle account, string key, LuaValue value)
    {
        var (strValue, luaType) = LuaValueToStorable(value);
        LuaValue oldValue;
        lock (this.dbLock)
        {
            using var getCmd = this.connection.CreateCommand();
            getCmd.CommandText = "SELECT value, type FROM userdata WHERE userid = @id AND key = @key";
            getCmd.Parameters.AddWithValue("@id", account.Id);
            getCmd.Parameters.AddWithValue("@key", key);
            using var reader = getCmd.ExecuteReader();
            oldValue = reader.Read() ? StorableToLuaValue(reader.GetString(0), reader.GetInt32(1)) : LuaValue.Nil;
        }

        lock (this.dbLock)
        {
            using var cmd = this.connection.CreateCommand();
            // Delete on nil type (0) or boolean false (type=1, value="false"), matching MTA behaviour
            if (luaType == 0 || (luaType == 1 && strValue == "false"))
            {
                cmd.CommandText = "DELETE FROM userdata WHERE userid = @id AND key = @key";
                cmd.Parameters.AddWithValue("@id", account.Id);
                cmd.Parameters.AddWithValue("@key", key);
            }
            else
            {
                cmd.CommandText = """
                    INSERT INTO userdata (userid, key, value, type) VALUES (@id, @key, @value, @type)
                    ON CONFLICT(userid, key) DO UPDATE SET value = excluded.value, type = excluded.type
                    """;
                cmd.Parameters.AddWithValue("@id", account.Id);
                cmd.Parameters.AddWithValue("@key", key);
                cmd.Parameters.AddWithValue("@value", strValue ?? "");
                cmd.Parameters.AddWithValue("@type", luaType);
            }
            cmd.ExecuteNonQuery();
        }

        this.AccountDataChanged?.Invoke(this, new AccountDataChangedEventArgs(account, key, value, oldValue));
        return true;
    }

    public Dictionary<string, string?> GetAllAccountData(AccountHandle account)
    {
        lock (this.dbLock)
        {
            var result = new Dictionary<string, string?>(StringComparer.Ordinal);
            using var cmd = this.connection.CreateCommand();
            cmd.CommandText = "SELECT key, value FROM userdata WHERE userid = @id";
            cmd.Parameters.AddWithValue("@id", account.Id);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                result[reader.GetString(0)] = reader.GetString(1);

            return result;
        }
    }

    public bool CopyAccountData(AccountHandle account, AccountHandle fromAccount)
    {
        lock (this.dbLock)
        {
            using var cmd = this.connection.CreateCommand();
            cmd.CommandText = """
                INSERT INTO userdata (userid, key, value, type)
                SELECT @toId, key, value, type FROM userdata WHERE userid = @fromId
                ON CONFLICT(userid, key) DO UPDATE SET value = excluded.value, type = excluded.type
                """;
            cmd.Parameters.AddWithValue("@toId", account.Id);
            cmd.Parameters.AddWithValue("@fromId", fromAccount.Id);
            cmd.ExecuteNonQuery();
        }
        return true;
    }

    public bool VerifyPassword(AccountHandle account, string password)
    {
        lock (this.dbLock)
        {
            using var cmd = this.connection.CreateCommand();
            cmd.CommandText = "SELECT password FROM accounts WHERE id = @id";
            cmd.Parameters.AddWithValue("@id", account.Id);
            var hash = cmd.ExecuteScalar() as string;
            if (hash == null)
                return false;
            return VerifyMtaPassword(password, hash);
        }
    }

    public void UpdateSerial(AccountHandle account, string? serial)
    {
        account.Serial = serial;
        lock (this.dbLock)
        {
            using var cmd = this.connection.CreateCommand();
            cmd.CommandText = "UPDATE accounts SET serial = @serial WHERE id = @id";
            cmd.Parameters.AddWithValue("@serial", serial ?? "");
            cmd.Parameters.AddWithValue("@id", account.Id);
            cmd.ExecuteNonQuery();
        }
    }

    public void UpdateIp(AccountHandle account, string? ip)
    {
        account.Ip = ip;
        lock (this.dbLock)
        {
            using var cmd = this.connection.CreateCommand();
            cmd.CommandText = "UPDATE accounts SET ip = @ip WHERE id = @id";
            cmd.Parameters.AddWithValue("@ip", ip ?? "");
            cmd.Parameters.AddWithValue("@id", account.Id);
            cmd.ExecuteNonQuery();
        }
    }

    private static string? NullIfEmpty(string value) => string.IsNullOrEmpty(value) ? null : value;

    public void Dispose()
    {
        this.connection.Dispose();
    }

    public event EventHandler<AccountEventArgs>? AccountCreated;
    public event EventHandler<AccountEventArgs>? AccountRemoved;
    public event EventHandler<AccountDataChangedEventArgs>? AccountDataChanged;
}
