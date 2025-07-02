using FluentValidation.Results;
namespace DataSyncService.Services.Validation
{
	public record ValidationFailed(IEnumerable<ValidationFailure> Errors)
	{
		public ValidationFailed(ValidationFailure error) : this(new[] { error })
		{
		}
	}
}
