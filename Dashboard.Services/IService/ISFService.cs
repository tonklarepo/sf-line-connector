using System.Collections.Generic;
using System.Threading.Tasks;
using Base.DTOs;
//using Database.Models;

namespace Dashboard.Services.IService
{
    public interface ISFService
    {
        Task<SFLoginResponse> LoginToSalesforce();
        Task<string> SFLineAccount(SFLoginResponse SF, UserDTO lineUser);
        Task<string> CheckSFLineAccount(SFLoginResponse SF, string userId);
        Task SFLineChat(SFLoginResponse SF, string sfLineUserId, string sfLineSessionId, EventDTO currentEvent, string imagePreviewLink);
        Task<string> CheckSFLineSession(SFLoginResponse SF, string userId);
        Task<string> SFLineSession(SFLoginResponse sF, string sfLineUserId, UserDTO lineUser);
        Task<string> SFLineImage(SFLoginResponse sF, MessageDTO messageId, string sfLineUserId);
        Task<string> SFLineFile(SFLoginResponse SF, MessageDTO base64String, string sfLineUserId, string fileName);
    }
}