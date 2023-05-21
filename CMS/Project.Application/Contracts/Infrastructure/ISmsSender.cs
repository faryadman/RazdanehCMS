using Project.Application.Models;

namespace Project.Application.Contracts.Infrastructure
{
    public interface ISmsSender
    {
        Task<bool> SendWithPattern(SmsWithPattern smsWithPattern);
        bool SimpleSend(SimpleSms simpleSms);
        bool SimpleSend(string to, string content);
    }
}
