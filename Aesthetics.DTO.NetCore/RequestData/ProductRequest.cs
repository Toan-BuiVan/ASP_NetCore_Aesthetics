using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DTO.NetCore.RequestData
{
    public class ProductRequest
    {
		public int? EmployeeID { get; set; }
		public int ProductsOfServicesID { get; set; }
		public int SupplierID { get; set; }
		public string? ProductName { get; set; }
		public string? ProductDescription { get; set; }

		public decimal? SellingPrice { get; set; }
		public int? Quantity { get; set; }
		public string? ProductImages { get; set; }
	}
	public class Update_Products
	{
		public int ProductID { get; set; }
		public string? ProductName { get; set; }
		public int? ProductsOfServicesID { get; set; }
		public int? SupplierID { get; set; }
		public string? ProductDescription { get; set; }

		public decimal? SellingPrice { get; set; }
		public int? Quantity { get; set; }
		public string? ProductImages { get; set; }
	}
	public class Delete_Products
	{
		public int ProductID { get; set; }
	}

	public class GetList_SearchProducts
	{
		public int? ProductID { get; set; }
		public string? ProductName { get; set; }
		public string? ProductsOfServicesName { get; set; }
		public string? SupplierName { get; set; }
	}

	public class ExportProductExcel
	{
		public int? ProductID { get; set; }
		public string? ProductName { get; set; }
		public string? ProductsOfServicesName { get; set; }
		public string? SupplierName { get; set; }
		public string filePath { get; set; }
	}

	public class SortListProducts
	{
		public string? SortType { get; set; }
		public int? PageIndex { get; set; }
		public int? PageSize { get; set; }
		public decimal? MinPrice { get; set; }
		public decimal? MaxPrice { get; set; }
	}
}
