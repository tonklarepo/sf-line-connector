using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Base.DTOs;
using System.Net;
using System.Text.Json;
using Dashboard.Services.IService;
using Dashboard.Services.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PagingExtensions;
using Swashbuckle.AspNetCore.Swagger;
using JsonSerializer = System.Text.Json.JsonSerializer;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Dashboard.API.Controllers
{
    [Route("api/[controller]")]
    public class LinesController : BaseController
    {
        private readonly IConfiguration Config;
        private ILineService LineService;
        private ISFService SFService;
        private JsonSerializerSettings _jsonSerializerSettings;
        

        public LinesController(IConfiguration config, ILineService lineService, ISFService sfService)
        {
            this.Config = config;
            this.LineService = lineService;
            this.SFService = sfService;
        }
        
        [HttpPost("LineBot")]
        [ProducesResponseType(200, Type = typeof(string))]
        public async Task<IActionResult> LineBot([FromBody] dynamic req)
        {
            string status = "success250";
            try
            {
                string responseTextJson = "";
                string lineAccessToken = Config["LINE:LineChannelAccessToken"];
                var requestToString = req.ToString();
                var convertedRequest = JsonConvert.DeserializeObject<LineDTO>(requestToString);
                Console.WriteLine(convertedRequest);
                
                //_logger.Info(input.ToString());
                List<EventDTO> events = convertedRequest.events;

                string userId = string.Empty;
                string roomId = string.Empty;
                string groupId = string.Empty;
                string imagePreviewLink = string.Empty;


                userId = events[0].source.userId;
                //string roomId = events.EventList[0].Source.RoomId;
                //string groupId = events.EventList[0].Source.GroupId;
                string replyToken = events[0].replyToken;
                string message = events[0].message.text;

                

                var chatId = string.Empty;
                if (!string.IsNullOrEmpty(groupId))
                {
                    chatId = groupId;
                }
                else if (!string.IsNullOrEmpty(roomId))
                {
                    chatId = roomId;
                }
                else if (!string.IsNullOrEmpty(userId))
                {
                    chatId = userId;
                }

                UserDTO lineUser = await LineService.GetUserDetail(userId);
                
                //Login to Salesforce
                var SF = await SFService.LoginToSalesforce();

                //Check User in SF
                string sfLineUserId = await SFService.CheckSFLineAccount(SF, userId);

                //Check Line Session in SF
                string sfLineSessionId = await SFService.CheckSFLineSession(SF, userId);

                if (string.IsNullOrEmpty(sfLineUserId))
                {
                    //Insert User in SF
                    sfLineUserId = await SFService.SFLineAccount(SF, lineUser);
                }

                if (string.IsNullOrEmpty(sfLineSessionId))
                {
                    //Insert Line Session in SF
                    sfLineSessionId = await SFService.SFLineSession(SF, sfLineUserId, lineUser);
                }

                if (events[0].message.type.ToLower() == "sticker")
                {
                    events[0].message.text = "https://stickershop.line-scdn.net/stickershop/v1/sticker/" +
                              events[0].message.stickerId + "/iPhone/sticker@2x.png";
                }
                if (events[0].message.type.ToLower() == "image")
                {
                    //events[0].message.text = events[0].message.previewImageUrl;
                    MessageDTO messageId = await LineService.GetContent(events[0].message.id);
                    imagePreviewLink = await SFService.SFLineImage(SF, messageId, sfLineUserId);
                }
                if (events[0].message.type.ToLower() == "file")
                {
                    //events[0].message.text = events[0].message.previewImageUrl;
                    MessageDTO messageId = await LineService.GetContent(events[0].message.id);
                    imagePreviewLink = await SFService.SFLineFile(SF, messageId, sfLineUserId, events[0].message.fileName);
                }

                //Insert Chat in SF
                await SFService.SFLineChat(SF, sfLineUserId, sfLineSessionId, events[0], imagePreviewLink);
                
                //Reply Message
                /* MessageReplyDTO MessageReply = new MessageReplyDTO();
                MessageReply.replyToken = replyToken;
                MessageReply.messages = new List<MessageDTO>();
                MessageDTO newMessage = new MessageDTO();
                newMessage.type = "text";
                newMessage.text = "success";
                MessageReply.messages.Add(newMessage);
                
                await LineService.MessageReply(MessageReply); */
                
                //_logger.Info(chatId.ToString());

                var replyMessage = string.Empty;
                
                //_logger.Info(string.Format("UserId: {0}", chatId.ToString()));
                //_logger.Info(string.Format("ReplyToken: {0}", replyToken.ToString()));
                //_logger.Info(string.Format("Message: {0}", message.ToString()));

                List<string> res = _messageHandler(message);
                //_logger.Info(string.Format("replyMessage {0}", replyMessage));

                /* var messagesJson = string.Empty;

                var jsonresult = string.Empty;

                for (int i = 0; i < res.Count; i++)
                {
                    var msg = res[i];
                    messagesJson += msg;
                    jsonresult += "{\"type\":\"text\",\"text\":\"" + msg + "\"}";
                    var str = i == res.Count - 1 ? "" : ",";
                    jsonresult += str;
                }

                jsonresult = string.Empty;
                jsonresult += "{\"type\":\"text\",\"text\":\"" + messagesJson + "\"}";


           
                responseTextJson = "{\"replyToken\":\"" + replyToken +
                                   "\",\"messages\":[" + jsonresult +"]}";


                var httpWebRequest = (HttpWebRequest) WebRequest.Create(Config["LINE:LineApiEndPoint"]);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers.Add("Authorization", "Bearer " + lineAccessToken);
                //_logger.Info(string.Format("{0}:{1}", replyToken, responseTextJson));
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(responseTextJson);
                }

                var httpResponse = (HttpWebResponse) httpWebRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                } */
            }
            catch (Exception ex)
            {
                //_logger.Info(ex.Message);
            }
            
            return Ok(status);

        }
        
        [HttpPost("MessageReply")]
        [ProducesResponseType(200, Type = typeof(string))]
        public async Task<IActionResult> MessageReply([FromBody] MessageReplyDTO message)
        {
            try
            {
                await LineService.MessageReply(message);
            }
            catch (Exception ex)
            {
                //_logger.Info(ex.Message);
            }
            
            return Ok();

        }

        [HttpPost]
        public void Post([FromBody] dynamic req)
        {
            try
            {
                string responseTextJson = "";
                string lineAccessToken = Config["LINE:LineChannelAccessToken"];
                var input = JsonConvert.SerializeObject(req);
                //_logger.Info(input.ToString());
                Events events = JsonConvert.DeserializeObject<Events>(input) as Events;
                string userId = events.EventList[0].Source.UserId;
                string roomId = events.EventList[0].Source.RoomId;
                string groupId = events.EventList[0].Source.GroupId;
                string replyToken = events.EventList[0].ReplyToken;
                string message = events.EventList[0].Message.Text;


               

                var chatId = string.Empty;
                if (groupId != null)
                {
                    chatId = groupId;
                }
                else if (roomId != null)
                {
                    chatId = roomId;
                }
                else if (userId != null)
                {
                    chatId = userId;
                }

                //_logger.Info(chatId.ToString());

                var replyMessage = string.Empty;
                
                //_logger.Info(string.Format("UserId: {0}", chatId.ToString()));
                //_logger.Info(string.Format("ReplyToken: {0}", replyToken.ToString()));
                //_logger.Info(string.Format("Message: {0}", message.ToString()));

                List<string> res = _messageHandler(message);
                //_logger.Info(string.Format("replyMessage {0}", replyMessage));

                var messagesJson = string.Empty;

                var jsonresult = string.Empty;

                for (int i = 0; i < res.Count; i++)
                {
                    var msg = res[i];
                    messagesJson += msg;
                    jsonresult += "{\"type\":\"text\",\"text\":\"" + msg + "\"}";
                    var str = i == res.Count - 1 ? "" : ",";
                    jsonresult += str;
                }

                jsonresult = string.Empty;
                jsonresult += "{\"type\":\"text\",\"text\":\"" + messagesJson + "\"}";


           
                responseTextJson = "{\"replyToken\":\"" + replyToken +
                                   "\",\"messages\":[" + jsonresult +"]}";


                var httpWebRequest = (HttpWebRequest) WebRequest.Create(Config["LINE:LineApiEndPoint"]);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers.Add("Authorization", "Bearer " + lineAccessToken);
                //_logger.Info(string.Format("{0}:{1}", replyToken, responseTextJson));
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(responseTextJson);
                }

                var httpResponse = (HttpWebResponse) httpWebRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                //_logger.Info(ex.Message);
            }
        }

        private List<string> _messageHandler(string message)
        {
            string returnMessage;
            var listMessage = new List<string>();
            message = message.ToLower();
            
            if (message.Contains("Hello"))
            {

                returnMessage = "Hi boss.";
                listMessage.Add(returnMessage);
            }

            return listMessage;
        }
        
    }

}