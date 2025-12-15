using System.Threading.Tasks;

using System.Threading.Tasks;

namespace VeilleNet.Services.Tools
{
    public interface IEmailService
    {
        Task SendEmailAsync(string subject, string body, string toEmail);
    }
}