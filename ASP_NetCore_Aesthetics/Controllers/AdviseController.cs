using Aesthetics.DataAccess.NetCore.Repositories.Interfaces;
using Aesthetics.DTO.NetCore.RequestData;
using ASP_NetCore_Aesthetics.Services.IoggerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ASP_NetCore_Aesthetics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdviseController : ControllerBase
    {
        private IAdviseRepository _adviseRepository;
		private readonly ILoggerManager _loggerManager;
		public AdviseController(IAdviseRepository adviseRepository, ILoggerManager loggerManager)
		{
			_adviseRepository = adviseRepository;
			_loggerManager = loggerManager;
		}

		[HttpPost("Insert_Advise")]
		public async Task<IActionResult> Insert_Advise(AdviseRequest adviseRequest)
		{
			try
			{
				var responseData = await _adviseRepository.InsertAdvise(adviseRequest);
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error Insert_Advise} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		[HttpPost("Update_Advise")]
		public async Task<IActionResult> Update_Advise(Update_Advise update_Advise)
		{
			try
			{
				var responseData = await _adviseRepository.UpdateAdvise(update_Advise);
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error Update_Advise} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		[HttpDelete("Delete_Advise")]
		public async Task<IActionResult> Delete_Advise(Update_Advise delete_)
		{
			try
			{
				var responseData = await _adviseRepository.DeleteAdvise(delete_);
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error Delete_Advise} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		[HttpGet("GetList_SearchAdvise")]
		public async Task<IActionResult> GetList_SearchAdvise(GetLisr_SearchAdvise getLisr_)
		{
			try
			{
				var responseData = await _adviseRepository.GetList_SreachAdvise(getLisr_);
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error GetList_SearchAdvise} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}
	}
}
