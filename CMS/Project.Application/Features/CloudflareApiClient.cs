using CloudFlare.NET;
using System.Text;

namespace Project.Application.Features
{
    public class CloudflareApiClient
    {

        public async Task UpdateDnsRecordAsync(string zoneId, string recordId, string newCname, string cnameContent, string apiKey, string email)
        {

            var apiUrl = $"https://api.cloudflare.com/client/v4/zones/{zoneId}/dns_records/{recordId}";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("X-Auth-Email", email);
            httpClient.DefaultRequestHeaders.Add("X-Auth-Key", apiKey);

            var requestBody = new
            {
                type = DnsRecordType.CNAME,
                name = newCname,
                content = cnameContent,
                ttl = 1,
                proxied = false
            };

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PutAsync(apiUrl, content);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("DNS record updated successfully.");
            }
            else
            {
                Console.WriteLine("Failed to update DNS record. Status code: " + response.StatusCode);
            }
        }

        public async Task CreateDnsRecordAsync(string zoneId, string newCname, string cnameContent, string apiKey, string email)
        {
            var apiUrl = $"https://api.cloudflare.com/client/v4/zones/{zoneId}/dns_records";

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("X-Auth-Email", email);
            httpClient.DefaultRequestHeaders.Add("X-Auth-Key", apiKey);

            var requestBody = new
            {
                type = DnsRecordType.CNAME,
                name = newCname,
                content = cnameContent,
                ttl = 1,
                proxied = false
            };

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(apiUrl, content);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("DNS record created successfully.");
            }
            else
            {
                Console.WriteLine("Failed to create DNS record. Status code: " + response.StatusCode);
            }
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
