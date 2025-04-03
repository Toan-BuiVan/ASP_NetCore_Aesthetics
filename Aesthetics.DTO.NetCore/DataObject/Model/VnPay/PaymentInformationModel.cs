using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DTO.NetCore.DataObject.Model.VnPay
{
	public class PaymentInformationModel
	{
		public string OrderType { get; set; }               //1.Loại đơn hàng
		public double Amount { get; set; }                  //2.Số tiền cần thanh toán.
		public string OrderDescription { get; set; }        //3.Mô tả đơn hàng (ví dụ: "Thanh toán dịch vụ chăm sóc da").
		public string UserName { get; set; }                //4.Tên người thực hiện giao dịch.
	}
}
