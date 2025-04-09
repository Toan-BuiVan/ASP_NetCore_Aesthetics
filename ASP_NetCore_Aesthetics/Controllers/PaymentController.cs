using Aesthetics.DataAccess.NetCore.Repositories.Interface;
using Aesthetics.DataAccess.NetCore.Repositories.Interfaces;
using Aesthetics.DTO.NetCore.DataObject.Model.Momo;
using Aesthetics.DTO.NetCore.DataObject.Model.VnPay;
using ASP_NetCore_Aesthetics.Services.MomoServices;
using ASP_NetCore_Aesthetics.Services.VnPaySevices;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ASP_NetCore_Aesthetics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
		private readonly IVnPayService _vnPayService;
		private IMomoService _momoService;
		private IUserRepository _userRepository;
		private IInvoiceRepository _invoiceRepository;
		private IProductsRepository _productsRepository;
		public PaymentController(IVnPayService vnPayService, IMomoService momoService, 
			IUserRepository userRepository, IInvoiceRepository invoiceRepository, IProductsRepository productsRepository)
		{
			_vnPayService = vnPayService;
			_momoService = momoService;
			_userRepository = userRepository;
			_invoiceRepository = invoiceRepository;
			_productsRepository = productsRepository;
		}
		[HttpPost("CreatePaymentUrlVnPay")]
		public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
		{
			var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

			return Redirect(url);
		}

		[HttpGet("CallBackVnPay")]
		public async Task<IActionResult> PaymentCallbackVnpay()
		{
			try
			{
				var response = _vnPayService.PaymentExecute(Request.Query);
				if (response.VnPayResponseCode == "00")
				{

					//Lấy OrderInfo trực tiếp từ query string
					string orderInfo = Request.Query["vnp_OrderInfo"];

					//Tìm OrderID từ chuỗi OrderInfo: "OrderID:12345|..."
					var match = Regex.Match(orderInfo, @"OrderID:(\d+)");
					if (!match.Success || !int.TryParse(match.Groups[1].Value, out int orderId))
					{
						return BadRequest("Không tìm được OrderID từ vnp_OrderInfo.");
					}
					await _invoiceRepository.UpdateStatusInvoice(orderId);
					var invoice = await _invoiceRepository.GetInvoiceByInvoiceID(orderId);
					if (invoice != null)
					{
						await _userRepository.UpdateRatingPoints_Customer(invoice.CustomerID ?? 0);
						if (invoice.EmployeeID != null)
						{
							await _userRepository.UpdateSalesPoints(invoice.EmployeeID ?? 0, invoice.TotalMoney ?? 0);
						}
						var InvoiceDetail = await _invoiceRepository.InvoiceDetailByInvoiceID(invoice.InvoiceID);
						await _invoiceRepository.UpdateStatusInvoiceDetail(invoice.InvoiceID);
						if (InvoiceDetail != null && InvoiceDetail.Any())
						{
							for (int i = 0; i < InvoiceDetail.Count; i++)
							{
								var detail = InvoiceDetail[i];
								if (detail.ProductID != null)
								{
									await _productsRepository.UpdateQuantityPro(detail.ProductID ?? 0, detail.TotalQuantityProduct ?? 0);
								}
							}

						}
					}
				}
				return new JsonResult(response);
			}
			catch(Exception ex)
			{
				return Ok(ex.Message);
			}
		}

		[HttpPost]
		[Route("CreatePaymentUrl")]
		public async Task<IActionResult> CreatePaymentUrl(OrderInfoModel model)
		{
			var response = await _momoService.CreatePaymentAsync(model);
			return Redirect(response.PayUrl);
		}

		[HttpGet("CallBackMomo")]
		public IActionResult PaymentCallPack()
		{
			var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
			return Ok(response);
		}
	}
}
