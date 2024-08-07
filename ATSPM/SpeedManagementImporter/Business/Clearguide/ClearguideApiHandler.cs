using Newtonsoft.Json;
using System.Net;

namespace SpeedManagementImporter.Business.Clearguide
{
    public class ClearguideApiHandler
    {
        private readonly string username;
        private readonly string password;
        private string refresh_token;
        private string access_token;

        private HttpClient httpClient;

        public ClearguideApiHandler(string username, string password)
        {
            this.username = username;
            this.password = password;
            this.refresh_token = null;
            this.access_token = null;
            this.httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(20);
            Authenticate();
        }

        private void Authenticate()
        {
            var data = new Dictionary<string, string>
        {
            { "username", username },
            { "password", password }
        };

            var response = httpClient.PostAsync("https://auth.iteris-clearguide.com/api/token/", new FormUrlEncodedContent(data)).Result;

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
                var responseJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);
                refresh_token = responseJson.GetValueOrDefault("refresh");
                access_token = responseJson.GetValueOrDefault("access");
            }
            else
            {
                throw new Exception($"Error while authenticating... Status Code: {response.StatusCode} Message: {response.Content.ReadAsStringAsync().Result}");
            }
        }

        private void RefreshAccessToken()
        {
            var data = new Dictionary<string, string>
        {
            { "refresh", refresh_token }
        };

            var response = httpClient.PostAsync("https://auth.iteris-clearguide.com/api/token/refresh/", new FormUrlEncodedContent(data)).Result;

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
                var responseJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);
                access_token = responseJson.GetValueOrDefault("access");
            }
            else
            {
                Authenticate();
            }
        }

        public Dictionary<string, string> AuthHeader
        {
            get
            {
                return new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {access_token}" }
            };
            }
        }

        public async Task<string?> CallAsync(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", $"Bearer {access_token}");

            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                RefreshAccessToken();
                return await CallAsync(url);
            }
            else
            {
                return await response.Content.ReadAsStringAsync();
                //throw new Exception($"Error fetching response from ClearGuide... Status Code: {response.StatusCode} Message: {await response.Content.ReadAsStringAsync()}");
            }
        }
    }
}
