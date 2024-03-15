using System.Collections.Generic;

namespace Base.DTOs
{
    public class LineDTO
    {
        public string destination { get; set; }
        public List<EventDTO> events { get; set; }
    }
}