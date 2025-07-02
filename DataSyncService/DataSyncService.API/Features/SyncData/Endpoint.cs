using DataSyncService.Services.Interfaces;
using FastEndpoints;

namespace DataSyncService.API.Features.SyncData
{
	public class SyncDataEndpoint: EndpointWithoutRequest
	{
		private readonly IDataSyncService _dataSyncService;
		private readonly ILogger<SyncDataEndpoint> _logger;

		public SyncDataEndpoint(IDataSyncService dataSyncService, ILogger<SyncDataEndpoint> logger)
		{
			_dataSyncService = dataSyncService;
			_logger = logger;
		}

		public override void Configure()
		{
			Post("/DataOutOfSync");
			Description(x => x
				.WithName("SyncData")
			.Produces<DataOutOfSyncResponses>(200)
			.Produces(204) // NoContent
			.Produces(400) // BadRequest
			.Produces(500) // InternalServerError
			);

		}

		public override async Task HandleAsync(CancellationToken ct)
		{
			if (ct.IsCancellationRequested)
			{
				_logger.LogWarning("Request was canceled.");
				return;
			}

			var companies = await _dataSyncService.CreateCompaniesAsync(ct);

			var result = new DataOutOfSyncResponses
			{
				TotalCount = 0,
				Data = new Dictionary<string, object>()
			};

			var processedCompanies = companies.Match<DataOutOfSyncResponses>(
				coreCompanyIds =>
				{
					result.Data["CoreCompany"] = coreCompanyIds;
					result.TotalCount += coreCompanyIds.Count();
					return result;
				},
				noContent => result,
				validationFailed =>
				{
					_logger.LogError($"Validation failed in data sync process: {validationFailed}");
					return result;
				}
			);

			await SendAsync(result, cancellation: ct);
		}
	}

	
}
