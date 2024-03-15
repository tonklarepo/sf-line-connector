using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Base.DTOs;
//using Dashboard.API.Controllers;
using Dashboard.Services.Exceptions;
using Dashboard.Services.IService;
//using Database.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Dashboard.Services.Service
{
    public class LineService : ILineService
    {
        private readonly IConfiguration Config;
        private readonly IHttpContextAccessor HttpContextAccessor;
        //private readonly UserManager<AppUser> UserManager;
        //private readonly DatabaseContext DB;
        private string _actorId;

        public LineService(IConfiguration config,
            IHttpContextAccessor httpContextAccessor
            //DatabaseContext db,
            //UserManager<AppUser> userManager
            )
        {
            this.Config = config;
            //this.DB = db;
            this.HttpContextAccessor = httpContextAccessor;
            //this.UserManager = userManager;

            /* if (HttpContextAccessor != null && HttpContextAccessor.HttpContext != null && HttpContextAccessor.HttpContext.User != null)
            {
                if (UserManager != null)
                {
                    _actorId = UserManager.GetUserId(HttpContextAccessor.HttpContext.User);
                }
            } */
        }

        public async Task<MessageDTO> GetContent(string id)
        {
            MessageDTO result = new MessageDTO();


            var accessToken = Config["LINE:LineChannelAccessToken"];
            var finalUrl = "https://api-data.line.me/v2/bot/message/"+id+ "/content";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage response = await client.GetAsync(finalUrl).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {

                    var stream = await response.Content.ReadAsStreamAsync();
                    var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);

                    // Convert the image to base64
                    string base64String = Convert.ToBase64String(memoryStream.ToArray());   


                    // Create and return a MessageDTO with the base64 string
                    return new MessageDTO { ImageBase64 = base64String };
                }
                else
                {
                    // Handle API call failure
                    throw new Exception($"Failed to call API. Status code: {response.StatusCode}");
                }
            }
        }

        public async Task<UserDTO> GetUserDetail(string userId)
        {
            UserDTO result = new UserDTO();

            var lineUrl = Config["LINE:LineApiEndPoint"] + Config["LINE:LineApiProfile"];
            var accessToken = Config["LINE:LineChannelAccessToken"];
            var finalUrl = lineUrl + userId;
            
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(lineUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(finalUrl).ConfigureAwait(false);
                
                if (response.IsSuccessStatusCode)
                {
                    var getResponse = await response.Content.ReadAsAsync<UserDTO>();

                    result.userId = getResponse.userId;
                    result.displayName = getResponse.displayName;
                    result.pictureUrl = getResponse.pictureUrl;
                    result.statusMessage = getResponse.statusMessage;
                    result.language = getResponse.language;
                    
                    return result;
                }
                else
                {
                    return result;
                }
            }
            
        }

        public async Task MessageReply(MessageReplyDTO message)
        {
            var endpointURL = Config["LINE:LineApiEndPoint"];
            var replyMessageURL = Config["LINE:LineAPIMessage"];
            var finalURL = endpointURL + replyMessageURL;
            var accessToken = Config["LINE:LineChannelAccessToken"];
                
            var messagesJson = JsonConvert.SerializeObject(message);
            
            using (var client = new HttpClient())
            {
                /* client.BaseAddress = new Uri(finalURL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                
                StringContent jsonMessageReply = new StringContent(messagesJson, Encoding.UTF8, "application/json");
                
                HttpResponseMessage response = await client.PostAsync(finalURL, jsonMessageReply).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Send message to line user");
                    var updateResponse = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine("Response : " + updateResponse);
                } */
                
                StringContent content = new StringContent(messagesJson, Encoding.UTF8, "application/json");
                        
                HttpRequestMessage requestUpdate = new HttpRequestMessage(HttpMethod.Post, finalURL);
                requestUpdate.Headers.Add("Authorization", "Bearer " + accessToken);
                requestUpdate.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                requestUpdate.Content = content;

                HttpResponseMessage response = client.SendAsync(requestUpdate).Result;
                        
                if (response.IsSuccessStatusCode)
                {
                    var updateResponse = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine("Response : " + updateResponse);
                }
            }

            /* var httpWebRequest = (HttpWebRequest) WebRequest.Create(finalURL);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.Headers.Add("Authorization", "Bearer " + accessToken);
                
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(message);
            }

            var httpResponse = (HttpWebResponse) httpWebRequest.GetResponse();

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
            } */
        }
        
    }
}
