using Aesthetics.DTO.NetCore.DataObject.Model;
using Aesthetics.DTO.NetCore.RequestData;
using Aesthetics.DTO.NetCore.ResponseInvoice_Loggin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DataAccess.NetCore.Repositories.Interfaces
{
    public interface IInvoiceRepository
    {
		//1. Insert Invoice
		Task<ResponseInvoice_Loggin> Insert_Invoice(InvoiceRequest insert_);

		//2. Delete Invoice
		Task<ResponseInvoice_Loggin> Delete_Invoice(Delete_Invoice delete_);

		//3. Get list search Invoice
		Task<ResponseGetListInvoice> GetList_SearchInvoice(GetList_Invoice getList_);

		//4. Get list search InvoiceDetail
		Task<ResponseGetListInvoiceDetail> GetList_SearchInvoiceDetail(GetList_InvoiceDetail getList_);

		//5. Get Invoice by InvoiceID 
		Task<Invoice> GetInvoiceByInvoiceID(int InvoiceID);

		//6. Update status Invoice
		Task UpdateStatusInvoice(int InvoiceID);


		//7. Update status InvoiceDetail 
		Task UpdateStatusInvoiceDetail(int InvoiceID);

		//8. Get List InvoiceDetail by InvoiceID 
		Task<List<InvoiceDetail>> InvoiceDetailByInvoiceID(int invoiceID);
	}
}
