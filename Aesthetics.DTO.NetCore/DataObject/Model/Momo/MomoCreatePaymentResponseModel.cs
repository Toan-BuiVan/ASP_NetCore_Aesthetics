using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DTO.NetCore.DataObject.Model.Momo
{
	public class MomoCreatePaymentResponseModel
	{
		public string RequestId { get; set; }
		public int ErrorCode { get; set; }
		public string OrderId { get; set; }
		public string Message { get; set; }
		public string LocalMessage { get; set; }
		public string RequestType { get; set; }

		[JsonProperty("payUrl")]
		public string PayUrl { get; set; }

		[JsonProperty("signature")]
		public string Signature { get; set; }

		[JsonProperty("qrCodeUrl")]
		public string QrCodeUrl { get; set; }

		[JsonProperty("deeplink")]
		public string Deeplink { get; set; }

		[JsonProperty("deeplinkWebInApp")]
		public string DeeplinkWebInApp { get; set; }
	}

}
