using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DTO.NetCore.DataObject.Model.MoMo
{
	public class MomoOptionModel
	{
		public string MomoApiUrl { get; set; }
		public string SecretKey { get; set; }
		public string AccessKey { get; set; }
		public string ReturnUrl { get; set; }
		public string NotifyUrl { get; set; }
		public string PartnerCode { get; set; }
		public string RequestType { get; set; }
	}
}
