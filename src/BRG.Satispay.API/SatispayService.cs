using BRG.Satispay.API.Models;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BRG.Satispay.API
{
    public class SatispayService
    {
        private const string baseDomain = "authservices.satispay.com";
        private HttpClient httpClient;
        private bool isSandBox;

        public SatispayService(HttpClient httpClient, bool isSandBox = false)
        {
            this.isSandBox = isSandBox;
            this.httpClient = httpClient;
            httpClient.BaseAddress = isSandBox ?
            new Uri($"https://staging.{baseDomain}/g_business/v1/") :
            new Uri($"https://{baseDomain}/g_business/v1/");
        }

        private AsymmetricCipherKeyPair GetAsymmetricCipherKeyPair(string pemPrivateKey)
        {
            var pemReader = new PemReader(new StringReader(pemPrivateKey));
            var ackp = (AsymmetricCipherKeyPair)pemReader.ReadObject();
            return ackp;
        }

        private async Task<T> SendJsonAsync<T>(SatispayConfigurationParams config, HttpMethod method, string requestUri, object content = null, string idempotencyKey = null)
        {
            var requestJson = string.Empty;

            if (content != null)
                requestJson = JsonSerializer.Serialize(content, new JsonSerializerOptions()
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    WriteIndented = true
                });

            var httpRequestMessage = new HttpRequestMessage(method, requestUri)
            {
                Content = content == null ? null : new StringContent(requestJson, Encoding.UTF8, "application/json")
            };

            using SHA256 sha256 = SHA256.Create();

            var now = DateTime.Now;
            var date = now.ToString("ddd, d MMM yyyy HH:mm:ss", CultureInfo.InvariantCulture) + " " + now.ToString("zzz").Replace(":", string.Empty);

            httpRequestMessage.Headers.Add("Date", date);

            var signature = new StringBuilder();

            signature.Append($"(request-target): {method.Method.ToLower()} {httpClient.BaseAddress.LocalPath}{requestUri}\n");
            signature.Append($"host: {httpClient.BaseAddress.Host}\n");
            signature.Append($"date: {((string[])httpRequestMessage.Headers.GetValues("Date"))[0]}\n");

            var digest = $"SHA-256={Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(requestJson)))}";

            signature.Append($"digest: {digest}");

            var sign = SignData(signature.ToString(), GetAsymmetricCipherKeyPair(config.PemPrivateKey).Private);

            httpRequestMessage.Headers.Add("Digest", digest);
            httpRequestMessage.Headers.Add("Authorization",
                $"Signature keyId=\"{config.KeyId}\", algorithm=\"rsa-sha256\", headers=\"(request-target) host date digest\", signature=\"{sign}\"");

            if (idempotencyKey != null)
                httpRequestMessage.Headers.Add("Idempotency-Key", idempotencyKey);

            HttpResponseMessage response = null;

            string stringContent = string.Empty;

            try
            {
                response = await httpClient.SendAsync(httpRequestMessage);

                stringContent = await response.Content.ReadAsStringAsync();

                response.EnsureSuccessStatusCode();

                return JsonSerializer.Deserialize<T>(stringContent);
            }
            catch (HttpRequestException)
            {
                throw new SatispayException(stringContent, response.StatusCode);
            }
            catch (JsonException)
            {
                throw new SatispayException(stringContent, HttpStatusCode.OK);
            }
        }

        private string SignData(string msg, ICipherParameters privateKey)
        {
            byte[] msgBytes = Encoding.UTF8.GetBytes(msg);
            ISigner signer = GetSigner();
            signer.Init(true, privateKey);
            signer.BlockUpdate(msgBytes, 0, msgBytes.Length);
            byte[] sigBytes = signer.GenerateSignature();

            return Convert.ToBase64String(sigBytes);
        }

        private static ISigner GetSigner()
        {
            return SignerUtilities.GetSigner("SHA256WithRSA");
        }

        private bool VerifySignature(string privateKey, string signature, string msg)
        {
            byte[] msgBytes = Encoding.UTF8.GetBytes(msg);
            byte[] sigBytes = Convert.FromBase64String(signature);

            ISigner signer = GetSigner();
            signer.Init(false, GetAsymmetricCipherKeyPair(privateKey).Public);
            signer.BlockUpdate(msgBytes, 0, msgBytes.Length);
            return signer.VerifySignature(sigBytes);
        }

        public async Task<CreatePaymentResponse<T>> CreatePayment<T>(SatispayConfigurationParams config, CreatePaymentRequest<T> request, string idempotencyKey = null)
        {
            if (request.amount_unit == 0)
                throw new SatispayException("amount_unit must be greater than 0", HttpStatusCode.BadRequest);

            var response = await SendJsonAsync<CreatePaymentResponse<T>>(config, HttpMethod.Post, "payments", request, idempotencyKey);

            response.QrCodeUrl = isSandBox ? $"https://staging.online.satispay.com/qrcode/{response.code_identifier}" : $"https://online.satispay.com/qrcode/{response.code_identifier}";

            return response;
        }

        public async Task<GetCustomerResponse> GetConsumer(SatispayConfigurationParams config, string phone, string idempotencyKey = null)
        {
            if (phone is null)
                throw new SatispayException("phone must be not null", HttpStatusCode.BadRequest);

            var response = await SendJsonAsync<GetCustomerResponse>(config, HttpMethod.Get, $"consumers/{phone}", null, idempotencyKey);

            return response;
        }

        public async Task<PaymentDetailsResponse<T>> GetPaymentDetails<T>(SatispayConfigurationParams config, string paymentId)
        {
            return await SendJsonAsync<PaymentDetailsResponse<T>>(config, HttpMethod.Get, $"payments/{paymentId}");
        }
        public async Task<PaymentDetailsResponse<T>> UpdatePaymentDetails<T>(SatispayConfigurationParams config, string paymentId, UpdatePaymentRequest<T> request)
        {
            return await SendJsonAsync<PaymentDetailsResponse<T>>(config, HttpMethod.Put, $"payments/{paymentId}", request);
        }
    }
}
