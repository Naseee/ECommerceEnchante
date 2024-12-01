using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace ECommerceApp.Settings
{
    public class EmailSender : IEmailSender
    {
        public string SendGridSecret { get; set; }
        public EmailSender(IConfiguration _config)
        {
            SendGridSecret = _config.GetValue<string>("SendGridSettings:SecretKey");
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SendGridClient(SendGridSecret);
            var from = new EmailAddress("nasiya864@gmail.com", "ECommerceApp");
            var to = new EmailAddress(email);
            var message = MailHelper.CreateSingleEmail(from, to, subject, "", htmlMessage);
            return client.SendEmailAsync(message);
        }



    }

}
