using Aesthetics.DTO.NetCore.DataObject.Model.MoMo;

namespace ASP_NetCore_Aesthetics.Services.MomoServices
{
	public interface IMomoService
	{
		Task<MomoCreatePaymentResponseModel> CreatePaymentMomo(OrderInfoModel model);
		MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
	}
}
