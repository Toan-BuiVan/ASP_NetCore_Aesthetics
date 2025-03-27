using Aesthetics.DataAccess.NetCore.DBContext;
using Aesthetics.DataAccess.NetCore.Repositories.Interfaces;
using Aesthetics.DTO.NetCore.DataObject.Model;
using Aesthetics.DTO.NetCore.RequestData;
using Aesthetics.DTO.NetCore.ResponeFunctions;
using BE_102024.DataAces.NetCore.Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DataAccess.NetCore.Repositories.Implement
{
	public class FunctionRepository : BaseApplicationService, IFunctionRepository
	{
		private DB_Context _context;
		private IConfiguration _configuration;
		public FunctionRepository(DB_Context context,
			IConfiguration configuration, IServiceProvider serviceProvider) : base(serviceProvider)
		{
			_context = context;
			_configuration = configuration;
		}

		public async Task<ResponeFunctions> Insert_Function(FunctionRequest insert_)
		{
			throw new NotImplementedException();
		}

		public Task<ResponeFunctions> Update_Function(Update_Function update_)
		{
			throw new NotImplementedException();
		}

		public Task<ResponeFunctions> Delete_Function(Delete_Function delete_)
		{
			throw new NotImplementedException();
		}

		public Task<ResponeFunctions> GetList_SearchFunction(GetList_SearchFunction getList_)
		{
			throw new NotImplementedException();
		}
		public Task<Functions> GetFunctionByFunctionCode(string functionCode)
		{
			throw new NotImplementedException();
		}

		public Task<Functions> GetFunctionByFunctionName(string functionName)
		{
			throw new NotImplementedException();
		}

		
	}
}
