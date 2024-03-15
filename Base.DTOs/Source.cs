using Newtonsoft.Json;

namespace Base.DTOs
{
    public class Source
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("roomId")]
        public string RoomId { get; set; }

        [JsonProperty("groupId")]
        public string GroupId { get; set; }
    }
}