using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DTO.NetCore.DataObject.LogginModel
{
    public class Invoice_Loggin_Input
    {
		public int InvoiceID { get; set; }
		public int? EmployeeID { get; set; }
		public DateTime? DateCreated { get; set; }
		public int? DeleteStatus { get; set; }
		public decimal? TotalMoney { get; set; }
		public string? Type { get; set; }
	}

	public class Invoice_Loggin_Ouput
	{
		public int InvoiceID { get; set; }
		public int? CustomerID { get; set; }
		public int? EmployeeID { get; set; }
		public int? VoucherID { get; set; }
		public string? Code { get; set; }
		public double? DiscountValue { get; set; }
		public decimal? TotalMoney { get; set; }
		public DateTime? DateCreated { get; set; }
		public string? Status { get; set; }
		public int? DeleteStatus { get; set; }
		public string? Type { get; set; }
	}

	public class GetList_Invoice_Out
	{
		public int InvoiceID { get; set; }
		public int? CustomerID { get; set; }
		public string? CustomerName { get; set; }
		public string? Code { get; set; }
		public double? DiscountValue { get; set; }
		public decimal? TotalMoney { get; set; }
		public decimal? TotalAmountAfterDiscount { get; set; }
		public string? Status { get; set; }
		public DateTime? DateCreated { get; set; }
	}

	public class GetList_InvoiceDetail_Out
	{
		public string? CustomerName { get; set; }
		public string? ProductName { get; set; }
		public decimal? PriceProduct { get; set; }
		public int? TotalQuantityProduct { get; set; }
		public string? ServiceName { get; set; }
		public decimal? PriceService { get; set; }
		public int? TotalQuantityService { get; set; }
		public string? Code { get; set; }
		public double? DiscountValue { get; set; }
		public decimal? TotalMoney { get; set; }
		public string? Status { get; set; }
	}
}
