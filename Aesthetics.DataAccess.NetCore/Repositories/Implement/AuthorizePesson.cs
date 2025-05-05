using Aesthetics.DataAccess.NetCore.DBContext;
using Aesthetics.DataAccess.NetCore.Repositories.Interfaces;
using BE_102024.DataAces.NetCore.Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DataAccess.NetCore.Repositories.Implement
{
    public class AuthorizePesson : BaseApplicationService, IAuthorizePesson
	{
		private DB_Context _context;
		private IConfiguration _configuration;
		public AuthorizePesson(DB_Context context, IConfiguration configuration, IServiceProvider serviceProvider) : base(serviceProvider)
		{
			_context = context;
			_configuration = configuration;
		}

		public async Task AuthorizeAdmin(int userID)
		{
			throw new NotImplementedException();
		}

		public Task AuthorizeCustomer(int userID)
		{
			throw new NotImplementedException();
		}

		public Task AuthorizeDoctor(int userID)
		{
			throw new NotImplementedException();
		}

		public Task AuthorizeEmployee(int userID)
		{
			throw new NotImplementedException();
		}
	}
}
