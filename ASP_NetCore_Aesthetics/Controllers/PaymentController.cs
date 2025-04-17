using Aesthetics.DataAccess.NetCore.Repositories.Interface;
using Aesthetics.DataAccess.NetCore.Repositories.Interfaces;
using Aesthetics.DTO.NetCore.DataObject.Model.Momo;
using Aesthetics.DTO.NetCore.DataObject.Model.VnPay;
using ASP_NetCore_Aesthetics.Services.MomoServices;
using ASP_NetCore_Aesthetics.Services.SenderMail;
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
		private IEmailSender _emailSender;
		public PaymentController(IVnPayService vnPayService, IMomoService momoService, 
			IUserRepository userRepository, IInvoiceRepository invoiceRepository, 
			IProductsRepository productsRepository, IEmailSender emailSender)
		{
			_vnPayService = vnPayService;
			_momoService = momoService;
			_userRepository = userRepository;
			_invoiceRepository = invoiceRepository;
			_productsRepository = productsRepository;
			_emailSender = emailSender;
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
					var invoice = await _invoiceRepository.GetInvoiceByInvoiceID(orderId);
					if (invoice != null)
					{
						//1. Update trạng thái của háo đơn
						await _invoiceRepository.UpdateStatusInvoice(orderId);
						//2. Update RatingPoints của khách hàng
						await _userRepository.UpdateRatingPoints_Customer(invoice.CustomerID ?? 0);
						if (invoice.EmployeeID != null)
						{
							//3.Nếu hóa đơn bán có tồn tại nhân vên thì update SalesPoints của nhân viên
							await _userRepository.UpdateSalesPoints(invoice.EmployeeID ?? 0, invoice.TotalMoney ?? 0);
						}
						//4. Update trạng thái của chi tiết hóa đơn
						await _invoiceRepository.UpdateStatusInvoiceDetail(invoice.InvoiceID);

						var InvoiceDetail = await _invoiceRepository.InvoiceDetailByInvoiceID(invoice.InvoiceID);
						if (InvoiceDetail != null && InvoiceDetail.Any())
						{
							for (int i = 0; i < InvoiceDetail.Count; i++)
							{
								var detail = InvoiceDetail[i];
								if (detail.ProductID != null)
								{
									//5. Update lại số lượng của sản phẩm tại của hàng
									await _productsRepository.UpdateQuantityPro(detail.ProductID ?? 0, detail.TotalQuantityProduct ?? 0);
								}
							}
						}
						//6. Gửi mail thông báo xác nhận hóa đơn đã thanh toán thành công
						var customer = await _userRepository.GetUserByUserID(invoice.CustomerID);
						if (customer != null)
						{
							var emailCustomer = customer.Email; 
							if (emailCustomer != null)
							{
								var subject = "Thanh Toán Hóa Đơn Thành Công!";
								var message = $"Kính gửi Quý Khách," +
											  $"\n\nChúng tôi xin thông báo: Hóa đơn {orderId}." +
											  $"\nTổng số tiền thanh toán: {invoice.TotalMoney:N0} VND." +
											  $"\n\nChân thành cảm ơn Quý Khách đã tin tưởng và sử dụng dịch vụ của chúng tôi." +
											  $"\n\nTrân trọng!";
								await _emailSender.SendEmailAsync(emailCustomer, subject, message);
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
