namespace TBT.App.Services.Encryption.Interfaces
{
    public interface IEncryptionService
    {
        string Encrypt(string cypherText);
        string Decrypt(string plainText);
    }
}
