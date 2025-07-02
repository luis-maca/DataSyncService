using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DataSyncService.Domain.Entities.Interface
{
	public abstract class BaseEntity
	{
		private int? _requestedHashCode;

		public Guid Id { get; private set; } = Guid.NewGuid();

		public bool IsDeleted { get; protected set; }

		public DateTime CreatedOn { get; private set; } = DateTime.UtcNow;

		public DateTime? LastModifiedOn { get; private set; }

		public string? CreatedBy { get; private set; }

		public string? LastModifiedBy { get; private set; }

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is BaseEntity))
			{
				return false;
			}

			if (this == obj)
			{
				return true;
			}

			if (GetType() != obj.GetType())
			{
				return false;
			}

			return ((BaseEntity)obj).Id == Id;
		}

		public override int GetHashCode()
		{
			if (!_requestedHashCode.HasValue)
			{
				_requestedHashCode = Id.GetHashCode() ^ 0x1F;
			}

			return _requestedHashCode.Value;
		}

		public static bool operator ==(BaseEntity left, BaseEntity right)
		{
			if (object.Equals(left, null))
			{
				return object.Equals(right, null);
			}

			return left.Equals(right);
		}

		public static bool operator !=(BaseEntity left, BaseEntity right)
		{
			return !(left == right);
		}
	}
}
