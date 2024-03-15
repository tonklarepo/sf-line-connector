using Newtonsoft.Json;

namespace Base.DTOs
{
    public class Message
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }
}