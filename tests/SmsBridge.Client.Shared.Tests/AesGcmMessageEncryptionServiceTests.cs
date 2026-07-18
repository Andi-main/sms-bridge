using System.Security.Cryptography;

using SmsBridge.Client.Shared.Cryptography;

namespace SmsBridge.Client.Shared.Tests;

public sealed class AesGcmMessageEncryptionServiceTests
{
    private readonly AesGcmMessageEncryptionService _service =
        new();

    [Fact]
    public void EncryptAndDecrypt_ReturnsOriginalPlaintext()
    {
        byte[] key =
            RandomNumberGenerator.GetBytes(32);

        const string plaintext =
            """
            {"sender":"Mom","body":"Test message"}
            """;

        string encrypted = _service.Encrypt(
            plaintext,
            key,
            "demo-channel");

        string decrypted = _service.Decrypt(
            encrypted,
            key,
            "demo-channel");

        Assert.Equal(plaintext, decrypted);
        Assert.DoesNotContain(
            "Test message",
            encrypted);
    }

    [Fact]
    public void Decrypt_WithWrongKey_ThrowsCryptographicException()
    {
        byte[] encryptionKey =
            RandomNumberGenerator.GetBytes(32);

        byte[] wrongKey =
            RandomNumberGenerator.GetBytes(32);

        string encrypted = _service.Encrypt(
            "Secret message",
            encryptionKey,
            "demo-channel");

        Assert.ThrowsAny<CryptographicException>(() =>
            _service.Decrypt(
                encrypted,
                wrongKey,
                "demo-channel"));
    }

    [Fact]
    public void Decrypt_WithDifferentChannel_ThrowsCryptographicException()
    {
        byte[] key =
            RandomNumberGenerator.GetBytes(32);

        string encrypted = _service.Encrypt(
            "Secret message",
            key,
            "channel-one");

        Assert.ThrowsAny<CryptographicException>(() =>
            _service.Decrypt(
                encrypted,
                key,
                "channel-two"));
    }
}