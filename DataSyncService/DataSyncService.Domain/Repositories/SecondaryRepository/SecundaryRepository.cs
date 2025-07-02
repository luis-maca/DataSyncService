using Dapper;
using DataSyncService.Domain.Repositories.SecondaryRepository.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSyncService.Domain.Repositories.SecondaryRepository
{
	public class SecundaryRepository : ISecundaryRepository
	{

		private readonly SqlConnection _connection;
		public SecundaryRepository(SqlConnection connection) { _connection = connection; }

		public async Task CreateCompaniesBulk(IEnumerable<SecundaryCompany> companies)
		{
			using var transaction = _connection.BeginTransaction();
			try
			{
				var createSql = @"
                                INSERT INTO Company 
                                VALUES (@Id, @CoreCompanyId, @Name, @Description, @PhoneNumber, @Status);
                                ";

				// Execute the bulk insert
				await _connection.ExecuteAsync(createSql, companies, transaction: transaction);

				transaction.Commit();
			}
			catch
			{
				transaction?.Rollback();
				throw;
			}
		}

		public async Task<IEnumerable<SecundaryCompany>> GetCompanyByCoreCompanyIdList(List<Guid> coreCompanyIds)
		{
			if (coreCompanyIds == null || !coreCompanyIds.Any())
				return Enumerable.Empty<SecundaryCompany>();

			var query = @"
                        SELECT *
                        FROM Company
                        WHERE CoreCompanyId IN @coreCompanyIds;
                        ";

			var companies = await _connection.QueryAsync<SecundaryCompany>(
				query,
				new { LegacyCompanyIds = coreCompanyIds }
			);
			return companies;

		}

		public async Task<IEnumerable<Guid>> GetLatestCompanyIdAsync(int timeSpan)
		{
			var query = @"
            SELECT CoreCompanyId
            FROM Company
            WHERE CreatedOn >= DATEADD(HOUR, -@timeSpan, GETDATE())
            AND IsDeleted = 0
            ORDER BY CreatedOn DESC;
            ";

			var coreCompanyIds = await _connection.QueryAsync<Guid>(query, new { timeSpan });

			return coreCompanyIds;
		}
	}
}
