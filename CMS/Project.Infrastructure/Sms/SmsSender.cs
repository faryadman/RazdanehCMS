using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Project.Application.Contracts.Infrastructure;
using Project.Application.Models;
using RestSharp;

namespace Project.Infrastructure.Sms
{
    public class SmsSender : ISmsSender
    {
        private readonly SmsSettings _smsSettings;
        private readonly RestClient client;
        public SmsSender(IOptions<SmsSettings> smsSettings)
        {
            _smsSettings = smsSettings.Value;
            client = new RestClient(_smsSettings.ApiUrl);
        }

        public async Task<bool> SendWithPattern(SmsWithPattern smsWithPattern)
        {
            var request = new RestRequest
            {
                Method = Method.Post
            };
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter(
                "undefined",
                JsonConvert.SerializeObject(new
                {
                    op = "pattern",
                    user = _smsSettings.Username,
                    pass = _smsSettings.Password,
                    fromNum = _smsSettings.FromNumber,
                    toNum = smsWithPattern.To,
                    patternCode = smsWithPattern.PatternCode,
                    inputData = JsonConvert.SerializeObject(smsWithPattern.PatternData)
                }),
                ParameterType.RequestBody);

            var cancellationTokenSource = new CancellationTokenSource();

            var response = await client.ExecuteAsync(request, cancellationTokenSource.Token);

            return true;
        }

        public bool SimpleSend(SimpleSms simpleSms)
        {
            return true;
        }

        public bool SimpleSend(string to, string content)
        {
            return true;
        }

    }
}
