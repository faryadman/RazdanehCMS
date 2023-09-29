using CloudFlare.NET;
using Newtonsoft.Json;
using System.Text;

namespace Project.Application.Features
{
    public class CloudflareApiClient
    {

        public async Task UpdateDnsRecordAsync(string zoneId, string recordId, string newCname, string cnameContent,
            string apiKey, string email)
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
                proxied = false,
                comment = DateTime.Now.ToString()
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

        public async Task CreateDnsRecordAsync(string zoneId, string newCname, string cnameContent, string apiKey,
            string email)
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
                proxied = false,
                comment = DateTime.Now.ToString()
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

        public async Task<IList<CnameRecord>> GetAllRecords(string zoneId, string apiKey, string email)
        {
            try
            {
                // ساخت URL API برای دریافت تمام رکوردهای CNAME در منطقه
                var apiUrl = $"https://api.cloudflare.com/client/v4/zones/{zoneId}/dns_records?type=CNAME";
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("X-Auth-Email", email);
                httpClient.DefaultRequestHeaders.Add("X-Auth-Key", apiKey);

                var response = await httpClient.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode) return new List<CnameRecord>();
                var content = await response.Content.ReadAsStringAsync();
                var records = JsonConvert.DeserializeObject<CnameRecords>(content);
                return records.result.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return null;
            }
        }
        public async Task DeleteCnameRecords(string zoneId, string recordToDeleteId, string apiKey, string email)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("X-Auth-Email", email);
                httpClient.DefaultRequestHeaders.Add("X-Auth-Key", apiKey);
                var deleteUrl = $"https://api.cloudflare.com/client/v4/zones/{zoneId}/dns_records/{recordToDeleteId}";
                await httpClient.DeleteAsync(deleteUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }

        public class CnameRecords
        {
            public bool success { get; set; }
            public List<CnameRecord> result { get; set; }
        }

        public class CnameRecord
        {
            public string id { get; set; }

            public string name { get; set; }

            public string comment { get; set; }

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
}