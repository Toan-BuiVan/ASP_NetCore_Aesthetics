using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DTO.NetCore.DataObject.Model
{
	public class Advise
	{
		public int AdviseID { get; set; }
		public string? FullName { get; set; }
		public string? Phone { get; set; }
		public string? Email { get; set; }
		public string? Content { get; set; }
		public int? ConsultingStatus { get; set; }
		public DateTime CreationDate { get; set; }
	}
}
