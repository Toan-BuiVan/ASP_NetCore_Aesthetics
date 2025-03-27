using Aesthetics.DataAccess.NetCore.CheckConditions.Response;
using Aesthetics.DTO.NetCore.DataObject.LogginModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DTO.NetCore.ResponseWallets
{
	public class WalletsData
	{
		public int WalletsID { get; set; }
		public int UserID { get; set; }
		public int VoucherID { get; set; }
		public string? Code { get; set; }
		public string? Description { get; set; }
		public string? VoucherImage { get; set; }
		public double? DiscountValue { get; set; }
		public double? MinimumOrderValue { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
	}
	public class ResponseWallets : ResponseData
    {
		public List<WalletsData>? Data { get; set; }
		public List<Wallets_Loggin>? wallets_Loggins { get; set; }
	}
}
