using Aesthetics.DTO.NetCore.DataObject.Model.MoMo;
using Aesthetics.DTO.NetCore.DataObject.Model.VnPay;
using ASP_NetCore_Aesthetics.Services.MomoServices;
using ASP_NetCore_Aesthetics.Services.VnPayServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASP_NetCore_Aesthetics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
		private readonly IVnPayService _vnPayService;
		private IMomoService _momoService;
		public PaymentController(IVnPayService vnPayService, IMomoService momoService)
		{

			_vnPayService = vnPayService;
			_momoService = momoService;
		}

		[HttpPost("CreatePaymentUrlVnpay")]
		public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
		{
			var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
			return Redirect(url);
		}
		[HttpGet("CallBackVnPay")]
		public IActionResult PaymentCallbackVnpay()
		{
			try
			{
				var response = _vnPayService.VnPaymentExecute(Request.Query);
				//if (response.VnPayResponseCode == "00")
				//{

				//}
				return new JsonResult(response);
			}
			catch (Exception ex)
			{
				return Ok();
			}
		}

		[HttpPost("CreatePaymentUrlMomo")]
		public async Task<IActionResult> CreatePaymentUrlMomo(OrderInfoModel model)
		{
			var response = await _momoService.CreatePaymentMomo(model);
			return Redirect(response.PayUrl);
		}
		[HttpGet("CallBackMomo")]
		public IActionResult PaymentCallbackMomo()
		{
			var response = _momoService.PaymentExecuteAsync(Request.Query);

			return new JsonResult(response);
		}
	}
}
