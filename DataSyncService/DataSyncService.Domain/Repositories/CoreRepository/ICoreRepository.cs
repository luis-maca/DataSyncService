using DataSyncService.Domain.Repositories.CoreRepository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSyncService.Domain.Repositories.CoreRepository
{
	public interface ICoreRepository
	{
		Task<IEnumerable<CoreCompany>?> GetCompany(int timeSpan);
	}
}
