using Aesthetics.DataAccess.NetCore.DBContext;
using Aesthetics.DataAccess.NetCore.Repositories.Interface;
using Aesthetics.DataAccess.NetCore.Repositories.Interfaces;
using Aesthetics.DTO.NetCore.DataObject.LogginModel;
using Aesthetics.DTO.NetCore.DataObject.Model;
using Aesthetics.DTO.NetCore.RequestData;
using Aesthetics.DTO.NetCore.ResponseWallets;
using BE_102024.DataAces.NetCore.Dapper;
using Dapper;
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
	public class WalletsRepository : BaseApplicationService, IWalletsRepository
	{
		private DB_Context _context;
		private IConfiguration _configuration;
		private IUserRepository _userRepository;
		private IVouchersRepository _vouchersRepository;
		public WalletsRepository(DB_Context context, IConfiguration configuration,
			IServiceProvider serviceProvider, IUserRepository userRepository,
			IVouchersRepository vouchersRepository) : base(serviceProvider) 
		{
			_context = context;
			_configuration = configuration;
			_userRepository = userRepository;
			_vouchersRepository = vouchersRepository;
		}

		public async Task<ResponseWallets> Insert_Wallets(WalletsRequest insert_)
		{
			var rerturnData = new ResponseWallets();
			var wallets_Loggins = new List<Wallets_Loggin>();
			try
			{
				var findUser = await _userRepository.GetUserByUserID(insert_.UserID);
				if (insert_.UserID <= 0 || findUser == null)
				{
					rerturnData.ResponseCode = -1;
					rerturnData.ResposeMessage = $"UserID: {insert_.UserID} không hợp lệ || không tồn tại!";
					return rerturnData;
				}
				var findVouchers = await _vouchersRepository.GetVouchersByVouchersID(insert_.VoucherID);
				if (insert_.VoucherID <= 0 || findVouchers == null)
				{
					rerturnData.ResponseCode = -1;
					rerturnData.ResposeMessage = $"VoucherID: {insert_.VoucherID} không hợp lệ || không tồn tại!";
					return rerturnData;
				}
				// Kiểm tra quyền sử dụng voucher dựa trên RankMember
				bool isValidRank = false;
				switch (findUser.RankMember.Trim())
				{
					case "Diamond":
						isValidRank = findVouchers.RankMember.Trim() is "Diamond" or "Gold" or "Silver" or "Bronze" or "Default";
						break;
					case "Gold":
						isValidRank = findVouchers.RankMember.Trim() is "Gold" or "Silver" or "Bronze" or "Default";
						break;
					case "Silver":
						isValidRank = findVouchers.RankMember.Trim() is "Silver" or "Bronze" or "Default";
						break;
					case "Bronze":
						isValidRank = findVouchers.RankMember.Trim() is "Bronze" or "Default";
						break;
					default:
						isValidRank = false;
						break;
				}
				if (!isValidRank)
				{
					rerturnData.ResponseCode = -1;
					rerturnData.ResposeMessage = $"Bạn không có quyền dùng VoucherID: {findVouchers.VoucherID}!";
					return rerturnData;
				}
				var newWallets = new Wallets
				{
					UserID = insert_.UserID,
					VoucherID = insert_.VoucherID,
				};
				_context.Wallets.Add(newWallets);
				await _context.SaveChangesAsync();
				wallets_Loggins.Add(new Wallets_Loggin
				{
					WalletsID = newWallets.WalletsID,
					UserID = newWallets.UserID,
					VoucherID= newWallets.VoucherID,
				});
				rerturnData.ResponseCode = 1;
				rerturnData.ResposeMessage = "Insert Wallets thành công!";
				rerturnData.wallets_Loggins = wallets_Loggins;
				return rerturnData;
			}
			catch (Exception ex) 
			{
				throw new Exception($"Error in Insert_Wallets Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponseWallets> Update_Wallets(Update_Wallest update_)
		{
			var rerturnData = new ResponseWallets();
			var wallets_Loggins = new List<Wallets_Loggin>();
			try
			{
				var resultWallets = await GetWalletsByWalletId(update_.WalletsID);
				if (update_.WalletsID <= 0 || resultWallets == null)
				{
					rerturnData.ResponseCode = -1;
					rerturnData.ResposeMessage = $"WalletsID: {update_.WalletsID} không hợp lệ || không tồn tại!";
					return rerturnData;
				}
				resultWallets.WalletsID = update_.WalletsID;
				if (update_.UserID == null && update_.VoucherID == null)
				{
					rerturnData.ResponseCode = -1;
					rerturnData.ResposeMessage = "Vui lòng chọn mục ít nhất 1 mục Update";
					return rerturnData;
				}
				Users findUser = null;
				Vouchers findVouchers = null;
				if (update_.UserID != null)
				{
					findUser = await _userRepository.GetUserByUserID(update_.UserID);
				}
				if (update_.VoucherID != null) 
				{
					findVouchers = await _vouchersRepository.GetVouchersByVouchersID(update_.VoucherID ?? 0);
				}
				
				// Kiểm tra quyền sử dụng voucher dựa trên RankMember
				bool isValidRank = false;
				if (update_.UserID != null && update_.VoucherID!= null)
				{
					switch (findUser?.RankMember.Trim())
					{
						case "Diamond":
							isValidRank = findVouchers.RankMember.Trim() is "Diamond" or "Gold" or "Silver" or "Bronze" or "Default";
							break;
						case "Gold":
							isValidRank = findVouchers.RankMember.Trim() is "Gold" or "Silver" or "Bronze" or "Default";
							break;
						case "Silver":
							isValidRank = findVouchers.RankMember.Trim() is "Silver" or "Bronze" or "Default";
							break;
						case "Bronze":
							isValidRank = findVouchers.RankMember.Trim() is "Bronze" or "Default";
							break;
						default:
							isValidRank = false;
							break;
					}
					if (!isValidRank)
					{
						rerturnData.ResponseCode = -1;
						rerturnData.ResposeMessage = $"Bạn không có quyền dùng VoucherID: {findVouchers.VoucherID}!";
						return rerturnData;
					}
					resultWallets.UserID = update_.UserID ?? 0;
					resultWallets.VoucherID = update_.VoucherID ?? 0;
				} 
				if (update_.UserID != null && update_.VoucherID == null)
				{
					var vouchersID = resultWallets.VoucherID;
					var resultRankMemberVoucher = await _context.Vouchers
						.Where(s => s.VoucherID == vouchersID)
						.Select(s => s.RankMember)
						.FirstOrDefaultAsync();

					switch (findUser.RankMember.Trim())
					{
						case "Diamond":
							isValidRank = resultRankMemberVoucher.Trim() is "Diamond" or "Gold" or "Silver" or "Bronze" or "Default";
							break;
						case "Gold":
							isValidRank = resultRankMemberVoucher.Trim() is "Gold" or "Silver" or "Bronze" or "Default";
							break;
						case "Silver":
							isValidRank = resultRankMemberVoucher.Trim() is "Silver" or "Bronze" or "Default";
							break;
						case "Bronze":
							isValidRank = resultRankMemberVoucher.Trim() is "Bronze" or "Default";
							break;
						default:
							isValidRank = false;
							break;
					}
					if (!isValidRank)
					{
						rerturnData.ResponseCode = -1;
						rerturnData.ResposeMessage = $"Bạn không có quyền dùng VoucherID: {vouchersID}!";
						return rerturnData;
					}
					resultWallets.UserID = update_.UserID ?? 0;
				}

				if (update_.UserID == null && update_.VoucherID != null) 
				{
					var userID = resultWallets.UserID;
					var resultRankMemberUser = await _context.Users
						.Where(s => s.UserID == userID && s.DeleteStatus == 1)
						.Select(s => s.RankMember)
						.FirstOrDefaultAsync();
					switch(findVouchers.RankMember.Trim())
					{
						case "Diamond":
							isValidRank = resultRankMemberUser.Trim() is "Diamond";
							break;
						case "Gold":
							isValidRank = resultRankMemberUser.Trim() is "Gold" or "Diamond";
							break;
						case "Silver":
							isValidRank = resultRankMemberUser.Trim() is "Silver" or "Gold" or "Diamond";
							break;
						case "Bronze":
							isValidRank = resultRankMemberUser.Trim() is "Bronze" or "Silver" or "Gold" or "Diamond";
							break;
						case "Default":
							isValidRank = resultRankMemberUser.Trim() is "Bronze" or "Silver" or "Gold" or "Diamond";
							break;
						default:
							isValidRank = false;
							break;
					}
					if (!isValidRank)
					{
						rerturnData.ResponseCode = -1;
						rerturnData.ResposeMessage = $"VoucherID: {findVouchers.VoucherID} không thể sử dụng!";
						return rerturnData;
					}
					resultWallets.VoucherID = update_.VoucherID ?? 0; 
				}
				_context.Wallets.Update(resultWallets);
				await _context.SaveChangesAsync();
				wallets_Loggins.Add(new Wallets_Loggin
				{
					WalletsID = resultWallets.WalletsID,
					UserID = resultWallets.UserID,
					VoucherID = resultWallets.VoucherID,
				});
				rerturnData.ResponseCode = 1;
				rerturnData.ResposeMessage = "Update Wallets thành công!";
				rerturnData.wallets_Loggins = wallets_Loggins;
				return rerturnData;
			}
			catch (Exception ex) 
			{
				throw new Exception($"Error in Update_Wallets Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponseWallets> Delete_Wallets(Delete_Wallest delete_)
		{
			var rerturnData = new ResponseWallets();
			var wallets_Loggins = new List<Wallets_Loggin>();
			try
			{
				var findWallets = await GetWalletsByWalletId(delete_.WalletsID);
				if (findWallets != null)
				{
					_context.Wallets.Remove(findWallets);
					await _context.SaveChangesAsync();
					wallets_Loggins.Add(new Wallets_Loggin
					{
						WalletsID = findWallets.WalletsID,
						UserID = findWallets.UserID,
						VoucherID = findWallets.VoucherID,
					});
					rerturnData.ResponseCode = 1;
					rerturnData.ResposeMessage = "Delete Wallets thành công!";
					rerturnData.wallets_Loggins = wallets_Loggins;
					return rerturnData;
				}
				rerturnData.ResponseCode = -1;
				rerturnData.ResposeMessage = "WalletsID không tồn tại!";
				return rerturnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error in Delete_Wallets Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponseWallets> GetList_SearchWallets(GetList_SearchWallets getList_)
		{
			var rerturnData = new ResponseWallets();
			var listWallets = new List<WalletsData>();
			var wallets_Loggins = new List<Wallets_Loggin>();
			try
			{
				if (getList_.WalletsID != null)
				{
					if (getList_.WalletsID <= 0 || await GetWalletsByWalletId(getList_.WalletsID ?? 0) == null)
					{
						rerturnData.ResponseCode = -1;
						rerturnData.ResposeMessage = "WalletsID không hợp lệ || không tồn tại!";
						return rerturnData;
					}
				}
				if (getList_.VoucherID != null)
				{
					if (getList_.VoucherID <= 0 ||await _vouchersRepository.GetVouchersByVouchersID(getList_.VoucherID ?? 0) == null)
					{
						rerturnData.ResponseCode = -1;
						rerturnData.ResposeMessage = $"VoucherID: {getList_.VoucherID} không hợp lệ || không tồn tại!";
						return rerturnData;
					}
				}
				if (getList_.UserID != null)
				{
					if (getList_.UserID <= 0 ||await _userRepository.GetUserByUserID(getList_.UserID) == null)
					{
						rerturnData.ResponseCode = -1;
						rerturnData.ResposeMessage = $"UserID: {getList_.UserID} không hợp lệ || không tồn tại!";
						return rerturnData;
					}
				}
				var parameters = new DynamicParameters();
				parameters.Add("@WalletsID", getList_.WalletsID ?? null);
				parameters.Add("@UserID", getList_.UserID ?? null);
				parameters.Add("@VoucherID", getList_.VoucherID ?? null);
				var result = await DbConnection.QueryAsync<WalletsData>("GetList_SearchWallets", parameters);
				if (result != null && result.Any()) 
				{
					foreach (var item in result)
					{
						wallets_Loggins.Add(new Wallets_Loggin
						{
							WalletsID = item.WalletsID,
							UserID = item.UserID,
							VoucherID = item.VoucherID,
						});
					}
					rerturnData.ResponseCode = 1;
					rerturnData.ResposeMessage = "Lấy thành công danh sách Wallets!";
					rerturnData.Data = result.ToList();
					rerturnData.wallets_Loggins = wallets_Loggins;
					return rerturnData;
				}
				rerturnData.ResponseCode = 0;
				rerturnData.ResposeMessage = "Lấy thất bại danh sách Wallets!";
				return rerturnData;
			}
			catch (Exception ex) 
			{
				throw new Exception($"Error in GetList_SearchWallets Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<Wallets> GetWalletsByWalletId(int walletId)
		{
			return await _context.Wallets.Where(s => s.WalletsID == walletId).FirstOrDefaultAsync();
		}

		public async Task<ResponseWallets> RedeemPointsForVoucher(RedeemVouchers _redeem)
		{
			var returnData = new ResponseWallets();
			var wallets_Loggins = new List<Wallets_Loggin>();
			try
			{
				// 1. Kiểm tra User
				var user = await _userRepository.GetUserByUserID(_redeem.UserID);
				if (user == null)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = $"UserID: {_redeem.UserID} không tồn tại!";
					return returnData;
				}

				// 2. Kiểm tra Voucher
				var voucher = await _vouchersRepository.GetVouchersByVouchersID(_redeem.VoucherID);
				if (voucher == null)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = $"VoucherID: {_redeem.VoucherID} không tồn tại!";
					return returnData;
				}

				// 3. Kiểm tra loại điểm hợp lệ
				if (string.IsNullOrEmpty(_redeem.PointType) || (_redeem.PointType != "Accumulated" && _redeem.PointType != "Rating"))
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "Vui lòng chọn loại điểm hợp lệ: 'Accumulated' hoặc 'Rating'!";
					return returnData;
				}

				// 4. Xử lý đổi điểm theo loại
				if (_redeem.PointType == "Accumulated")
				{
					if (user.AccumulatedPoints < voucher.AccumulatedPoints)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "Điểm tích lũy của bạn không đủ!";
						return returnData;
					}

					user.AccumulatedPoints -= voucher.AccumulatedPoints;
				}
				else if (_redeem.PointType == "Rating")
				{
					if (user.RatingPoints < voucher.RatingPoints)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "Điểm mua hàng của bạn không đủ!";
						return returnData;
					}

					user.RatingPoints -= voucher.RatingPoints;
				}

				// 5. Tạo ví voucher
				var newWallets = new Wallets
				{
					UserID = _redeem.UserID,
					VoucherID = _redeem.VoucherID,
				};

				wallets_Loggins.Add(new Wallets_Loggin
				{
					WalletsID = newWallets.WalletsID,
					UserID = newWallets.UserID,
					VoucherID = newWallets.VoucherID,
				});
				_context.Wallets.Add(newWallets);
				await _context.SaveChangesAsync();

				_context.Users.Update(user);
				await _context.SaveChangesAsync();

				returnData.ResponseCode = 1;
				returnData.ResposeMessage = "Đổi điểm thành công!";
				returnData.wallets_Loggins = wallets_Loggins;
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error in RedeemPointsForVoucher: {ex.Message}", ex);
			}
		}
	}
}
