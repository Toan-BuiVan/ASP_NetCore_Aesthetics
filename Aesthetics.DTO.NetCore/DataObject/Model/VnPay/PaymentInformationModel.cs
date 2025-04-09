using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DTO.NetCore.DataObject.Model.VnPay
{
	public class PaymentInformationModel
	{
		public string OrderID { get; set; }
		public double Amount { get; set; }
		public string OrderDescription { get; set; }
		public string Name { get; set; }
	}
}
