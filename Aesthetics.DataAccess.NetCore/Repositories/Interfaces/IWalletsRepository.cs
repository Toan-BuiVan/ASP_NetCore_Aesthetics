using Aesthetics.DTO.NetCore.DataObject.Model;
using Aesthetics.DTO.NetCore.RequestData;
using Aesthetics.DTO.NetCore.ResponseWallets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DataAccess.NetCore.Repositories.Interfaces
{
    public interface IWalletsRepository
    {
        //1. Insert wallets 
        Task<ResponseWallets> Insert_Wallets(WalletsRequest insert_);

		//2. Update wallets 
		Task<ResponseWallets> Update_Wallets(Update_Wallest update_);

		//3. Delete wallets 
		Task<ResponseWallets> Delete_Wallets(Delete_Wallest delete_);

		//4. Get list & Search wallets 
		Task<ResponseWallets> GetList_SearchWallets(GetList_SearchWallets getList_);

		//5. Get wallets by walletsID 
		Task<Wallets> GetWalletsByWalletId(int walletId);

		//6. Đổi điểm thưởng mua hàng dịch vụ - điểm nhập mã giới thiệu lấy voucher
		Task<ResponseWallets> RedeemPointsForVoucher(RedeemVouchers _redeem);

	}
}
