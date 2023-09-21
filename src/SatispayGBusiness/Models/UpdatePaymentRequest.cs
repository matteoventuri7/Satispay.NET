﻿using System.Text.Json.Serialization;

namespace BRG.Satispay.Models
{
    public class UpdatePaymentRequest<T>
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UpdateAction action { get; set; }
        public T metadata { get; set; }
    }
}
