namespace GlobalKiosk.Integrations.QualityAssurance
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using RestSharp;
    using System.Net;
    using System.Collections.Generic;

    public class ManagementAPI
    {
        private readonly string baseurl;
        private readonly string managementApi = String.Empty;
        private readonly string tokenUrl;
        private readonly string clientId;
        private readonly string secret;

        //
        public ManagementAPI(string baseurl,string clientId, string secret)
        {
            try
            {
                this.baseurl = baseurl;

                managementApi = $"{baseurl}api/";

				tokenUrl = $"{baseurl}token/";
				this.clientId = clientId;
				this.secret = secret;
			}
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<RestResponse> Put(string requestMethod, string headerName, string headerValue, object requestBody)
        {
            try
            {
                RestResponse response;

				response = await PutToManagementAPI(requestMethod, headerName, headerValue, requestBody, null);
				string token = Environment.GetEnvironmentVariable("OAUTH_TOKEN_MGMT", EnvironmentVariableTarget.Process);

                    if (token == null)
                        //No oAuth token environment variable exists for the process
                        token = await GetRequestToken();

                    response = await PutToManagementAPI(requestMethod, headerName, headerValue, requestBody, token);

                    if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        //First attempt failed (oAuth token has expired)
                        //try to get a token three times
                        for (int i = 0; i <= 2; i++)
                        {
                            token = await GetRequestToken();
                            if (token.Length > 0)
                                break;
                        }

                        if (token.Length > 0)
                            response = await PutToManagementAPI(requestMethod, headerName, headerValue, requestBody, token);
                        else { 
                        }
                    }

                return response;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        //Really Use this one!!! for real
        public async Task<RestResponse> Post(string requestMethod, object requestBody)
        {
            try
            {
                RestResponse response;

				response = await PostToManagementAPI(requestMethod, requestBody, null);
				string token = Environment.GetEnvironmentVariable("OAUTH_TOKEN_MGMT", EnvironmentVariableTarget.Process);

                    if (token == null)
                        //No oAuth token environment variable exists for the process
                        token = await GetRequestToken();

                    response = await PostToManagementAPI(requestMethod, requestBody, token);

                    if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        //First attempt failed (oAuth token has expired)
                        //try to get a token three times
                        for (int i = 0; i <= 2; i++)
                        {
                            token = await GetRequestToken();
                            if (token.Length > 0)
                                break;
                        }

                        if (token.Length > 0)
                            response = await PostToManagementAPI(requestMethod, requestBody, token);
                        else{

                        }
                            
                    }

                return response;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public async Task<string> Get(string requestMethod, Dictionary<string, string> queryHeaders, Dictionary<string, string> queryParameters)
        {
            try
            {
                RestResponse response;

				response = await GetFromManagementAPI(requestMethod, queryHeaders, queryParameters, null);
				string token = Environment.GetEnvironmentVariable("OAUTH_TOKEN_MGMT", EnvironmentVariableTarget.Process);

                    if (token == null)
                        //No oAuth token environment variable exists for the process
                        token = await GetRequestToken();

                    response = await GetFromManagementAPI(requestMethod, queryHeaders, queryParameters, token);

                    if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        //First attempt failed (oAuth token has expired)
                        //try to get a token three times
                        for (int i = 0; i <= 2; i++)
                        {
                            token = await GetRequestToken();
                            if (token.Length > 0)
                                break;
                        }

                        if (token.Length > 0)
                            response = await GetFromManagementAPI(requestMethod, queryHeaders, queryParameters, token);
                        else
                        {
                            return null;
                        }
                }

                return response.Content;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        private async Task<RestResponse> PutToManagementAPI(string requestMethod, string headerName, string headerValue, object requestBody, string token)
        {
            try
            {
                var client = new RestClient();
                var request = new RestRequest($"{managementApi}{requestMethod}", Method.Put);

                if (headerName != null && headerValue != null) request.AddHeader(headerName, headerValue);

                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Accept", "application/json");

                request.AddHeader("Authorization", $"Bearer {token}");

                request.AddHeader("Connection", "keep-alive");

                request.AddJsonBody(requestBody);

                var response = await client.ExecuteAsync(request);

                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //Use this
        private async Task<RestResponse> PostToManagementAPI(string requestMethod, object requestBody, string token)
        {
            try
            {
                var client = new RestClient();
                var request = new RestRequest($"{managementApi}{requestMethod}", Method.Post);

                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Accept", "application/json");

                request.AddHeader("Authorization", $"Bearer {token}");

                request.AddHeader("Connection", "keep-alive");

                request.AddJsonBody(requestBody);

                var response = await client.ExecuteAsync(request);

                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<RestResponse> GetFromManagementAPI(string requestMethod, Dictionary<string, string> queryHeaders, Dictionary<string, string> queryParameters, string token)
        {
            try
            {
                var client = new RestClient();
                var request = new RestRequest($"{managementApi}{requestMethod}", Method.Get);

                foreach (var header in queryHeaders)
                {
                    request.AddHeader(header.Key, header.Value);
                }

                foreach (var parameter in queryParameters)
                {
                    request.AddParameter(parameter.Key, parameter.Value, RestSharp.ParameterType.QueryString);
                }

                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Accept", "application/json");

                request.AddHeader("Authorization", $"Bearer {token}");

                request.AddHeader("Connection", "keep-alive");

                var response = await client.ExecuteAsync(request);

                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<string> GetRequestToken()
        {
            try
            {
                var plainTextBytes = Encoding.UTF8.GetBytes($"{clientId}:{secret}");
                var base64 = Convert.ToBase64String(plainTextBytes);

                var client = new RestClient()
                {
                    AcceptedContentTypes = new[] { "application/json", "text/json", "text/x-json", "text/javascript" }
                };
                var request = new RestRequest(tokenUrl, Method.Post);

                request.Timeout = -1;

                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddHeader("Authorization", $"Basic {base64}");
                request.AddParameter("grant_type", "client_credentials");
                request.AddParameter("scope", "default/gloki_scope");

                var tokenResponse = await client.ExecuteAsync(request);

                if (tokenResponse.StatusCode == HttpStatusCode.OK)
                {
                    string token = JToken.Parse(tokenResponse.Content.ToString())["access_token"].ToString();
                    Environment.SetEnvironmentVariable("OAUTH_TOKEN_MGMT", token, EnvironmentVariableTarget.Process);
                    return token;
                }
                else
                {
                    Environment.SetEnvironmentVariable("OAUTH_TOKEN_MGMT", null, EnvironmentVariableTarget.Process);
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
    }
}
