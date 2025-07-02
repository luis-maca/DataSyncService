namespace DataSyncService.API.Features.SyncData
{
	public class DataOutOfSyncResponses
	{
		public int TotalCount { get; set; }
		public Dictionary<string, object>? Data { get; set; }
	}
}
