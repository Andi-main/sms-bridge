using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using SmsBridge.Client.Shared.Messages;

namespace SmsBridge.Client.Shared.Cryptography;

public sealed class AesGcmMessageEncryptionService
    : IMessageEncryptionService
{
    private const int ProtocolVersion = 1;
    private const int KeySize = 32;
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private const string Algorithm = "A256GCM";

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public string Encrypt(
        string plaintext,
        byte[] key,
        string channelId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plaintext);

        ValidateKey(key);

        string normalizedChannelId =
            NormalizeChannelId(channelId);

        byte[] plaintextBytes =
            Encoding.UTF8.GetBytes(plaintext);

        byte[] nonce = new byte[NonceSize];
        byte[] ciphertext = new byte[plaintextBytes.Length];
        byte[] tag = new byte[TagSize];

        byte[] associatedData =
            CreateAssociatedData(normalizedChannelId);

        RandomNumberGenerator.Fill(nonce);

        using AesGcm aes = new(key, TagSize);

        aes.Encrypt(
            nonce,
            plaintextBytes,
            ciphertext,
            tag,
            associatedData);

        EncryptedPayload payload = new()
        {
            Version = ProtocolVersion,
            Algorithm = Algorithm,
            Nonce = Convert.ToBase64String(nonce),
            Ciphertext = Convert.ToBase64String(ciphertext),
            Tag = Convert.ToBase64String(tag)
        };

        return JsonSerializer.Serialize(
            payload,
            JsonOptions);
    }

    public string Decrypt(
        string encryptedPayload,
        byte[] key,
        string channelId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            encryptedPayload);

        ValidateKey(key);

        string normalizedChannelId =
            NormalizeChannelId(channelId);

        EncryptedPayload payload =
            JsonSerializer.Deserialize<EncryptedPayload>(
                encryptedPayload,
                JsonOptions)
            ?? throw new CryptographicException(
                "Encrypted payload is invalid.");

        if (payload.Version != ProtocolVersion)
        {
            throw new CryptographicException(
                "Unsupported encryption protocol version.");
        }

        if (!string.Equals(
                payload.Algorithm,
                Algorithm,
                StringComparison.Ordinal))
        {
            throw new CryptographicException(
                "Unsupported encryption algorithm.");
        }

        byte[] nonce =
            Convert.FromBase64String(payload.Nonce);

        byte[] ciphertext =
            Convert.FromBase64String(payload.Ciphertext);

        byte[] tag =
            Convert.FromBase64String(payload.Tag);

        if (nonce.Length != NonceSize ||
            tag.Length != TagSize)
        {
            throw new CryptographicException(
                "Encrypted payload has invalid parameters.");
        }

        byte[] plaintext =
            new byte[ciphertext.Length];

        byte[] associatedData =
            CreateAssociatedData(normalizedChannelId);

        using AesGcm aes = new(key, TagSize);

        aes.Decrypt(
            nonce,
            ciphertext,
            tag,
            plaintext,
            associatedData);

        return Encoding.UTF8.GetString(plaintext);
    }

    private static void ValidateKey(byte[] key)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (key.Length != KeySize)
        {
            throw new ArgumentException(
                "Encryption key must contain exactly 32 bytes.",
                nameof(key));
        }
    }

    private static string NormalizeChannelId(
        string channelId)
    {
        string normalizedChannelId =
            channelId?.Trim() ?? string.Empty;

        if (normalizedChannelId.Length == 0)
        {
            throw new ArgumentException(
                "Channel ID cannot be empty.",
                nameof(channelId));
        }

        return normalizedChannelId;
    }

    private static byte[] CreateAssociatedData(
        string channelId)
    {
        return Encoding.UTF8.GetBytes(
            $"sms-bridge:v1:{channelId}");
    }
}