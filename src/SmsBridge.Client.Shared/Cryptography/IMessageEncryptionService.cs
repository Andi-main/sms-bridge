namespace SmsBridge.Client.Shared.Cryptography;

public interface IMessageEncryptionService
{
    string Encrypt(
        string plaintext,
        byte[] key,
        string channelId);

    string Decrypt(
        string encryptedPayload,
        byte[] key,
        string channelId);
}