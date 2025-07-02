using DataSyncService.Domain.Entities.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSyncService.Domain.Entities
{
	public class Configuration : BaseEntity
	{
		public Configuration(string name, string value)
		{
			Name = name;
			Value = value;
		}
		public string Name { get; private set; }
		public string Value { get; private set; }
	}
}
