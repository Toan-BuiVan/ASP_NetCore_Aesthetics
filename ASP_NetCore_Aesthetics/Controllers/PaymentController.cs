using Aesthetics.DTO.NetCore.DataObject.Model.VnPay;
using ASP_NetCore_Aesthetics.Services.VnPaySevices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASP_NetCore_Aesthetics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
		private readonly IVnPayService _vnPayService;
		public PaymentController(IVnPayService vnPayService)
		{
			_vnPayService = vnPayService;
		}
		[HttpPost]
		public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
		{
			var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

			return Redirect(url);
		}
		[HttpGet]
		public IActionResult PaymentCallbackVnpay()
		{
			var response = _vnPayService.PaymentExecute(Request.Query);
			return new JsonResult(response);
		}

	}
}
