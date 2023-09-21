using BRG.Satispay.Models;
using System.Net;
using Xunit.Abstractions;

namespace BRG.Satispay.Test
{
    public class TestPayment : IDisposable
    {
        private const string TOKEN = "JL5CCJ",
            PRIVATE_KEY_PEM = "-----BEGIN RSA PRIVATE KEY-----\n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\n-----END RSA PRIVATE KEY-----\n",
            KEY_ID = "ikvpph3n7ujus5i4vqta5b2v7e2n3r5931chg988s3encgfo0i6sl5odnjhbgavfh83snsv69cmke9pp11gui1ts3puu135etvn7e6adfsb7dcdsgekoutjmr5it2brhoub6i0v0des7u6lpqpae3bg16bp34rq1o72eij2u0a271gsov5bdhi5blo1go2i2kq006jsb",
            PUBLIC_KEY = "-----BEGIN PUBLIC KEY-----\nMIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEA60fNwCt/7ZEaS3rjb0uh\nWD+b+B89iZJYKjwvi21fo+AKH7WNlYYKrNoBIafKOz6lkHjSLdiXutajK6qAkRfP\n2JLA/6loMsILkDvujenjUS2Puk0ofjtQ93BHaz0zLrRn9t/3X2d0ut8Rf9YTkdAN\nuh6rq0GdM+LEZyp8Ja9dP8mEhDVqJpJKQqIuuOwUbPS9va7pXvYkZKxchNdhq2ra\nGvWNHQDFe7kCasG7oXb0pEakTOvDSg8+9axMDCHUvO7rcYT45rHrDLJzsWymxSQn\nT0E2qJLMrS4X2DioK/SaeV8c71lbhFvXhopOsHEyAAli31b0w29KdWe4+Ftb261b\nypnfGdUX23Al4jRw7ZkfAXtp6wZeEht8fGhCDKGtVAeI2HC6Dt+9034UGoKdkKWX\nqsr6sbuUoMgOVB4C+xxGoIO9zbXlIqMil6ZF1ekLF2we7B9NiAtDzD7BrfuCslCE\nSNao5oD1v+vvtWw57uRqGikx7elI5GWc+lja/O5WwO1Mhi29/4eOXgzPD+pl/DFB\n/DJNJBYx7fde1AjBo3wPcZH/G9hPE1R3H1cqxUwlPCnf7H638AN4UpMmPsBR+toV\n2p9Tg2b12SCpUXc56cnp8EXmQTZA1IlA22F5Y0SK7ltaD5k7GZqBqAb2aaWkKqqo\naFzf753fp5e59zOheq1uTzkCAwEAAQ==\n-----END PUBLIC KEY-----";
        private readonly ITestOutputHelper _output;
        private readonly HttpClient _httpClient;
        private readonly SatispayService _api;
        private readonly SatispayConfigurationParams _config;

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        public TestPayment(ITestOutputHelper output)
        {
            _output = output;
            _httpClient = new HttpClient();
            _api = new SatispayService(_httpClient, true);
            _config = new SatispayConfigurationParams(PRIVATE_KEY_PEM, PUBLIC_KEY, KEY_ID);
        }

        [Fact]
        public async Task Payment_MATCH_CODE()
        {
            var paymentRequest = new CreatePaymentRequest()
            {
                amount_unit = 1,
                currency = "EUR",
                flow = Flow.MATCH_CODE,
                redirect_url = "https://myServer.com/myRedirectUrl"
            };

            var paymentResponse = await _api.CreatePayment(_config, paymentRequest);

            Assert.NotNull(paymentResponse);
            Assert.NotNull(paymentResponse.QrCodeUrl);
            Assert.NotNull(paymentResponse.redirect_url);

            _output.WriteLine(paymentResponse.id);
            _output.WriteLine(paymentResponse.QrCodeUrl);
            _output.WriteLine(paymentResponse.redirect_url);
        }

        [Fact]
        public async Task Payment_MATCH_USER()
        {
            var consumer = await _api.GetConsumer(_config, "+393485524181");

            var paymentRequest = new CreatePaymentRequest()
            {
                amount_unit = 1,
                currency = "EUR",
                flow = Flow.MATCH_USER,
                redirect_url = "https://myServer.com/myRedirectUrl",
                consumer_uid = consumer.id
            };

            var paymentResponse = await _api.CreatePayment(_config, paymentRequest);

            Assert.NotNull(paymentResponse);
            Assert.NotNull(paymentResponse.QrCodeUrl);
            Assert.NotNull(paymentResponse.redirect_url);

            _output.WriteLine(paymentResponse.id);
            _output.WriteLine(paymentResponse.QrCodeUrl);
            _output.WriteLine(paymentResponse.redirect_url);
        }

        [Fact]
        public async Task Payment_MATCH_USER_PhoneNotExists()
        {
            try
            {
                await _api.GetConsumer(_config, "+393659999999");
            }
            catch (SatispayException ex)
            {
                Assert.Equal(HttpStatusCode.NotFound, ex.Code);
            }
        }
    }
}