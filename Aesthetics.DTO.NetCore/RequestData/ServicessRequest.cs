using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DTO.NetCore.RequestData
{
	public class ServicessRequest
	{
		public int ProductsOfServicesID { get; set; }
		public string ServiceName { get; set; }
		public string Description { get; set; }
		public string ServiceImage { get; set; }
		public decimal PriceService { get; set; }
	}

	public class Update_Servicess
	{
		public int ServiceID { get; set; }
		public int? ProductsOfServicesID { get; set; }
		public string? ServiceName { get; set; }
		public string? Description { get; set; }
		public string? ServiceImage { get; set; }

		public decimal? PriceService { get; set; }
	}
	public class Delete_Servicess
	{
		public int ServiceID { get; set; }
	}

	public class GetList_SearchServicess
	{
		public int? ServiceID { get; set; }
		public string? ServiceName { get; set; }
		public int? ProductsOfServicesID { get; set; }
	}

	public class ExportSevicessExcel
	{
		public int? ServiceID { get; set; }
		public string? ServiceName { get; set; }
		public int? ProductsOfServicesID { get; set; }
		public string filePath { get; set; }
	}

	public class SortListSevicess
	{
		public string? SortType { get; set; }
		public int? PageIndex { get; set; }
		public int? PageSize { get; set; }
		public decimal? MinPrice { get; set; }
		public decimal? MaxPrice { get; set; }
	}
}
