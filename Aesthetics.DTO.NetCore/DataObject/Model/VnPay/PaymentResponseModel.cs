using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DTO.NetCore.DataObject.Model.VnPay
{
	public class PaymentResponseModel
	{
		public string OrderDescription { get; set; }            //1.Mô tả đơn hàng (trùng với mô tả ban đầu)
		public string TransactionId { get; set; }               //2.Mã giao dịch do cổng thanh toán cung cấp.
		public string OrderId { get; set; }                     //3.Mã đơn hàng duy nhất của hệ thống.
		public string PaymentMethod { get; set; }               //4.Phương thức thanh toán (Ví dụ: "VnPay", "MoMo").
		public string PaymentId { get; set; }                   //5.Mã định danh thanh toán (nếu có).
		public bool Success { get; set; }                       //6.Trạng thái giao dịch (true nếu thành công).
		public string Token { get; set; }                       //7.Mã xác thực có thể dùng để tra cứu giao dịch || xác nhận thanh toán
		public string VnPayResponseCode { get; set; }           //7.Mã phản hồi từ hệ thống VnPay
	}
}
