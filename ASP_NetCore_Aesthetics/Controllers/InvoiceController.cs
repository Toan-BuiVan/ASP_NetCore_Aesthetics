using Aesthetics.DataAccess.NetCore.Repositories.Implement;
using Aesthetics.DataAccess.NetCore.Repositories.Interface;
using Aesthetics.DataAccess.NetCore.Repositories.Interfaces;
using Aesthetics.DTO.NetCore.RequestData;
using ASP_NetCore_Aesthetics.Loggin;
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
		public InvoiceController(IInvoiceRepository invoiceRepository, IDistributedCache cache
			, ILoggerManager loggerManager)
		{
			_invoiceRepository = invoiceRepository;
			_cache = cache;
			_loggerManager = loggerManager;
		}

		[HttpPost("Insert_Invoice")]
		public async Task<IActionResult> Insert_Invoice(InvoiceRequest insert_)
		{
			try
			{
				//1.Insert_Supplier 
				var responseData = await _invoiceRepository.Insert_Invoice(insert_);
				//2. Lưu log request
				_loggerManager.LogInfo("Insert_Invoice Request: " + JsonConvert.SerializeObject(insert_));
				//3. Lưu log data Insert_Invoice response
				_loggerManager.LogInfo("Insert_Invoice Response data: " + JsonConvert.SerializeObject(responseData.invoiceOut_Loggin));
				//3. Lưu log data Insert_InvoiceDetail response
				_loggerManager.LogInfo("Insert_InvoiceDetail Response data: " + JsonConvert.SerializeObject(responseData.invoiceDetailOut_Loggin));
				//if (responseData.ResponseCode == 1)
				//{
				//	var cacheKey = "GetSupplier_Cache";
				//	await _cache.RemoveAsync(cacheKey);
				//}
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error Insert_Invoice} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}
	}
}
