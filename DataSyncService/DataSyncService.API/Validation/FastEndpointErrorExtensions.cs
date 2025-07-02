using DataSyncService.Services.Validation;
using FastEndpoints;
namespace DataSyncService.API.Validation
{
	public static class FastEndpointErrorExtensions
	{
		public static void UseProblemDetailsResponseBuilder(this ErrorOptions errorsConfig)
		{
			errorsConfig.ResponseBuilder = (failures, ctx, statusCode) =>
			{
				return new Microsoft.AspNetCore.Mvc.ProblemDetails
				{
					Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
					Title = "One or more validation errors occurred.",
					Status = statusCode,
					Instance = ctx.Request.Path,
					Extensions = {
					{ "errors", failures
						.Select(error => new ValidationResponse(error.PropertyName, error.ErrorMessage))
						.ToList() },
					{ "traceId", ctx.TraceIdentifier }
				}
				};
			};

		}
	}
}
