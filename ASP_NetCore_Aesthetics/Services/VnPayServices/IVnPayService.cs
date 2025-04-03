using Aesthetics.DTO.NetCore.DataObject.Model.VnPay;

namespace ASP_NetCore_Aesthetics.Services.VnPayServices
{
	public interface IVnPayService
	{
		string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
		PaymentResponseModel VnPaymentExecute(IQueryCollection collections);
	}
}
