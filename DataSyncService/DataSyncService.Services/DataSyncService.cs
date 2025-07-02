using DataSyncService.Database;
using DataSyncService.Domain.Entities;
using DataSyncService.Domain.Repositories.CoreRepository;
using DataSyncService.Domain.Repositories.CoreRepository.Models;
using DataSyncService.Domain.Repositories.SecondaryRepository;
using DataSyncService.Domain.Repositories.SecondaryRepository.Models;
using DataSyncService.Services.Interfaces;
using DataSyncService.Services.Validation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;

namespace DataSyncService.Services
{
	public class DataSyncService(
		ICoreRepository _coreRepository,
		ISecundaryRepository _secundaryRepository,
		ILogger<DataSyncService> _logger,
		DataSyncDbContext _dbContext) : IDataSyncService
	{

		public async Task<OneOf<IEnumerable<CoreCompany>, NoContentResult, ValidationFailed>> GetCompaniesAsync(CancellationToken cancellationToken)
		{
			return await GetOutOfSyncCompanies(cancellationToken)
				.ConfigureAwait(false);
		}

		public async Task<OneOf<IEnumerable<CoreCompany>, NoContentResult, ValidationFailed>> CreateCompaniesAsync(CancellationToken cancellationToken)
		{
			var outOfSyncCompanies = await GetOutOfSyncCompanies(cancellationToken)
				.ConfigureAwait(false);

			if (outOfSyncCompanies.IsT1)
			{
				LogNoCompaniesOutOfSync();
				return outOfSyncCompanies.AsT1;
			}
			if (outOfSyncCompanies.IsT2)
			{
				LogNoCompaniesOutOfSync();
				return outOfSyncCompanies.AsT2;
			}

			var legacyCompanyIds = outOfSyncCompanies.AsT0;

			var existingCompanyIds = (await _secundaryRepository.GetCompanyByCoreCompanyIdList(
									 legacyCompanyIds.Select(x => x.Id).ToList()))
									 .Select(c => c.CoreCompanyId)
									 .ToHashSet();

			var newCompanies = legacyCompanyIds
				.Where(c => !existingCompanyIds.Contains(c.Id))
				.Select(c => new SecundaryCompany
				{
					CoreCompanyId = c.Id,
					Name = c.Name,
					Description = c.Description,
					PhoneNumber = c.PhoneNumber,
					Status = c.Status,
					Id = new Guid()
				}).ToList();

			if (!newCompanies.Any())
			{
				LogNoCompaniesOutOfSync();
				return new NoContentResult();
			}

			// Insert new data in bulk
			await _secundaryRepository.CreateCompaniesBulk(newCompanies);

			var syncLog = new SyncLog("CreateCompaniesOutOfSync",
				$"Total Records Synced: {newCompanies.Count}, {string.Join(", ", newCompanies.Select(c => $"({c.CoreCompanyId})"))}",
				"");

			await _dbContext.SyncLog.AddAsync(syncLog);
			await _dbContext.SaveChangesAsync();

			var mappedCompanies = newCompanies.Select(c => new CoreCompany
			{
				Name = c.Name,
				Status = c.Status,
				Description = c.Description,
				PhoneNumber = c.PhoneNumber,
				Id = c.Id,
			}).ToList();

			return mappedCompanies;
		}

		private async Task<OneOf<IEnumerable<CoreCompany>, NoContentResult, ValidationFailed>> GetOutOfSyncCompanies(CancellationToken cancellationToken)
		{
			try
			{
				//Create in the SyncDatabase.Configuration table a record with the name CompaniesHoursToSync to works as a parameter for the sync process}
				//and how much time back look for to sync
				var companiesHoursToSyncValue = await _dbContext.Configuration
					.Where(c => c.Name == "CompaniesHoursToSync")
					.Select(c => c.Value)
					.FirstOrDefaultAsync(cancellationToken);

				if (string.IsNullOrEmpty(companiesHoursToSyncValue))
				{
					_logger.LogWarning("No configuration found for 'CompaniesHoursToSync'.");
					return new ValidationFailed(new ValidationFailure("CompaniesHoursToSync", "No configuration found for 'CompaniesHoursToSync'"));
				}

				var listCoreCompanyDetails = await _coreRepository.GetCompany(int.Parse(companiesHoursToSyncValue)) ?? [];

				var listSecundayCompanyDetails = await _secundaryRepository.GetLatestCompanyIdAsync(int.Parse(companiesHoursToSyncValue)) ?? [];

				var outOfSyncCompanies = listSecundayCompanyDetails == null || !listSecundayCompanyDetails.Any()
					? listCoreCompanyDetails
					: listCoreCompanyDetails.Where(c => !listSecundayCompanyDetails.Contains(c.Id)).ToList();

				if (outOfSyncCompanies.Any())
				{
					_logger.LogInformation($"Out of sync companies: {string.Join(", ", outOfSyncCompanies.Select(c => c.Id))}");
					return Enumerable.ToList(outOfSyncCompanies);
				}

				return new NoContentResult();
			}
			catch (Exception ex) 
			{
				_logger.LogError(ex, "Error occurred while getting out of sync companies");
				return new ValidationFailed(new[] { new FluentValidation.Results.ValidationFailure("DataSync", "An error occurred while fetching out of sync companies") });
			}
		}

		private async void LogNoCompaniesOutOfSync()
		{
			_logger.LogInformation("No companies were created as all were already in sync.");
			var syncLogEmpty = new SyncLog("CreateCompaniesOutOfSync", "Total Records Synced: 0", "No companies were created as all were already in sync.");
			await _dbContext.SyncLog.AddAsync(syncLogEmpty);
			await _dbContext.SaveChangesAsync();

		}

	}
}
