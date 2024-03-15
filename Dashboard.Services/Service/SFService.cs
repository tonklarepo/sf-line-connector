using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
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
    public class SFService : ISFService
    {
        private readonly IConfiguration Config;
        private readonly IHttpContextAccessor HttpContextAccessor;
        //private readonly UserManager<AppUser> UserManager;
        //private readonly DatabaseContext DB;
        private string _actorId;

        public SFService(IConfiguration config,
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

        public async Task<SFLoginResponse> LoginToSalesforce()
        {
            var sfAuthUrl = Config["Salesforce:AuthEndpoint"];
            var sfConsumerKey = Config["Salesforce:ConsumerKey"];
            var sfConsumerSecret = Config["Salesforce:ConsumerSecret"];
            var sfUsername = Config["Salesforce:Username"];
            var sfPassword = Config["Salesforce:Password"];
            var sfBaseUrl = Config["Salesforce:BaselineURL"];
            var sfToken = Config["Salesforce:SecurityToken"];

            var sfCPEM = new AuthService(sfBaseUrl, sfAuthUrl, sfBaseUrl, sfUsername, sfPassword,
                sfConsumerKey, sfConsumerSecret, sfToken);

            var result = await sfCPEM.Login();

            SFLoginResponse response = new SFLoginResponse();
            response.access_token = result.access_token;
            response.url = result.instance_url;
            
            return response;
        }
        
        public async Task<string> CheckSFLineAccount(SFLoginResponse SF, string userId)
        {
            string sfLineUserId = string.Empty;
            
            var sfBaseUrl = SF.url;
            var sfUrl = SF.url + "/services/data/v53.0/query?q=select+id+,+name+from+Line_Account__c";
            
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(sfBaseUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", SF.access_token);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.GetAsync(sfUrl).ConfigureAwait(false);
                
                    if (response.IsSuccessStatusCode)
                    {
                        
                        var getResponse = await response.Content.ReadAsAsync<SFRecordResponse>();

                        foreach (var records in getResponse.records)
                        {
                            if (userId == records.name)
                            {
                                sfLineUserId = records.id;
                            }
                        }


                    }
                    else
                    {
                        Console.WriteLine($"Failed");
                    }
                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            return sfLineUserId;
        }

        public async Task<string> CheckSFLineSession(SFLoginResponse SF, string userId)
        {
            string sfLineSessionId = string.Empty;

            var sfBaseUrl = SF.url;
            var sfUrl = SF.url + "/services/data/v53.0/query?q=select+id+,+name+,+status__c+,+line_userid__c+from+Line_Session__c";

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(sfBaseUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", SF.access_token);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.GetAsync(sfUrl).ConfigureAwait(false);

                    if (response.IsSuccessStatusCode)
                    {

                        var getResponse = await response.Content.ReadAsAsync<SFRecordResponse>();

                        foreach (var records in getResponse.records)
                        {
                            if (userId == records.Line_UserID__c && records.Status__c == "Opened")
                            {
                                sfLineSessionId = records.id;
                            }
                        }


                    }
                    else
                    {
                        Console.WriteLine($"Failed");
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return sfLineSessionId;
        }

        public async Task<string> SFLineAccount(SFLoginResponse SF, UserDTO lineUser)
        {
            string sfLineUserId = string.Empty;
            
            var sfBaseUrl = SF.url;
            var sfUrl = SF.url + "/services/data/v53.0/sobjects/Line_Account__c";
            
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(sfBaseUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", SF.access_token);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    SFLineAccountDTO SFLineAccount = new SFLineAccountDTO();
                    SFLineAccount.name = lineUser.userId;
                    SFLineAccount.display_name__c = lineUser.displayName;
                    SFLineAccount.picture_url__c = lineUser.pictureUrl;
                    SFLineAccount.status_message__c = lineUser.statusMessage;
                    SFLineAccount.language__c = lineUser.language;

                    var param = JsonConvert.SerializeObject(SFLineAccount);
                    StringContent jsonPISUser = new StringContent(param, Encoding.UTF8, "application/json");
                    
                    HttpResponseMessage response = await client.PostAsync(sfUrl, jsonPISUser).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                    {
                        var createResponse = await response.Content.ReadAsAsync<SFResponse>();

                        if (createResponse.success)
                        {
                            Console.WriteLine($"Create Line Account : {createResponse}");
                            sfLineUserId = createResponse.id;
                        }

                    }
                    else
                    {
                        Console.WriteLine($"Create Failed : {lineUser.userId}");
                    }
                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            return sfLineUserId;
        }

        public async Task<string> SFLineSession(SFLoginResponse SF, string sfLineUserId, UserDTO lineUser)
        {
            string sfLineSessionId = string.Empty;

            var sfBaseUrl = SF.url;
            var sfUrl = SF.url + "/services/data/v53.0/sobjects/Line_Session__c";

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(sfBaseUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", SF.access_token);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    SFLineSessionDTO SFLineSession = new SFLineSessionDTO();
                    SFLineSession.line_account__c = sfLineUserId;
                    SFLineSession.status__c = "Opened";
                    SFLineSession.ownerid = "00G1s000003dQ0YEAU";

                    var param = JsonConvert.SerializeObject(SFLineSession);
                    StringContent jsonPISUser = new StringContent(param, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(sfUrl, jsonPISUser).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                    {
                        var createResponse = await response.Content.ReadAsAsync<SFResponse>();

                        if (createResponse.success)
                        {
                            Console.WriteLine($"Create Line Session : {createResponse}");
                            sfLineSessionId = createResponse.id;
                        }

                    }
                    else
                    {
                        var createResponse = await response.Content.ReadAsAsync<SFResponse>();
                        Console.WriteLine($"Create Line Session Failed : {lineUser.userId}");
                        Console.WriteLine($"{createResponse}");
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return sfLineSessionId;
        }

        public async Task<string> SFLineImage(SFLoginResponse SF, MessageDTO base64String, string sfLineUserId)
        {
            string previewLink = string.Empty;
            var sfBaseUrl = SF.url;
            var sfUrl = SF.url + "/services/data/v59.0/sobjects/ContentVersion/";

            // Convert UTC time to GMT+7
            DateTime gmtPlus7Time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));


            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(sfBaseUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", SF.access_token);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    SFLineImageDTO SFLineImage = new SFLineImageDTO();
                    SFLineImage.Title = $"Image-{gmtPlus7Time.ToString("yyyy-MM-dd-HH:mm:ss")} (GMT+7)";
                    SFLineImage.PathOnClient = "simple.jpg";
                    SFLineImage.ContentLocation = "S";
                    SFLineImage.FirstPublishLocationId = sfLineUserId;
                    SFLineImage.VersionData = base64String.ImageBase64;
                    


                    var param = JsonConvert.SerializeObject(SFLineImage);
                    StringContent jsonSFLineChat = new StringContent(param, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(sfUrl, jsonSFLineChat).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                    {
                        var createResponse = await response.Content.ReadAsAsync<SFResponse>();
                        previewLink = "https://sundaytechnologies--b2cgrowth.sandbox.file.force.com/sfc/servlet.shepherd/version/renditionDownload?rendition=ORIGINAL_Jpg&versionId=";

                        if (createResponse.success)
                        {
                            Console.WriteLine($"Create Line Image : {createResponse}");
                            return previewLink + createResponse.id;
                        }

                    }
                    else
                    {
                        Console.WriteLine($"Create Line Image Failed");
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return previewLink;
        }

        public async Task<string> SFLineFile(SFLoginResponse SF, MessageDTO base64String, string sfLineUserId, string fileName)
        {
            string previewLink = string.Empty;
            var sfBaseUrl = SF.url;
            var sfUrl = SF.url + "/services/data/v59.0/sobjects/ContentVersion/";

            // Convert UTC time to GMT+7
            DateTime gmtPlus7Time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));


            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(sfBaseUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", SF.access_token);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    SFLineImageDTO SFLineImage = new SFLineImageDTO();
                    SFLineImage.Title = $"File-{gmtPlus7Time.ToString("yyyy-MM-dd-HH:mm:ss")} (GMT+7)";
                    SFLineImage.PathOnClient = $"{fileName}";
                    SFLineImage.ContentLocation = "S";
                    SFLineImage.FirstPublishLocationId = sfLineUserId;
                    SFLineImage.VersionData = base64String.ImageBase64;



                    var param = JsonConvert.SerializeObject(SFLineImage);
                    StringContent jsonSFLineChat = new StringContent(param, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(sfUrl, jsonSFLineChat).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                    {
                        var createResponse = await response.Content.ReadAsAsync<SFResponse>();
                        

                        if (createResponse.success)
                        {
                            string contentDocumentId = await this.GetContentDocumentId(SF, createResponse.id);
                            previewLink = $"https://sundaytechnologies--b2cgrowth.sandbox.lightning.force.com/lightning/r/ContentDocument/{contentDocumentId}/view";
                            Console.WriteLine($"Create Line File : {createResponse}");
                            return previewLink;
                        }

                    }
                    else
                    {
                        Console.WriteLine($"Create Line File Failed");
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return previewLink;
        }

        public async Task<string> GetContentDocumentId(SFLoginResponse SF, string versionId)
        {
            string sfContentDocumentId = string.Empty;

            var sfBaseUrl = SF.url;
            var sfUrl = SF.url + $"/services/data/v53.0/query?q=select+id+,+contentdocumentid+from+contentversion+where+id+=+'{versionId}'";

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(sfBaseUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", SF.access_token);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.GetAsync(sfUrl).ConfigureAwait(false);

                    if (response.IsSuccessStatusCode)
                    {

                        var getResponse = await response.Content.ReadAsAsync<SFRecordResponse>();

                        foreach (var records in getResponse.records)
                        {
                            if (versionId == records.id)
                            {
                                sfContentDocumentId = records.ContentDocumentId;
                            }
                        }


                    }
                    else
                    {
                        Console.WriteLine($"Failed");
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return sfContentDocumentId;
        }

        public async Task SFLineChat(SFLoginResponse SF, string sfLineUserId, string sfLineSessionId, EventDTO currentEvent, string imagePreviewLink)
        {
            var sfBaseUrl = SF.url;
            var sfUrl = SF.url + "/services/data/v53.0/sobjects/Line_Chat__c";
            
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(sfBaseUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", SF.access_token);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    SFLineChatDTO SFLineChat = new SFLineChatDTO();
                    SFLineChat.Events__c = currentEvent.ToString();
                    SFLineChat.Line_Account__c = sfLineUserId;
                    SFLineChat.Message_Id__c = currentEvent.message.id;
                    if (currentEvent.message.type == "image" || currentEvent.message.type == "file") 
                    {
                        SFLineChat.Message_Text__c = imagePreviewLink;
                    }
                    else
                    {
                        SFLineChat.Message_Text__c = currentEvent.message.text;
                    }
                    SFLineChat.Message_Type__c = currentEvent.message.type;
                    SFLineChat.Reply_Token__c = currentEvent.replyToken;
                    SFLineChat.Source_Type__c = currentEvent.source.type;
                    SFLineChat.Source_UserId__c = sfLineUserId;
                    SFLineChat.Line_Session__c = sfLineSessionId;


                    var param = JsonConvert.SerializeObject(SFLineChat);
                    StringContent jsonSFLineChat = new StringContent(param, Encoding.UTF8, "application/json");
                    
                    HttpResponseMessage response = await client.PostAsync(sfUrl, jsonSFLineChat).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                    {
                        var createResponse = await response.Content.ReadAsAsync<SFResponse>();

                        if (createResponse.success)
                        {
                            Console.WriteLine($"Create Line Chat : {createResponse}");
                        }

                    }
                    else
                    {
                        Console.WriteLine($"Create Line Chat Failed");
                    }
                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }
    }
}