using Aesthetics.DataAccess.NetCore.Repositories.Implement;
using Aesthetics.DataAccess.NetCore.Repositories.Interface;
using Aesthetics.DataAccess.NetCore.Repositories.Interfaces;
using Aesthetics.DTO.NetCore.DataObject.Model;
using Aesthetics.DTO.NetCore.RequestData;
using ASP_NetCore_Aesthetics.Services.IoggerServices;
using ASP_NetCore_Aesthetics.Services.SenderMail;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace ASP_NetCore_Aesthetics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private IInvoiceRepository _invoiceRepository;
		private readonly IDistributedCache _cache;
		private readonly ILoggerManager _loggerManager;
		private readonly IEmailSender _emailSender;
		private IUserRepository _userRepository;
		public InvoiceController(IInvoiceRepository invoiceRepository, IDistributedCache cache
			, ILoggerManager loggerManager, IEmailSender emailSender, IUserRepository userRepository)
		{
			_invoiceRepository = invoiceRepository;
			_cache = cache;
			_loggerManager = loggerManager;
			_emailSender = emailSender;
			_userRepository = userRepository;
		}

		[HttpPost("Insert_Invoice")]
		public async Task<IActionResult> Insert_Invoice(InvoiceRequest insert_)
		{
			try
			{
				//1.Insert_Invoice 
				var responseData = await _invoiceRepository.Insert_Invoice(insert_);
				//2. Lưu log request
				_loggerManager.LogInfo("Insert_Invoice Request: " + JsonConvert.SerializeObject(insert_));
				//3. Lưu log data Insert_Invoice response
				_loggerManager.LogInfo("Insert_Invoice Response data: " + JsonConvert.SerializeObject(responseData.invoiceOut_Loggin));
				//3. Lưu log data Insert_InvoiceDetail response
				_loggerManager.LogInfo("Insert_InvoiceDetail Response data: " + JsonConvert.SerializeObject(responseData.invoiceDetailOut_Loggin));
				//4. Gửi mail thông báo xác nhận đặt hàng thành công
				var customer = await _userRepository.GetUserByUserID(insert_.CustomerID);
				if (customer != null)
				{
					var emailCustomer = customer.Email;
					if (emailCustomer != null)
					{
						var subject = "Đặt Hàng Thành Công!";
						var message = $"Kính gửi Quý Khách," +
									  $"\n\nChúng tôi xin thông báo: Hóa đơn {responseData.InvoiceID} của Quý Khách đã được tạo thành công." +
									  $"\nTổng số tiền thanh toán: {responseData.TotalMoney:N0} VND." +
									  $"\n\nChân thành cảm ơn Quý Khách đã tin tưởng và sử dụng dịch vụ của chúng tôi." +
									  $"\n\nQuý Khách sẽ nhận được thông tin về đơn hàng và quá trình giao hàng trong thời gian sớm nhất." +
									  $"\n\nTrân trọng!";

						await _emailSender.SendEmailAsync(emailCustomer, subject, message);
					}
				}
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error Insert_Invoice} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		[HttpDelete("Delete_Invoice")]
		public async Task<IActionResult> Delete_Invoice(Delete_Invoice delete_)
		{
			try
			{
				//1.Delete_Invoice 
				var responseData = await _invoiceRepository.Delete_Invoice(delete_);
				//2. Lưu log request
				_loggerManager.LogInfo("Delete_Invoice Request: " + JsonConvert.SerializeObject(delete_));
				//3. Lưu log data Delete_Invoice response
				_loggerManager.LogInfo("Delete_Invoice Response data: " + JsonConvert.SerializeObject(responseData.invoiceOut_Loggin));
				//3. Lưu log data Delete_InvoiceDetail response
				_loggerManager.LogInfo("Delete_InvoiceDetail Response data: " + JsonConvert.SerializeObject(responseData.invoiceDetailOut_Loggin));
				
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error Delete_Invoice} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		[HttpGet("GetList_SearchInvoice")]
		public async Task<IActionResult> GetList_SearchInvoice(GetList_Invoice getList_)
		{
			try
			{
				//1.GetList_SearchInvoice 
				var responseData = await _invoiceRepository.GetList_SearchInvoice(getList_);
				//2. Lưu log request
				_loggerManager.LogInfo("Delete_Invoice Request: " + JsonConvert.SerializeObject(getList_));
				//3. Lưu log data GetList_SearchInvoice response
				_loggerManager.LogInfo("GetList_SearchInvoice Response data: " + JsonConvert.SerializeObject(responseData.Data));
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error GetList_SearchInvoice} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		[HttpGet("GetList_SearchInvoiceDetail")]
		public async Task<IActionResult> GetList_SearchInvoiceDetail(GetList_InvoiceDetail getList_)
		{
			try
			{
				//1.GetList_SearchInvoiceDetail 
				var responseData = await _invoiceRepository.GetList_SearchInvoiceDetail(getList_);
				//2. Lưu log request
				_loggerManager.LogInfo("Delete_Invoice Request: " + JsonConvert.SerializeObject(getList_));
				//3. Lưu log data GetList_SearchInvoiceDetail response
				_loggerManager.LogInfo("GetList_SearchInvoice Response data: " + JsonConvert.SerializeObject(responseData.Data));
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error GetList_SearchInvoice} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}
	}
}
