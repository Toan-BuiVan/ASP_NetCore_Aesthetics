using Aesthetics.DataAccess.NetCore.DBContext;
using Aesthetics.DataAccess.NetCore.Repositories.Interfaces;
using Aesthetics.DTO.NetCore.DataObject.LogginModel;
using Aesthetics.DTO.NetCore.DataObject.Model;
using Aesthetics.DTO.NetCore.RequestData;
using Aesthetics.DTO.NetCore.ResponseVouchers;
using BE_102024.DataAces.NetCore.CheckConditions;
using BE_102024.DataAces.NetCore.Dapper;
using Dapper;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Aesthetics.DataAccess.NetCore.Repositories.Implement
{
    public class VouchersRepository : BaseApplicationService, IVouchersRepository
	{
        private DB_Context _context;
        private IConfiguration _configuration;
		public VouchersRepository(DB_Context context, IConfiguration configuration,
			IServiceProvider serviceProvider) : base(serviceProvider) 
		{
			 _context = context;
			_configuration = configuration;
		}

		public async Task<ResponseVouchers_Loggin> Insert_Vouchers(VouchersRequest insert_)
		{
			var returnData = new ResponseVouchers_Loggin();
			var vouchers_Loggins = new List<Vouchers_Loggin>();
			try
			{
				if (!Validation.CheckString(insert_.Description) || !Validation.CheckXSSInput(insert_.Description)) 
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "Description không hợp lệ!";
					return returnData;
				}
				if (insert_.DiscountValue <= 0 || insert_.DiscountValue > 100)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "DiscountValue không hợp lệ! Phải nằm trong khoảng 1 - 100.";
					return returnData;
				}
				if (insert_.StartDate < DateTime.Today || insert_.EndDate < DateTime.Today || insert_.StartDate > insert_.EndDate)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "StartDate hoặc EndDate không hợp lệ!";
					return returnData;
				}
				if (insert_.StartDate > insert_.EndDate)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "Hạn sử dụng Vouchers không hợp lệ!";
					return returnData;
				}
				if (insert_.MinimumOrderValue <= 0)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "MinimumOrderValue không hợp lệ!";
					return returnData;
				}
				if (insert_.MaxValue <= 0)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "MinimumOrderValue không hợp lệ!";
					return returnData;
				}
				if (insert_.AccumulatedPoints < 0)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "AccumulatedPoints không hợp lệ!";
					return returnData;
				}
				if (insert_.RatingPoints < 0)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "RatingPoints không hợp lệ!";
					return returnData;
				}
				if (!Validation.CheckString(insert_.RankMember) || !Validation.CheckXSSInput(insert_.RankMember))
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "RankMember không hợp lệ!";
					return returnData;
				}
				if (!Validation.CheckString(insert_.VoucherImage))
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "VoucherImage không hợp lệ!";
					return returnData;
				}
				var vouchersImagePath = await BaseProcessingFunction64(insert_.VoucherImage);
				var code = await GenCodeUnique();
				var newVouchers = new Vouchers
				{
					Code = code,
					Description = insert_.Description,
					VoucherImage = vouchersImagePath,
					DiscountValue = insert_.DiscountValue,
					StartDate = insert_.StartDate,
					EndDate = insert_.EndDate,
					MinimumOrderValue = insert_.MinimumOrderValue,
					MaxValue = insert_.MaxValue,
					RankMember = insert_.RankMember,
					RatingPoints = insert_.RatingPoints,
					AccumulatedPoints = insert_.AccumulatedPoints,
					IsActive = 1
				};
				_context.Vouchers.Add(newVouchers);
				await _context.SaveChangesAsync();
				vouchers_Loggins.Add(new Vouchers_Loggin
				{
					VoucherID = newVouchers.VoucherID,
					Code = newVouchers.Code,
					Description = newVouchers.Description,
					VoucherImage = newVouchers.VoucherImage,
					DiscountValue= newVouchers.DiscountValue,
					StartDate = newVouchers.StartDate,
					EndDate = newVouchers.EndDate,
					MinimumOrderValue = newVouchers.MinimumOrderValue,
					MaxValue = newVouchers.MaxValue,
					RankMember = newVouchers.RankMember,
					RatingPoints = newVouchers.RatingPoints,
					AccumulatedPoints = newVouchers.AccumulatedPoints,
					IsActive = newVouchers.IsActive,
				});
				returnData.ResponseCode = 1;
				returnData.ResposeMessage = "Insert Vouchers thành công!";
				returnData.vouchers_Loggins = vouchers_Loggins;
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error in Insert_Vouchers Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponseVouchers_Loggin> Update_Vouchers(Update_Vouchers update_)
		{
			var returnData = new ResponseVouchers_Loggin();
			var vouchers_Loggins = new List<Vouchers_Loggin>();
			try
			{
				var resultVouchers = await GetVouchersByVouchersID(update_.VoucherID);
				if (update_.VoucherID <= 0 || resultVouchers == null)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "VouchersID không hợp lệ || không tồn tại!";
					return returnData;
				}
				if (update_.Description != null)
				{
					if (!Validation.CheckString(update_.Description) || !Validation.CheckXSSInput(update_.Description))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "Description không hợp lệ!";
						return returnData;
					}
					resultVouchers.Description = update_.Description;
				}
				if (update_.VoucherImage != null) 
				{
					var vouchersImagePath = await BaseProcessingFunction64(update_.VoucherImage);
					resultVouchers.VoucherImage = vouchersImagePath;
				}
				if (update_.DiscountValue != null)
				{
					if (update_.DiscountValue <= 0 || update_.DiscountValue > 100)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "DiscountValue không hợp lệ! Phải nằm trong khoảng 1 - 100.";
						return returnData;
					}
					resultVouchers.DiscountValue = update_.DiscountValue;
				}
				if (update_.StartDate != null)
				{
					if (update_.StartDate < DateTime.Today || update_.StartDate > resultVouchers.EndDate)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "StartDate không hợp lệ!";
						return returnData;
					}
					resultVouchers.StartDate = update_.StartDate;
				}
				if (update_.EndDate != null)
				{
					if (update_.EndDate < DateTime.Today || update_.EndDate < resultVouchers.StartDate)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "EndDate không hợp lệ!";
						return returnData;
					}
					resultVouchers.EndDate = update_.EndDate;
				}
				if (update_.MinimumOrderValue != null)
				{
					if (update_.MinimumOrderValue <= 0)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "MinimumOrderValue không hợp lệ!";
						return returnData;
					}
					resultVouchers.MinimumOrderValue = update_.MinimumOrderValue;
				}

				if (update_.MaxValue != null)
				{
					if (update_.MaxValue <= 0)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "MaxValue không hợp lệ!";
						return returnData;
					}
					resultVouchers.MaxValue = update_.MaxValue;
				}
				if (update_.RankMember != null)
				{
					if (!Validation.CheckString(update_.RankMember) || !Validation.CheckXSSInput(update_.RankMember))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "RankMember không hợp lệ!";
						return returnData;
					}
					resultVouchers.RankMember = update_.RankMember;
				}
				if (update_.AccumulatedPoints != null)
				{
					if (update_.AccumulatedPoints < 0)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "AccumulatedPoints không hợp lệ!";
						return returnData;
					}
					resultVouchers.AccumulatedPoints = update_.AccumulatedPoints;
				}
				if(update_.RatingPoints != null)
				{
					if (update_.RatingPoints < 0)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "RatingPoints không hợp lệ!";
						return returnData;
					}
					resultVouchers.RatingPoints = update_.RatingPoints;
				}
				_context.Vouchers.Update(resultVouchers);
				await _context.SaveChangesAsync();
				vouchers_Loggins.Add(new Vouchers_Loggin
				{
					VoucherID = resultVouchers.VoucherID,
					Code = resultVouchers.Code,
					Description = resultVouchers.Description,
					VoucherImage = resultVouchers.VoucherImage,
					DiscountValue = resultVouchers.DiscountValue,
					StartDate = resultVouchers.StartDate,
					EndDate = resultVouchers.EndDate,
					MinimumOrderValue = resultVouchers.MinimumOrderValue,
					RankMember = resultVouchers.RankMember,
					AccumulatedPoints = resultVouchers.AccumulatedPoints,
					RatingPoints = resultVouchers.RatingPoints,
					IsActive = resultVouchers.IsActive,
				});
				returnData.ResponseCode = 1;
				returnData.ResposeMessage = "Update Vouchers thành công!";
				returnData.vouchers_Loggins = vouchers_Loggins;
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error in Update_Vouchers Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponeDeleteVouchers_Loggin> Delete_Vouchers(Delete_Vouchers delete_)
		{
			var returnData = new ResponeDeleteVouchers_Loggin();
			var vouchers_Loggins = new List<Vouchers_Loggin>();
			var wallets_Loggins = new List<Wallets_Loggin>();
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				var findVouchers = await _context.Vouchers
						.Include(s => s.Wallets)
						.AsSplitQuery()
						.FirstOrDefaultAsync(s => s.VoucherID == delete_.VoucherID);
				if (findVouchers != null)
				{
					//1. Xóa voucher
					findVouchers.IsActive = 0;
					vouchers_Loggins.Add(new Vouchers_Loggin
					{
						VoucherID = findVouchers.VoucherID,
						Code = findVouchers.Code,
						Description = findVouchers.Description,
						VoucherImage = findVouchers.VoucherImage,
						DiscountValue = findVouchers.DiscountValue,
						StartDate = findVouchers.StartDate,
						EndDate = findVouchers.EndDate,
						MinimumOrderValue = findVouchers.MinimumOrderValue,
						MaxValue = findVouchers.MaxValue,
						RankMember = findVouchers.RankMember,
						IsActive = findVouchers.IsActive,
					});

					//2. Xóa các bản ghi ở wallets liên quan đến vouchersID
					var resultWallest = findVouchers.Wallets
							.Where(s => s.VoucherID == findVouchers.VoucherID).ToList();
					if (resultWallest != null && resultWallest.Any()) 
					{
						foreach (var item in resultWallest)
						{
							_context.Wallets.Remove(item);
							await _context.SaveChangesAsync();
							wallets_Loggins.Add(new Wallets_Loggin
							{
								WalletsID = item.WalletsID,
								VoucherID = item.VoucherID,
								UserID = item.UserID,
							});
						}
					}

					//Commit transaction nếu thành công
					await _context.SaveChangesAsync();
					await transaction.CommitAsync();
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = "Delete thành công Vouchers!";
					returnData.vouchers_Loggins = vouchers_Loggins;
					returnData.wallets_Loggins = wallets_Loggins;
					return returnData;
				}
				returnData.ResponseCode = -1;
				returnData.ResposeMessage = $"{delete_.VoucherID} không tồn tại!";
				return returnData;
			}
			catch (Exception ex) 
			{
				throw new Exception($"Error in Delete_Vouchers Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponseVouchers_Loggin> GetList_SearchVouchers(GetList_SearchVouchers getList_)
		{
			var returnData = new ResponseVouchers_Loggin();
			try
			{
				if (getList_.VoucherID != null) 
				{
					var findVouchers = await GetVouchersByVouchersID(getList_.VoucherID ?? 0);
					if (getList_.VoucherID <= 0 || findVouchers == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = $"{getList_.VoucherID} không hợp lệ || không tồn tại!";
						return returnData;
					}
				}
				if (getList_.StartDate != null && getList_.EndDate != null)
				{
					if (getList_.StartDate > getList_.EndDate)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "Ngày tìm kiếm không hợp lệ!";
						return returnData;
					}
				}
				if (getList_.RankMember != null)
				{
					if (!Validation.CheckString(getList_.RankMember) || !Validation.CheckXSSInput(getList_.RankMember))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "RankMember không hợp lệ!";
						return returnData;
					}
				}
				var parameters = new DynamicParameters();
				parameters.Add("@VoucherID", getList_.VoucherID ?? null);
				parameters.Add("@StartDate", getList_.StartDate ?? null);
				parameters.Add("@EndDate", getList_.EndDate ?? null);
				parameters.Add("@RankMember", getList_.RankMember ?? null);
				var result = await DbConnection.QueryAsync<Vouchers_Loggin>("GetList_SearchVouchers", parameters);
				if (result != null & result.Any())
				{
					returnData.ResponseCode = 1;
					returnData.Data = result.ToList();
					returnData.ResposeMessage = "Lấy thành công danh sách Vouchers!";
					return returnData;
				}
				returnData.ResponseCode = 0;
				returnData.ResposeMessage = "Danh sách Vouchers trống";
				return returnData;
			}
			catch (Exception ex) 
			{
				throw new Exception($"Error in GetList_SearchVouchers Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<string> BaseProcessingFunction64(string? ServicessImage)
		{
			try
			{
				var path = "FilesImages/Vouchers";

				if (!System.IO.Directory.Exists(path))
				{
					System.IO.Directory.CreateDirectory(path);
				}
				string imageName = Guid.NewGuid().ToString() + ".png";
				var imgPath = Path.Combine(path, imageName);

				if (ServicessImage.Contains("data:image"))
				{
					ServicessImage = ServicessImage.Substring(ServicessImage.LastIndexOf(',') + 1);
				}

				byte[] imageBytes = Convert.FromBase64String(ServicessImage);
				MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
				ms.Write(imageBytes, 0, imageBytes.Length);
				System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);

				image.Save(imgPath, System.Drawing.Imaging.ImageFormat.Png);

				return imageName;
			}
			catch (Exception ex)
			{
				throw new Exception("Lỗi khi lưu file: " + ex.Message);
			}
		}

		public async Task<string> GenCodeUnique()
		{
			string code;
			bool exists;
			Random random = new Random();
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

			do
			{
				code = new string(chars.OrderBy(x => random.Next()).Take(5).ToArray());
				exists = await _context.Vouchers.AnyAsync(s => s.Code == code);
			} while (exists);
			return code;
		}

		public async Task<Vouchers> GetVouchersByVouchersID(int vouchersID)
		{
			return await _context.Vouchers.Where(s => s.VoucherID == vouchersID
							&& s.IsActive == 1).FirstOrDefaultAsync();
		}
	}
}
