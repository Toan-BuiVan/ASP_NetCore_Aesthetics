using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DTO.NetCore.DataObject.LogginModel
{
    public class InvoiceDetail_Loggin_Input
    {
		public int InvoiceDetailID { get; set; }
		public int InvoiceID { get; set; }
		public int? EmployeeID { get; set; }
		public string? EmployeeName { get; set; }
		public int? ProductID { get; set; }
		public string? ProductName { get; set; }
		public decimal? PriceProduct { get; set; }
		public int? TotalQuantityProduct { get; set; }
		public decimal? TotalMoney { get; set; }
		public int? DeleteStatus { get; set; }
		public string? Type { get; set; }
	}

	public class InvoiceDetail_Loggin_Ouput
	{
		public int InvoiceDetailID { get; set; }
		public int InvoiceID { get; set; }
		public int? CustomerID { get; set; }
		public string? CustomerName { get; set; }
		public int? EmployeeID { get; set; }
		public string? EmployeeName { get; set; }
		public int? ProductID { get; set; }
		public string? ProductName { get; set; }
		public int? ServiceID { get; set; }
		public string? ServiceName { get; set; }
		public int VoucherID { get; set; }
		public string? Code { get; set; }
		public double? DiscountValue { get; set; }
		public decimal? PriceService { get; set; }
		public decimal? PriceProduct { get; set; }
		public int? TotalQuantityService { get; set; }
		public int? TotalQuantityProduct { get; set; }
		public decimal? TotalMoney { get; set; }
		public string? Status { get; set; }
		public int? DeleteStatus { get; set; }
		public string? Type { get; set; }
	}
}
