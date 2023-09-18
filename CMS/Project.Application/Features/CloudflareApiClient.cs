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

        public async Task<bool> DeleteCnameRecords(string zoneId, string apiKey, string email)
        {
            try
            {
                // ساخت URL API برای دریافت تمام رکوردهای CNAME در منطقه
                var apiUrl = $"https://api.cloudflare.com/client/v4/zones/{zoneId}/dns_records?type=CNAME";
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("X-Auth-Email", email);
                httpClient.DefaultRequestHeaders.Add("X-Auth-Key", apiKey);

                var response = await httpClient.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var records = JsonConvert.DeserializeObject<CnameRecords>(content);
                    var recordsToDelete = records.result.ToList();

                    foreach (var recordToDelete in recordsToDelete)
                    {
                        //var modifiedDateTime = DateTime.Parse(recordToDelete.modified_on);
                        //var expireTimeInMinutes = double.Parse(expireTimeOn);
                        //var expireTimeSpan = TimeSpan.FromMinutes(expireTimeInMinutes);

                        //var currentTime = DateTime.Now;
                        //var timeDifference = currentTime - modifiedDateTime;

                        //if (timeDifference >= expireTimeSpan)
                        //{
                        var deleteUrl = $"https://api.cloudflare.com/client/v4/zones/{zoneId}/dns_records/{recordToDelete.id}";
                        await httpClient.DeleteAsync(deleteUrl);
                        //}
                    }

                    return true;
                }
                else
                {
                    Console.WriteLine($"Failed to Fetch CNAME Records, Status Code: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return false;
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

            public string modified_on { get; set; }

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