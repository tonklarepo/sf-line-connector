using System.Collections.Generic;
using System.Threading.Tasks;
using Base.DTOs;
//using Database.Models;

namespace Dashboard.Services.IService
{
    public interface ILineService
    {
        Task<UserDTO> GetUserDetail(string userId);
        Task MessageReply(MessageReplyDTO message);
        Task<MessageDTO> GetContent(string id);
    }
}