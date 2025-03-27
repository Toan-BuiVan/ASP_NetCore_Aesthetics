using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DTO.NetCore.DataObject.LogginModel
{
    public class Function_Loggin
    {
		public int FunctionID { get; set; }
		public string? FunctionCode { get; set; }
		public string? FunctionName { get; set; }
		public string? FunctionDescription { get; set; }
		public int? DeleteStatus { get; set; }
	}
}
