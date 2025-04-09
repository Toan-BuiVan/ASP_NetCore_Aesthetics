using Aesthetics.DTO.NetCore.DataObject.Model.VnPay;

namespace ASP_NetCore_Aesthetics.Services.VnPaySevices
{
	public interface IVnPayService
	{
		string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
		PaymentResponseModel PaymentExecute(IQueryCollection collections);
	}
}
