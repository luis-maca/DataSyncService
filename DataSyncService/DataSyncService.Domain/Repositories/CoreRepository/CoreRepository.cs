using DataSyncService.Domain.Repositories.CoreRepository.Models;
using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;

namespace DataSyncService.Domain.Repositories.CoreRepository
{
	public class CoreRepository : ICoreRepository
	{
		private readonly SqlConnection _connection;
		public CoreRepository(SqlConnection connection) { _connection = connection; }

		public async Task<IEnumerable<CoreCompany>?> GetCompany(int timeSpan)
		{
			if (timeSpan < 0)
				throw new ArgumentException("The time span can't be less than zero.");

			if (_connection.State != ConnectionState.Open)
				await _connection.OpenAsync();

			var query = @"
                        SELECT *
                        FROM CoreCompany
                        WHERE CreatedOn >= DATEADD(HOUR, -@timeSpan, GETDATE())
                        AND IsDeleted = 0
                        ORDER BY CreatedOn DESC;
                        "
			;

			return await _connection.QueryAsync<CoreCompany>(query, new { timeSpan });
		}
	}
}
