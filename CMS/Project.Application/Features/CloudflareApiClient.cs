using System.Net.Http.Json;

namespace Project.Application.Features
{
    public class CloudflareApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _email;

        public CloudflareApiClient(string apiKey, string email)
        {
            _apiKey = apiKey;
            _email = email;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://api.cloudflare.com/client/v4/");
            _httpClient.DefaultRequestHeaders.Add("X-Auth-Email", _email);
            _httpClient.DefaultRequestHeaders.Add("X-Auth-Key", _apiKey);
        }

        public async Task<string> GetZoneId(string domainName)
        {
            string endpoint = $"zones?name={domainName}";
            HttpResponseMessage response = await _httpClient.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to retrieve zone ID. Status code: {response.StatusCode}");
            }

            CloudflareZoneResponse zoneResponse = await response.Content.ReadFromJsonAsync<CloudflareZoneResponse>();
            if (zoneResponse.ResultInfo.Count == 0)
            {
                throw new Exception($"No zone found for domain '{domainName}'.");
            }

            return zoneResponse.Result[0].Id;
        }
    }

    public class CloudflareZoneResponse
    {
        public List<CloudflareZone> Result { get; set; }
        public ResultInfo ResultInfo { get; set; }
    }

    public class CloudflareZone
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class ResultInfo
    {
        public int Count { get; set; }
    }
}
