
namespace DataSyncService.Services.Validation
{
	public class ValidationFailureResponse
	{
		public required IEnumerable<ValidationResponse> Errors { get; init; }
	}

	public record ValidationResponse(string PropertyName, string Message);
}
