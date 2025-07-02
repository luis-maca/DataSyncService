using DataSyncService.Domain.Repositories.CoreRepository.Models;
using DataSyncService.Services.Validation;
using Microsoft.AspNetCore.Mvc;
using OneOf;

namespace DataSyncService.Services.Interfaces
{
	public interface IDataSyncService
	{
		Task<OneOf<IEnumerable<CoreCompany>, NoContentResult, ValidationFailed>> GetCompaniesAsync(CancellationToken cancellationToken);
		Task<OneOf<IEnumerable<CoreCompany>, NoContentResult, ValidationFailed>> CreateCompaniesAsync(CancellationToken cancellationToken);
	}
}
