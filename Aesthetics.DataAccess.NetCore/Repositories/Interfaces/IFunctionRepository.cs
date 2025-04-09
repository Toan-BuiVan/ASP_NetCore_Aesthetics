using Aesthetics.DTO.NetCore.DataObject.Model;
using Aesthetics.DTO.NetCore.RequestData;
using Aesthetics.DTO.NetCore.ResponeFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DataAccess.NetCore.Repositories.Interfaces
{
    public interface IFunctionRepository
    {
        //1.Insert Function
        Task<ResponeFunctions> Insert_Function(FunctionRequest insert_);

		//2.Update Function
		Task<ResponeFunctions> Update_Function(Update_Function update_);

		//3.Delete Function
		Task<ResponeFunctions> Delete_Function(Delete_Function delete_);

		//4.Get list & search Function
		Task<ResponeFunctions> GetList_SearchFunction(GetList_SearchFunction getList_);

		//5.Get Function by FunctionCode
		Task<Functions> GetFunctionByFunctionCode(string functionCode);

		//6.Get Function by FunctionName
		Task<Functions> GetFunctionByFunctionName(string functionName);
	}
}
