using Aesthetics.DataAccess.NetCore.CheckConditions.Response;
using Aesthetics.DTO.NetCore.DataObject.LogginModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DTO.NetCore.ResponseInvoice_Loggin
{
    public class ResponseInvoice_Loggin : ResponseData
	{
		public int InvoiceID { get; set; }
		public decimal TotalMoney { get; set; }
		public List<Invoice_Loggin_Ouput>? invoiceOut_Loggin { get; set; }
		public List<InvoiceDetail_Loggin_Ouput>? invoiceDetailOut_Loggin { get; set; }
	}
	public class ResponseGetListInvoice : ResponseData
	{
		public List<GetList_Invoice_Out>? Data { get; set; }
	}

	public class ResponseGetListInvoiceDetail : ResponseData
	{
		public List<GetList_InvoiceDetail_Out>? Data { get; set; }
	}
}
