using DataSyncService.Domain.Repositories.SecondaryRepository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSyncService.Domain.Repositories.SecondaryRepository
{
	public interface ISecundaryRepository
	{
		Task CreateCompaniesBulk(IEnumerable<SecundaryCompany> companies);
		Task<IEnumerable<SecundaryCompany>> GetCompanyByCoreCompanyIdList(List<Guid> coreCompanyIdList);

		Task<IEnumerable<Guid>> GetLatestCompanyIdAsync(int timeSpan);
	}
}
