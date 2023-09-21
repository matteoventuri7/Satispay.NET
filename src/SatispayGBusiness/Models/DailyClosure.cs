using System;
using System.Text.Json.Serialization;

namespace BRG.Satispay.Models
{
    public class DailyClosure
    {
        public string id { get; set; }

        [JsonConverter(typeof(SatispayDateTimeConverter))]
        public DateTime? date { get; set; }
    }
}
