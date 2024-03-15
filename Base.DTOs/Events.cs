using System.Collections.Generic;
using Newtonsoft.Json;

namespace Base.DTOs
{
    public class Events
    {
        [JsonProperty("events")]
        public List<EventsList> EventList { get; set; }
    }
}