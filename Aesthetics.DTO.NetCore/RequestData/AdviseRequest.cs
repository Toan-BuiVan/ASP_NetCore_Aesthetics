using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DTO.NetCore.RequestData
{
    public class AdviseRequest
    {
		public string? FullName { get; set; }
		public string? Phone { get; set; }
		public string? Email { get; set; }
		public string? Content { get; set; }
	}

	public class Update_Advise
	{
		public int AdviseID { get; set; }
	}

	public class GetLisr_SearchAdvise
	{
		public string? FullName { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
	}
}
