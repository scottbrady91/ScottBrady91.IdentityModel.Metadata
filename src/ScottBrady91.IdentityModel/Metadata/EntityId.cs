using System;

namespace ScottBrady91.IdentityModel.Metadata
{
    public class EntityId
    {
        private const int MaximumLength = 1024;
        private string id;

        public string Id
        {
            get => id;
            set
            {
                if (value != null)
                {
                    if (MaximumLength < value.Length)
                    {
                        throw new ArgumentException($"Id length must be less than {MaximumLength}", nameof(Id));
                    }
                }

                id = value;
            }
        }

        public EntityId() { }
        public EntityId(string id)
		{
			Id = id;
		}
	}
}
