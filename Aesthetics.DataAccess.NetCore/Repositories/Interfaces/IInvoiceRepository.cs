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
	}
}
