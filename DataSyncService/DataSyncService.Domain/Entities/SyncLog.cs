using DataSyncService.Domain.Entities.Interface;

namespace DataSyncService.Domain.Entities
{
	public class SyncLog : BaseEntity
	{
		public SyncLog(string name, string data, string? errorMessage)
		{
			Name = name;
			Data = data;
			ErrorMessage = errorMessage;
		}
		public string Name { get; private set; }
		public string Data { get; private set; }
		public string? ErrorMessage { get; set; }
	}
}
