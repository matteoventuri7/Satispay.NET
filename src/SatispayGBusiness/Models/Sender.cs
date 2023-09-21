using System.Text.Json.Serialization;

namespace BRG.Satispay.Models
{
    public class Sender
    {
        public string id { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ActorType type { get; set; } = ActorType.CONSUMER;
        public string name { get; set; }
    }
}
