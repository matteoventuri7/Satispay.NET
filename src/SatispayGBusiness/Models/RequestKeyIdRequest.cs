namespace BRG.Satispay.Models
{
    public class RequestKeyIdRequest
    {
        public string public_key { get; set; }

        public string token { get; set; }

        public RequestKeyIdRequest(string token, string publicKey)
        {
            this.token = token;
            this.public_key = publicKey;
        }
    }
}
