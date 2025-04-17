using System.Net.Mail;
using System.Net;

namespace ASP_NetCore_Aesthetics.Services.SenderMail
{
	public class EmailSender : IEmailSender
	{
		public Task SendEmailAsync(string email, string subject, string message)
		{
			var client = new SmtpClient("smtp.gmail.com", 587)
			{
				EnableSsl = true, //bật bảo mật
				UseDefaultCredentials = false,
				Credentials = new NetworkCredential("buitoan042004@gmail.com", "kdlsufcquylmxcwt")
			};

			return client.SendMailAsync(
				new MailMessage(from: "buitoan042004@gmail.com",
								to: email,
								subject,
								message
								));
		}
	}
}
