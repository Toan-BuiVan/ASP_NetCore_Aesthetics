namespace ASP_NetCore_Aesthetics.Services.SenderMail
{
	public interface IEmailSender
	{
		Task SendEmailAsync(string email, string subject, string message);
	}
}
