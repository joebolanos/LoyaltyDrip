namespace Core.Shared.Security
{
    public interface IEncryptionService
    {
        string Encrypt(string value);
        string Decrypt(string value);

    }
}
