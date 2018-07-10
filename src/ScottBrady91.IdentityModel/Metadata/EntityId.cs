using System;

namespace ScottBrady91.IdentityModel.Metadata
{
    public class EntityId
    {
		public string Id { get; set; }

		public EntityId(string id)
		{
			Id = id;
		}

		public EntityId() :
			this(null)
		{
		}
	}
}
