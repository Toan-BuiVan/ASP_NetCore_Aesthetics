using Aesthetics.DataAccess.NetCore.CheckConditions.Response;
using Aesthetics.DataAccess.NetCore.DBContext;
using Aesthetics.DataAccess.NetCore.Repositories.Interfaces;
using Aesthetics.DTO.NetCore.DataObject.LogginModel;
using Aesthetics.DTO.NetCore.DataObject.Model;
using Aesthetics.DTO.NetCore.RequestData;
using Aesthetics.DTO.NetCore.Response;
using Aesthetics.DTO.NetCore.ResponseAdvise;
using BE_102024.DataAces.NetCore.CheckConditions;
using BE_102024.DataAces.NetCore.Dapper;
using Dapper;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DataAccess.NetCore.Repositories.Implement
{
	public class AdviseRepository : BaseApplicationService, IAdviseRepository
	{
		private DB_Context _context;
		private IConfiguration _configuration;
		public AdviseRepository(DB_Context context, IConfiguration configuration, IServiceProvider serviceProvider)
			: base(serviceProvider)
		{
			_context = context;
			_configuration = configuration;
		}

		public async Task<ResponseData> InsertAdvise(AdviseRequest adviseRequest)
		{
			var returnData = new ResponseData();
			try
			{
				if (!Validation.CheckString(adviseRequest.FullName) || !Validation.CheckXSSInput(adviseRequest.FullName))
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "FullName không hợp lệ!";
					return returnData;
				}
				if (adviseRequest.Phone == null && adviseRequest.Email == null)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "Vui lòng để lại thông tin liên hệ!";
					return returnData;
				}
				if (adviseRequest.Phone != null)
				{
					if (!Validation.CheckNumber(adviseRequest.Phone) || !Validation.CheckXSSInput(adviseRequest.Phone))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "Phone không hợp lệ!";
						return returnData;
					}
				}
				if (adviseRequest.Email != null)
				{
					if (!Validation.CheckString(adviseRequest.Email) || !Validation.CheckXSSInput(adviseRequest.Email))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "Email không hợp lệ!";
						return returnData;
					}
				}
				if (adviseRequest.Content == null || !Validation.CheckString(adviseRequest.Content) 
					|| !Validation.CheckXSSInput(adviseRequest.Content))
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "Content không hợp lệ!";
					return returnData;
				}
				var newAdvise = new Advise
				{
					FullName = adviseRequest.FullName,
					Phone = adviseRequest.Phone,
					Email = adviseRequest.Email,
					Content = adviseRequest.Content,
					CreationDate = DateTime.Now,
					ConsultingStatus = 1
				};
				await _context.Advise.AddAsync(newAdvise);
				await _context.SaveChangesAsync();
				returnData.ResponseCode = 1;
				returnData.ResposeMessage = "Thêm thành công Advise!";
				return returnData;
			}
			catch(Exception ex)
			{
				throw new Exception($"Error in InsertAdvise Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponseData> UpdateAdvise(Update_Advise update_Advise)
		{
			var returnData = new ResponseData(); 
			try
			{
				var advise = await _context.Advise.Where(s => s.AdviseID == update_Advise.AdviseID).FirstOrDefaultAsync();
				if (advise != null)
				{
					advise.ConsultingStatus = 0;
					_context.Advise.Update(advise);
					await _context.SaveChangesAsync();
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = $"Cập nhật thông tin thành công!";
					return returnData;
				}
				returnData.ResponseCode = 0;
				returnData.ResposeMessage = $"Không tìm thấy AdviseID!";
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error in UpdateAdvise Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponseData> DeleteAdvise(Update_Advise delete_Advise)
		{
			var returnData = new ResponseData();
			try
			{
				var advise = await _context.Advise.Where(s => s.AdviseID == delete_Advise.AdviseID).FirstOrDefaultAsync();
				if (advise != null)
				{
					advise.ConsultingStatus = 0;
					_context.Advise.RemoveRange(advise);
					await _context.SaveChangesAsync();
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = $"Xóa Advise thành công!";
					return returnData;
				}
				returnData.ResponseCode = 0;
				returnData.ResposeMessage = $"Không tìm thấy AdviseID!";
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error in DeleteAdvise Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponseAdvise> GetList_SreachAdvise(GetLisr_SearchAdvise getLisr_Search)
		{
			var responseData = new ResponseAdvise();
			try
			{
				if (getLisr_Search.FullName!= null)
				{
					if (!Validation.CheckString(getLisr_Search.FullName) || !Validation.CheckXSSInput(getLisr_Search.FullName))
					{
						responseData.ResponseCode = -1;
						responseData.ResposeMessage = "FullName không hợp lệ!";
						return responseData;
					}
				}
				if (getLisr_Search.StartDate != null && getLisr_Search.EndDate != null)
				{
					if (getLisr_Search.EndDate < getLisr_Search.StartDate)
					{
						responseData.ResponseCode = -1;
						responseData.ResposeMessage = "Ngày kết thúc không được nhỏ hơn ngày bắt đầu!";
						return responseData;
					}
				}
				var parameter = new DynamicParameters();
				parameter.Add("@FullName", getLisr_Search.FullName ?? null);
				parameter.Add("@StartDate", getLisr_Search.StartDate ?? null);
				parameter.Add("@EndDate", getLisr_Search.EndDate ?? null);
				var result = await DbConnection.QueryAsync<Advise>("GetList_SearchAdvise", parameter);
				if (result != null && result.Any())
				{
					responseData.ResponseCode = 1;
					responseData.ResposeMessage = "Lấy danh sách Advise thành công!";
					responseData.Data = result.ToList();
					return responseData;
				}
				responseData.ResponseCode = 0;
				responseData.ResposeMessage = "Không tìm thấy Advise nào.";
				return responseData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error in GetList_SreachAdvise Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}
	}
}
