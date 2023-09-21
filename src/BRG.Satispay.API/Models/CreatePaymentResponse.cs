namespace BRG.Satispay.API.Models
{
    public class CreatePaymentResponse<T> : PaymentResponse<T>
    {
        public string QrCodeUrl { get; set; }
    }
}
