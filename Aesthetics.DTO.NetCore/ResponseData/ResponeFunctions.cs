using Aesthetics.DataAccess.NetCore.CheckConditions.Response;
using Aesthetics.DTO.NetCore.DataObject.LogginModel;
using Aesthetics.DTO.NetCore.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DTO.NetCore.ResponeFunctions
{
    public class ResponeFunctions : ResponseData
    {
		public List<Function_Loggin>? Data { get; set; }
		public List<Function_Loggin>? function_Loggins { get; set; }
	}
}
