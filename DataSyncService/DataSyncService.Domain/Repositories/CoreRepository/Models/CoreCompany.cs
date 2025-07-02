using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSyncService.Domain.Repositories.CoreRepository.Models
{
	public class CoreCompany
	{
		public Guid Id { get; set; }
		public string? Name { get; set; }
		public string? Description { get; set; }
		public string? PhoneNumber { get; set; }
		public string? Status { get; set; }
	}
}
