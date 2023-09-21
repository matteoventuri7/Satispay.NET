namespace BRG.Satispay.Models
{
    public class SatispayConfigurationParams
    {
        public SatispayConfigurationParams(string pemPrivateKey, string publicKey, string keyId)
        {
            PemPrivateKey = pemPrivateKey;
            PublicKey = publicKey;
            KeyId = keyId;
        }

        public string PemPrivateKey { get; private set; }
        public string PublicKey { get; private set; }
        public string KeyId { get; private set; }
    }
}
