using Aesthetics.DataAccess.NetCore.CheckConditions.Response;
using Aesthetics.DTO.NetCore.DataObject.Model;
using Aesthetics.DTO.NetCore.ResponseWallets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DTO.NetCore.ResponseAdvise
{
    public class ResponseAdvise : ResponseData
	{
		public List<Advise>? Data { get; set; }
	}
}
