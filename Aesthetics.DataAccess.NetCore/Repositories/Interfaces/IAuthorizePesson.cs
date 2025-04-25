using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DataAccess.NetCore.Repositories.Interfaces
{
    public interface IAuthorizePesson
    {
        public Task AuthorizeCustomer(int userID);

		public Task AuthorizeEmployee(int userID);

		public Task AuthorizeDoctor(int userID);

		public Task AuthorizeAdmin(int userID);
	}
}
