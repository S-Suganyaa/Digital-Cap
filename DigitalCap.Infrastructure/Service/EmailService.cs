using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Infrastructure.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Infrastructure.Service
{
    public class EmailService : IEmailService
    {
        private readonly IEmailRepository _emailRepository;
        public EmailService(IEmailRepository emailRepository)
        {
            _emailRepository = emailRepository;
        }
        public async Task SendEmailAsync(string from, string fromPw, string to, string subject, string body)
        {
            await _emailRepository.SendEmailAsync(from, fromPw, to, subject, body);
        }
        public async Task SendEmailAsync(string from, string fromPw, IEnumerable<string> to, string subject, string body, string htmlMessage)
        {
            await _emailRepository.SendEmailAsync(from, fromPw, to, subject, body, htmlMessage);
        }
        public async Task SendEmailAsync(IEnumerable<string> to, string subject, string body, string htmlMessage)
        {
            await _emailRepository.SendEmailAsync(to, subject, body, htmlMessage);
        }
        public async Task SendEmailAsyncWithCC(string to, string cc, string subject, string body, string htmlMessage)
        {
            await _emailRepository.SendEmailAsyncWithCC(to, cc, subject, body, htmlMessage);
        }
        public async Task SendEmailAsync(string from, string fromPw, IEnumerable<string> to, string subject, string body)
        {
            await _emailRepository.SendEmailAsync(from, fromPw, to, subject, body);
        }
        public async Task SendEmailAsync(IEnumerable<string> to, string subject, string body)
        {
            await _emailRepository.SendEmailAsync(to, subject, body);
        }
        public async Task SendEmailAsync(string email, string subject, string body)
        {
            await _emailRepository.SendEmailAsync(email, subject, body);
        }
        public async Task SendEmailAsync(string email, string subject, string body, string htmlMessage)
        {
            await _emailRepository.SendEmailAsync(email, subject, body, htmlMessage);
        }
    }
}



    


