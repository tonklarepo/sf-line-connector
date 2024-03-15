using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Base.DTOs;

namespace Dashboard.Services.Service
{
    public class AuthService
    {
        private string _baseAddress;
        private string _loginURI;
        private string _endpointURL;
        private string _userName;
        private string _password;
        private string _clientId;
        private string _clientSecret;
        private string _token;

        public string EndpointURI { get; set; }

        public AuthService(string baseAddress, string loginUri, string endpointURI, string userName, string password
            , string clientId, string clientSecret, string token)
        {
            _baseAddress = baseAddress;
            _loginURI = loginUri;
            _endpointURL = endpointURI;
            _userName = userName;
            _password = password;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _token = token;
        }

        public async Task<APIResponse_Auth_Login> Login()
        {
            try
            {
                APIResponse_Auth_Login loginResponse = new APIResponse_Auth_Login();

                using (var client = new HttpClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 |
                                                           SecurityProtocolType.Tls11 |
                                                           SecurityProtocolType.Tls;

                    client.BaseAddress = new Uri(_baseAddress);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    Dictionary<string, string> dictForm = new Dictionary<string, string>();
                    dictForm.Add("grant_type", "password");
                    dictForm.Add("username", _userName);
                    dictForm.Add("password", _password + _token);
                    dictForm.Add("client_id", _clientId);
                    dictForm.Add("client_secret", _clientSecret);
                    var content = new FormUrlEncodedContent(dictForm);

                    HttpResponseMessage response = await client.PostAsync(_loginURI, content).ConfigureAwait(false);
                    
                    //Console.WriteLine("Response : " + response);
                    if (response.IsSuccessStatusCode)
                    {
                        loginResponse = await response.Content.ReadAsAsync<APIResponse_Auth_Login>();
                    }
                    else
                    {
                        loginResponse.Message = await response.Content.ReadAsStringAsync();
                    }

                    loginResponse.StatusCode = response.StatusCode;
                }

                return loginResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
        }
    }
}