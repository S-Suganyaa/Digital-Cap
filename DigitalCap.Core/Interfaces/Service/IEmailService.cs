using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Core.Interfaces.Service
{
    public interface IEmailService
    {
        Task SendEmailAsync(string from, string fromPw, string to, string subject, string body);
        Task SendEmailAsync(string from, string fromPw, IEnumerable<string> to, string subject, string body, string htmlMessage);
        Task SendEmailAsync(IEnumerable<string> to, string subject, string body, string htmlMessage);
        Task SendEmailAsync(string from, string fromPw, IEnumerable<string> to, string subject, string body);
        Task SendEmailAsync(IEnumerable<string> to, string subject, string body);
        Task SendEmailAsync(string email, string subject, string body);
        Task SendEmailAsync(string email, string subject, string body, string htmlMessage);
        Task SendEmailAsyncWithCC(string to, string cc, string subject, string body, string htmlMessage);
    }
}



