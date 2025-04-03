using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DTO.NetCore.RequestData
{
    public class WalletsRequest
    {
		public int UserID { get; set; }
		public int VoucherID { get; set; }
	}

	public class RedeemVouchers
	{
		public int UserID { get; set; }
		public int VoucherID { get; set; }
		public string PointType { get; set; }
	}

	public class Update_Wallest
	{
		public int WalletsID { get; set; }
		public int? UserID { get; set; }
		public int? VoucherID { get; set; }
	}

	public class Delete_Wallest
	{
		public int WalletsID { get; set; }
	}

	public class GetList_SearchWallets
	{
		public int? WalletsID { get; set; }
		public int? UserID { get; set; }
		public int? VoucherID { get; set; }
	}
}
