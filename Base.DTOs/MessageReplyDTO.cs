using System.Collections.Generic;

namespace Base.DTOs
{
    public class MessageReplyDTO
    {
        public string replyToken { get; set; }
        public List<MessageDTO> messages { get; set; }
    }
}