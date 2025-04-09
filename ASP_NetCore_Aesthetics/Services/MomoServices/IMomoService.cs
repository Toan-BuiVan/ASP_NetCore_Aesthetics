using Aesthetics.DTO.NetCore.DataObject.Model.Momo;

namespace ASP_NetCore_Aesthetics.Services.MomoServices
{
	public interface IMomoService
	{
		public Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model);
		MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
	}
}
