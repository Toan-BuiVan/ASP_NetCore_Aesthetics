using Aesthetics.DataAccess.NetCore.CheckConditions.Response;
using Aesthetics.DTO.NetCore.RequestData;
using Aesthetics.DTO.NetCore.ResponseAdvise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DataAccess.NetCore.Repositories.Interfaces
{
    public interface IAdviseRepository
    {
        public Task<ResponseData> InsertAdvise(AdviseRequest adviseRequest);
		public Task<ResponseData> UpdateAdvise(Update_Advise update_Advise);
		public Task<ResponseData> DeleteAdvise(Update_Advise delete_Advise);
		public Task<ResponseAdvise> GetList_SreachAdvise(GetLisr_SearchAdvise getLisr_Search);
	}
}
