using BRG.Satispay.API;
using BRG.Satispay.API.Models;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace SatispayGBusiness
{
    /// <summary>
    /// Use this class to configure the first time
    /// </summary>
    public class SatispayConfigurationService
    {
        private const string baseDomain = "authservices.satispay.com";
        private HttpClient httpClient;
        private bool isSandBox;

        public SatispayConfigurationService(HttpClient httpClient, bool isSandBox = false)
        {
            this.isSandBox = isSandBox;
            this.httpClient = httpClient;
            httpClient.BaseAddress = isSandBox ?
            new Uri($"https://staging.{baseDomain}/g_business/v1/") :
            new Uri($"https://{baseDomain}/g_business/v1/");
        }

        /// <summary>
        /// Just to use the first time
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<SatispayConfigurationParams> GenerateConfiguration(string token)
        {
            RsaKeyPairGenerator rkpg = new RsaKeyPairGenerator();
            rkpg.Init(new KeyGenerationParameters(new SecureRandom(), 4096));
            var ackp = rkpg.GenerateKeyPair();

            var publicKey = GetPem(ackp.Public);
            var privateKey = GetPem(ackp.Private);
            var keyId = await RequestKeyId(new(token, publicKey));

            return new(privateKey, publicKey, keyId);
        }

        private string GetPem(AsymmetricKeyParameter akp)
        {
            StringBuilder keyPem = new StringBuilder();
            PemWriter pemWriter = new PemWriter(new StringWriter(keyPem));
            pemWriter.WriteObject(akp);
            pemWriter.Writer.Flush();

            return keyPem.ToString().Replace("\r", string.Empty);
        }

        /// <summary>
        /// Just one time configuration.
        /// </summary>
        /// <returns>The KeyId</returns>
        private async Task<string> RequestKeyId(RequestKeyIdRequest request)
        {
            HttpResponseMessage response = null;

            try
            {
                response = await httpClient.PostAsJsonAsync("authentication_keys", request);

                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<RequestKeyIdResponse>();

                return result.key_id;
            }
            catch (HttpRequestException ex)
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        throw new ActivationTokenNotFoundException();
                    case HttpStatusCode.Forbidden:
                        throw new ActivationTokenAlreadyPairedException();
                    case HttpStatusCode.BadRequest:
                        throw new InvalidRsaKeyException();
                }

                throw ex;
            }
        }
    }
}
