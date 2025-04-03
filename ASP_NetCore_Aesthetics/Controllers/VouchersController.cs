using Aesthetics.DataAccess.NetCore.Repositories.Interfaces;
using Aesthetics.DTO.NetCore.DataObject.LogginModel;
using Aesthetics.DTO.NetCore.DataObject.Model;
using Aesthetics.DTO.NetCore.RequestData;
using ASP_NetCore_Aesthetics.Services.IoggerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Text;

namespace ASP_NetCore_Aesthetics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VouchersController : ControllerBase
    {
        private IVouchersRepository _vouchersRepository;
		private readonly IDistributedCache _cache;
		private readonly ILoggerManager _loggerManager;
		public VouchersController(IVouchersRepository vouchersRepository, IDistributedCache cache, 
			ILoggerManager loggerManager)
		{
			_vouchersRepository = vouchersRepository;
			_cache = cache;
			_loggerManager = loggerManager;
		}

		[HttpPost("Insert_Vouchers")]
		public async Task<IActionResult> Insert_Vouchers(VouchersRequest insert_)
		{
			try
			{
				//1.Insert_Vouchers 
				var responseData = await _vouchersRepository.Insert_Vouchers(insert_);
				//2. Lưu log request
				_loggerManager.LogInfo("Insert_Vouchers Request: " + JsonConvert.SerializeObject(insert_));
				//3. Lưu log data response
				_loggerManager.LogInfo("Insert_Vouchers Response data: " + JsonConvert.SerializeObject(responseData.vouchers_Loggins));
				if (responseData.ResponseCode == 1)
				{
					var cacheKey = "GetVouchers_Cache";
					await _cache.RemoveAsync(cacheKey);
				}
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error Insert_Vouchers} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		[HttpPost("Update_Vouchers")]
		public async Task<IActionResult> Update_Vouchers(Update_Vouchers update_)
		{
			try
			{
				//1.Update_Vouchers 
				var responseData = await _vouchersRepository.Update_Vouchers(update_);
				//2. Lưu log request
				_loggerManager.LogInfo("Update_Vouchers Request: " + JsonConvert.SerializeObject(update_));
				//3. Lưu log data response
				_loggerManager.LogInfo("Update_Vouchers Response data: " + JsonConvert.SerializeObject(responseData.vouchers_Loggins));
				if (responseData.ResponseCode == 1)
				{
					var cacheKey = "GetVouchers_Cache";
					await _cache.RemoveAsync(cacheKey);
				}
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error Update_Vouchers} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		[HttpDelete("Delete_Vouchers")]
		public async Task<IActionResult> Delete_Vouchers(Delete_Vouchers delete_)
		{
			try
			{
				//1.Update_Vouchers 
				var responseData = await _vouchersRepository.Delete_Vouchers(delete_);
				//2. Lưu log request
				_loggerManager.LogInfo("Delete_Vouchers Request: " + JsonConvert.SerializeObject(delete_));
				//3. Lưu log data response delete vouchers
				_loggerManager.LogInfo("Delete_Vouchers Response data: " + JsonConvert.SerializeObject(responseData.vouchers_Loggins));
				//4. Lưu log data response delete wallets
				_loggerManager.LogInfo("Delete_Wallets Response data: " + JsonConvert.SerializeObject(responseData.wallets_Loggins));
				if (responseData.ResponseCode == 1)
				{
					var cacheKey = "GetVouchers_Cache";
					await _cache.RemoveAsync(cacheKey);
				}
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error Delete_Vouchers} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		[HttpGet("GetList_SearchVouchers")]
		public async Task<IActionResult> GetList_SearchVouchers(GetList_SearchVouchers getList_) 
		{
			try
			{
				var listVouchers = new List<Vouchers_Loggin>();
				//1. Khóa lưu dữ liệu trong cache
				var cachkey = "GetVouchers_Cache";
				//1.1 Lấy dữ liệu trong cache
				byte[] cachedData = await _cache.GetAsync(cachkey);
				//lưu log request
				_loggerManager.LogInfo("GetList_SearchVouchers Request: " + JsonConvert.SerializeObject(getList_));
				//1.2 Nếu cache có dữ liệu => giải mã trả về client
				if (cachedData != null)
				{
					var cachedDataString = Encoding.UTF8.GetString(cachedData);
					listVouchers = JsonConvert.DeserializeObject<List<Vouchers_Loggin>>(cachedDataString);

					//1.3 Lọc dữ liệu khi có request
					listVouchers = listVouchers.Where(s =>
						(getList_.VoucherID == null || (getList_.VoucherID == s.VoucherID && s.IsActive == 1))
						&&(getList_.RankMember == null || getList_.RankMember == s.RankMember && s.IsActive == 1)
						&& (
							(getList_.StartDate == null && getList_.EndDate == null) ||
							(getList_.StartDate != null && getList_.EndDate != null &&
								s.StartDate.Value >= getList_.StartDate &&
								s.StartDate.Value <= getList_.EndDate.Value.AddDays(1)) ||
							(getList_.StartDate != null && getList_.EndDate == null &&
								s.StartDate >= getList_.StartDate) ||
							(getList_.StartDate == null && getList_.EndDate != null &&
								s.EndDate < getList_.EndDate.Value.AddDays(1))
							)
					).ToList();

					//1.4 Lưu log dữ liệu cache trả về
					_loggerManager.LogInfo("GetList_SearchVouchers cache: " + cachedDataString);

					//1.5 Lưu log dữ liệu khi có request 
					_loggerManager.LogInfo("GetList_SearchVouchers cache Request: " + JsonConvert.SerializeObject(listVouchers));
					return Ok(listVouchers);
				}
				else
				{
					//2. Nếu cache không có dữ liệu gọi vào db
					var responseData = await _vouchersRepository.GetList_SearchVouchers(getList_);
					listVouchers = responseData?.Data?.ToList();

					//2.2. Chuyển danh sách dịch vụ thành chuỗi JSON để lưu vào cache
					var cachedDataString = JsonConvert.SerializeObject(listVouchers);
					var dataToCache = Encoding.UTF8.GetBytes(cachedDataString);

					//2.3 Cấu hình thời gian hết hạn cho Redis Cache
					DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
						.SetAbsoluteExpiration(DateTime.Now.AddMinutes(5))
						.SetSlidingExpiration(TimeSpan.FromMinutes(3));

					//2.4. Lưu log dữ liệu Vouchers trong db
					_loggerManager.LogInfo("GetList_SearchVouchers db: " + cachedDataString);

					//2.5. Kiểm tra nếu lấy thành công danh sách thì lưu cache
					if (responseData.ResponseCode == 1)
					{
						await _cache.SetAsync(cachkey, dataToCache, options);
					}
					return Ok(responseData);
				}
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error GetList_SearchVouchers} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}
	}
}
