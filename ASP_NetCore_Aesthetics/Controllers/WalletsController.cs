using Aesthetics.DataAccess.NetCore.Repositories.Implement;
using Aesthetics.DataAccess.NetCore.Repositories.Interfaces;
using Aesthetics.DTO.NetCore.RequestData;
using ASP_NetCore_Aesthetics.Services.IoggerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace ASP_NetCore_Aesthetics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletsController : ControllerBase
    {
        private IWalletsRepository _walletsRepository;
        private readonly IDistributedCache _cache;
        private readonly ILoggerManager _loggerManager;
		public WalletsController(IWalletsRepository walletsRepository, IDistributedCache cache, 
            ILoggerManager loggerManager)
		{
			_walletsRepository = walletsRepository;
            _cache = cache;
            _loggerManager = loggerManager;
		}

		[HttpPost("Insert_Wallets")]
		public async Task<IActionResult> Insert_Wallets(WalletsRequest insert_)
		{
			try
			{
				//1.Insert_Wallets 
				var responseData = await _walletsRepository.Insert_Wallets(insert_);
				//2. Lưu log request
				_loggerManager.LogInfo("Insert_Wallets Request: " + JsonConvert.SerializeObject(insert_));
				//3. Lưu log data response
				_loggerManager.LogInfo("Insert_Wallets Response data: " + JsonConvert.SerializeObject(responseData.wallets_Loggins));
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error Insert_Wallets} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		[HttpPost("RedeemPointsForVoucher")]
		public async Task<IActionResult> RedeemPointsForVoucher(RedeemVouchers _redeem)
		{
			try
			{
				//1.RedeemPointsForVoucher 
				var responseData = await _walletsRepository.RedeemPointsForVoucher(_redeem);
				//2. Lưu log request
				_loggerManager.LogInfo("RedeemPointsForVoucher Request: " + JsonConvert.SerializeObject(_redeem));
				//3. Lưu log data response
				_loggerManager.LogInfo("RedeemPointsForVoucher Response data: " + JsonConvert.SerializeObject(responseData.wallets_Loggins));
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error RedeemPointsForVoucher} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		[HttpPost("Update_Wallets")]
		public async Task<IActionResult> Update_Wallets(Update_Wallest update_)
		{
			try
			{
				//1.Update_Wallets 
				var responseData = await _walletsRepository.Update_Wallets(update_);
				//2. Lưu log request
				_loggerManager.LogInfo("Update_Wallets Request: " + JsonConvert.SerializeObject(update_));
				//3. Lưu log data response
				_loggerManager.LogInfo("Update_Wallets Response data: " + JsonConvert.SerializeObject(responseData.wallets_Loggins));
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error Update_Wallets} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		[HttpDelete("Delete_Wallets")]
		public async Task<IActionResult> Delete_Wallets(Delete_Wallest delete_)
		{
			try
			{
				//1.Update_Wallets 
				var responseData = await _walletsRepository.Delete_Wallets(delete_);
				//2. Lưu log request
				_loggerManager.LogInfo("Delete_Wallets Request: " + JsonConvert.SerializeObject(delete_));
				//3. Lưu log data response
				_loggerManager.LogInfo("Delete_Wallets Response data: " + JsonConvert.SerializeObject(responseData.wallets_Loggins));
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error Delete_Wallets} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		[HttpGet("GetList_SearchWallets")]
		public async Task<IActionResult> GetList_SearchWallets(GetList_SearchWallets getList_)
		{
			try
			{
				//1.GetList_SearchWallets 
				var responseData = await _walletsRepository.GetList_SearchWallets(getList_);
				//2. Lưu log request
				_loggerManager.LogInfo("GetList_SearchWallets Request: " + JsonConvert.SerializeObject(getList_));
				//3. Lưu log data response
				_loggerManager.LogInfo("GetList_SearchWallets Response data: " + JsonConvert.SerializeObject(responseData.wallets_Loggins));
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error GetList_SearchWalletss} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}
	}
}
