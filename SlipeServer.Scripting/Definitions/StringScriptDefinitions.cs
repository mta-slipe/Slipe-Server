using SlipeServer.Packets.Definitions.Lua;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SlipeServer.Scripting.Definitions;

public readonly record struct KeyPairResult(string PublicKey, string PrivateKey);

public class StringScriptDefinitions
{
    [ScriptFunctionDefinition("split")]
    public IEnumerable<string> Split(string text, LuaValue separatingChar)
    {
        string sep = GetSeparator(separatingChar);
        if (string.IsNullOrEmpty(sep))
            return [text];

        return text.Split(sep, StringSplitOptions.None);
    }

    [ScriptFunctionDefinition("gettok")]
    public string? GetTok(string text, int tokenNumber, LuaValue separatingCharacter)
    {
        var parts = Split(text, separatingCharacter);
        int index = 0;
        foreach (var part in parts)
        {
            index++;
            if (index == tokenNumber)
                return part;
        }
        return null;
    }

    private static string GetSeparator(LuaValue sep)
    {
        if (sep.IntegerValue.HasValue)
            return ((char)sep.IntegerValue.Value).ToString();
        if (sep.DoubleValue.HasValue)
            return ((char)(int)sep.DoubleValue.Value).ToString();
        if (sep.FloatValue.HasValue)
            return ((char)(int)sep.FloatValue.Value).ToString();
        if (sep.StringValue != null)
            return sep.StringValue;
        return string.Empty;
    }

    private static string? GetOption(LuaValue? opts, string key)
    {
        if (opts == null || opts.IsNil || opts.TableValue == null) 
            return null;

        var k = new LuaValue(key);
        return opts.TableValue.TryGetValue(k, out var val) ? val.StringValue : null;
    }

    private static RegexOptions ParseFlags(string? flags)
    {
        var options = RegexOptions.None;
        if (flags == null) return options;
        if (flags.Contains('i', StringComparison.OrdinalIgnoreCase)) options |= RegexOptions.IgnoreCase;
        if (flags.Contains('m', StringComparison.OrdinalIgnoreCase)) options |= RegexOptions.Multiline;
        if (flags.Contains('s', StringComparison.OrdinalIgnoreCase)) options |= RegexOptions.Singleline;
        if (flags.Contains('x', StringComparison.OrdinalIgnoreCase)) options |= RegexOptions.IgnorePatternWhitespace;
        return options;
    }


    [ScriptFunctionDefinition("pregFind")]
    public bool PregFind(string subject, string pattern, string? flags = null)
    {
        return Regex.IsMatch(subject, pattern, ParseFlags(flags));
    }

    [ScriptFunctionDefinition("pregMatch")]
    public IEnumerable<string> PregMatch(string subject, string pattern, string? flags = null, int maxResults = 100000)
    {
        var options = ParseFlags(flags);
        var matches = Regex.Matches(subject, pattern, options);
        var results = new List<string>();

        foreach (Match match in matches)
        {
            if (results.Count >= maxResults) break;
            results.Add(match.Value);
        }

        return results;
    }

    [ScriptFunctionDefinition("pregReplace")]
    public string PregReplace(string subject, string pattern, string replacement, string? flags = null)
    {
        return Regex.Replace(subject, pattern, replacement, ParseFlags(flags));
    }
    

    [ScriptFunctionDefinition("utfChar")]
    public string UtfChar(int characterCode)
    {
        return char.ConvertFromUtf32(characterCode);
    }

    [ScriptFunctionDefinition("utfCode")]
    public int UtfCode(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        return char.ConvertToUtf32(text, 0);
    }

    [ScriptFunctionDefinition("utfLen")]
    public int UtfLen(string text)
    {
        return new StringInfo(text).LengthInTextElements;
    }

    [ScriptFunctionDefinition("utfSeek")]
    public int UtfSeek(string text, int position)
    {
        var info = new StringInfo(text);
        if (position < 1 || position > info.LengthInTextElements)
            return 0;

        int bytePos = 0;
        for (int i = 0; i < position - 1; i++)
            bytePos += StringInfo.GetNextTextElement(text, bytePos).Length;

        return bytePos + 1;
    }

    [ScriptFunctionDefinition("utfSub")]
    public string UtfSub(string text, int start, int end)
    {
        var info = new StringInfo(text);
        int length = info.LengthInTextElements;
        if (start < 0) 
            start = Math.Max(1, length + start + 1);

        if (end < 0) 
            end = length + end + 1;

        start = Math.Max(1, start);
        end = Math.Min(length, end);
        if (start > end) 
            return string.Empty;

        var sb = new StringBuilder();
        int pos = 0;
        for (int i = 0; i < end; i++)
        {
            var elem = StringInfo.GetNextTextElement(text, pos);
            if (i >= start - 1) 
                sb.Append(elem);
            pos += elem.Length;
        }
        return sb.ToString();
    }


    [ScriptFunctionDefinition("encodeString")]
    public string? EncodeString(string algorithm, string input, LuaValue? options = null)
    {
        return algorithm.ToLowerInvariant() switch
        {
            "base64" => Convert.ToBase64String(Encoding.UTF8.GetBytes(input)),
            "tea" => TeaEncodeInternal(input, GetOption(options, "key") ?? string.Empty),
            "aes128" => Aes128Encode(input, options),
            _ => null
        };
    }

    [ScriptFunctionDefinition("decodeString")]
    public string? DecodeString(string algorithm, string input, LuaValue? options = null)
    {
        return algorithm.ToLowerInvariant() switch
        {
            "base64" => Encoding.UTF8.GetString(Convert.FromBase64String(input)),
            "tea" => TeaDecodeInternal(input, GetOption(options, "key") ?? string.Empty),
            "aes128" => Aes128Decode(input, options),
            _ => null
        };
    }


    [ScriptFunctionDefinition("teaEncode")]
    public string TeaEncode(string text, string key)
    {
        return TeaEncodeInternal(text, key);
    }

    [ScriptFunctionDefinition("teaDecode")]
    public string TeaDecode(string data, string key)
    {
        return TeaDecodeInternal(data, key);
    }

    private static string TeaEncodeInternal(string text, string key)
    {
        byte[] keyBytes = DeriveTeaKey(key);
        byte[] data = Encoding.UTF8.GetBytes(text);
        byte[] encrypted = XxteaEncrypt(data, keyBytes);
        return Convert.ToBase64String(encrypted);
    }

    private static string TeaDecodeInternal(string data, string key)
    {
        byte[] keyBytes = DeriveTeaKey(key);
        byte[] encrypted = Convert.FromBase64String(data);
        byte[] decrypted = XxteaDecrypt(encrypted, keyBytes);
        return Encoding.UTF8.GetString(decrypted);
    }

    private static byte[] DeriveTeaKey(string key)
    {
        byte[] raw = Encoding.UTF8.GetBytes(key);
        byte[] result = new byte[16];
        for (int i = 0; i < raw.Length && i < 16; i++)
            result[i] = raw[i];
        return result;
    }

    private const uint XxteaDelta = 0x9E3779B9;

    private static uint XxteaMX(uint sum, uint y, uint z, int p, uint e, uint[] k)
        => (((z >> 5) ^ (y << 2)) + ((y >> 3) ^ (z << 4))) ^ ((sum ^ y) + (k[(p & 3) ^ e] ^ z));

    private static byte[] XxteaEncrypt(byte[] data, byte[] keyBytes)
    {
        if (data.Length == 0) return data;
        uint[] v = ToUInt32Array(data, true);
        uint[] k = ToUInt32Array(keyBytes, false);
        int n = v.Length - 1;
        uint z = v[n], y, sum = 0, e;
        int q = 6 + 52 / (n + 1);
        while (q-- > 0)
        {
            sum += XxteaDelta;
            e = (sum >> 2) & 3;
            for (int p = 0; p < n; p++)
            {
                y = v[p + 1];
                v[p] += XxteaMX(sum, y, z, p, e, k);
                z = v[p];
            }
            y = v[0];
            v[n] += XxteaMX(sum, y, z, n, e, k);
            z = v[n];
        }
        return ToByteArray(v, false);
    }

    private static byte[] XxteaDecrypt(byte[] data, byte[] keyBytes)
    {
        if (data.Length == 0) return data;
        uint[] v = ToUInt32Array(data, false);
        uint[] k = ToUInt32Array(keyBytes, false);
        int n = v.Length - 1;
        uint z, y = v[0], e;
        int q = 6 + 52 / (n + 1);
        uint sum = (uint)(q * XxteaDelta);
        while (sum != 0)
        {
            e = (sum >> 2) & 3;
            for (int p = n; p > 0; p--)
            {
                z = v[p - 1];
                v[p] -= XxteaMX(sum, y, z, p, e, k);
                y = v[p];
            }
            z = v[n];
            v[0] -= XxteaMX(sum, y, z, 0, e, k);
            y = v[0];
            sum -= XxteaDelta;
        }
        return ToByteArray(v, true);
    }

    private static uint[] ToUInt32Array(byte[] data, bool includeLength)
    {
        int n = ((data.Length & 3) == 0) ? (data.Length >> 2) : ((data.Length >> 2) + 1);
        uint[] result = includeLength ? new uint[n + 1] : new uint[n];
        if (includeLength) result[n] = (uint)data.Length;
        for (int i = 0; i < data.Length; i++)
            result[i >> 2] |= (uint)data[i] << ((i & 3) << 3);
        return result;
    }

    private static byte[] ToByteArray(uint[] data, bool includeLength)
    {
        int n = data.Length << 2;
        if (includeLength)
        {
            int m = (int)data[data.Length - 1];
            n -= 4;
            if (m < n - 3 || m > n) throw new InvalidOperationException("XXTEA: invalid length");
            n = m;
        }
        byte[] result = new byte[n];
        for (int i = 0; i < n; i++)
            result[i] = (byte)((data[i >> 2] >> ((i & 3) << 3)) & 0xFF);
        return result;
    }


    private static string? Aes128Encode(string input, LuaValue? options)
    {
        var keyStr = GetOption(options, "key");
        if (keyStr == null) return null;
        byte[] key = PadOrTruncate(Encoding.UTF8.GetBytes(keyStr), 16);
        var ivStr = GetOption(options, "iv");
        byte[] iv = ivStr != null ? PadOrTruncate(Encoding.UTF8.GetBytes(ivStr), 16) : new byte[16];
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        using var encryptor = aes.CreateEncryptor();
        byte[] data = Encoding.UTF8.GetBytes(input);
        byte[] encrypted = encryptor.TransformFinalBlock(data, 0, data.Length);
        return Convert.ToBase64String(encrypted);
    }

    private static string? Aes128Decode(string input, LuaValue? options)
    {
        var keyStr = GetOption(options, "key");
        if (keyStr == null) return null;
        byte[] key = PadOrTruncate(Encoding.UTF8.GetBytes(keyStr), 16);
        var ivStr = GetOption(options, "iv");
        byte[] iv = ivStr != null ? PadOrTruncate(Encoding.UTF8.GetBytes(ivStr), 16) : new byte[16];
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        using var decryptor = aes.CreateDecryptor();
        byte[] data = Convert.FromBase64String(input);
        byte[] decrypted = decryptor.TransformFinalBlock(data, 0, data.Length);
        return Encoding.UTF8.GetString(decrypted);
    }

    private static byte[] PadOrTruncate(byte[] input, int length)
    {
        var result = new byte[length];
        Buffer.BlockCopy(input, 0, result, 0, Math.Min(input.Length, length));
        return result;
    }


    [ScriptFunctionDefinition("toJSON")]
    public string ToJson(LuaValue value, bool compact = false, string prettyType = "none")
    {
        var options = new JsonSerializerOptions { WriteIndented = prettyType != "none" && !compact };
        return JsonSerializer.Serialize(LuaValueToJsonObject(value), options);
    }

    [ScriptFunctionDefinition("fromJSON")]
    public LuaValue FromJson(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return JsonElementToLuaValue(doc.RootElement);
        }
        catch
        {
            return LuaValue.Nil;
        }
    }

    private static object? LuaValueToJsonObject(LuaValue value)
    {
        if (value.IsNil) return null;
        if (value.BoolValue.HasValue) return value.BoolValue.Value;
        if (value.StringValue != null) return value.StringValue;
        if (value.IntegerValue.HasValue) return value.IntegerValue.Value;
        if (value.FloatValue.HasValue) return (double)value.FloatValue.Value;
        if (value.DoubleValue.HasValue) return value.DoubleValue.Value;
        if (value.TableValue != null)
        {
            var dict = new Dictionary<string, object?>();
            foreach (var kvp in value.TableValue)
                dict[kvp.Key.StringValue ?? kvp.Key.IntegerValue?.ToString() ?? ""] = LuaValueToJsonObject(kvp.Value);
            return dict;
        }
        return null;
    }

    private static LuaValue JsonElementToLuaValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => new LuaValue(element.EnumerateObject().ToDictionary(
                p => new LuaValue(p.Name),
                p => JsonElementToLuaValue(p.Value))),
            JsonValueKind.Array => new LuaValue(element.EnumerateArray()
                .Select((v, i) => (Key: new LuaValue(i + 1), Val: JsonElementToLuaValue(v)))
                .ToDictionary(x => x.Key, x => x.Val)),
            JsonValueKind.String => new LuaValue(element.GetString() ?? string.Empty),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? new LuaValue((double?)l) : new LuaValue((double?)element.GetDouble()),
            JsonValueKind.True => new LuaValue(true),
            JsonValueKind.False => new LuaValue(false),
            _ => LuaValue.Nil
        };
    }


    [ScriptFunctionDefinition("hash")]
    public string Hash(string algorithm, string data, LuaValue? options = null)
    {
        byte[] input = Encoding.UTF8.GetBytes(data);
        byte[] hashBytes = algorithm.ToLowerInvariant() switch
        {
            "md5" => MD5.HashData(input),
            "sha1" => SHA1.HashData(input),
            "sha224" => SHA256.HashData(input)[..28],
            "sha256" => SHA256.HashData(input),
            "sha384" => SHA384.HashData(input),
            "sha512" => SHA512.HashData(input),
            _ => throw new ArgumentException($"Unsupported hash algorithm: {algorithm}")
        };
        return Convert.ToHexString(hashBytes).ToUpperInvariant();
    }

    [ScriptFunctionDefinition("passwordHash")]
    public string PasswordHash(string password, string algorithm, LuaValue? options = null)
    {
        return algorithm.ToLowerInvariant() switch
        {
            "bcrypt" => BCrypt.Net.BCrypt.HashPassword(password, GetOption(options, "cost") is { } cost ? int.Parse(cost) : 10),
            "sha512" => Convert.ToHexString(SHA512.HashData(Encoding.UTF8.GetBytes(password))).ToLowerInvariant(),
            _ => throw new ArgumentException($"Unsupported password algorithm: {algorithm}")
        };
    }

    [ScriptFunctionDefinition("passwordVerify")]
    public bool PasswordVerify(string password, string hash, LuaValue? options = null)
    {
        try
        {
            if (hash.StartsWith("$2"))
                return BCrypt.Net.BCrypt.Verify(password, hash);
            return string.Equals(Convert.ToHexString(SHA512.HashData(Encoding.UTF8.GetBytes(password))), hash, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }


    [ScriptFunctionDefinition("generateKeyPair")]
    public KeyPairResult GenerateKeyPair(string algorithm, LuaValue? options = null)
    {
        int keySize = GetOption(options, "size") is { } sizeStr ? int.Parse(sizeStr) : 2048;
        using var rsa = RSA.Create(keySize);
        string pub = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo());
        string priv = Convert.ToBase64String(rsa.ExportPkcs8PrivateKey());
        return new KeyPairResult(pub, priv);
    }
}
