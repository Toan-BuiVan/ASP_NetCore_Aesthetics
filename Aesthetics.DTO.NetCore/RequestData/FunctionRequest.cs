using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DTO.NetCore.RequestData
{
    public class FunctionRequest
    {
		public string FunctionCode { get; set; }
		public string FunctionName { get; set; }
		public string FunctionDescription { get; set; }
	}

	public class Update_Function
	{
		public int FunctionID { get; set; }
		public string? FunctionCode { get; set; }
		public string? FunctionName { get; set; }
		public string? FunctionDescription { get; set; }
	}

	public class Delete_Function
	{
		public int FunctionID { get; set; }
	}

	public class GetList_SearchFunction
	{
		public int? FunctionID { get; set; }
		public string? FunctionName { get; set; }
	}
}
