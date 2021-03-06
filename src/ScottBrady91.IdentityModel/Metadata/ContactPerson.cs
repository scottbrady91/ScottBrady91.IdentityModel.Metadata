﻿using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScottBrady91.IdentityModel.Metadata
{
    public class ContactPerson
    {
        public ContactType Type { get; set; }
        public string Company { get; set; }
		public string GivenName { get; set; }
		public string Surname { get; set; }
        public ICollection<string> EmailAddresses { get; } = new Collection<string>();
        public ICollection<string> TelephoneNumbers { get; } = new Collection<string>();

        public ContactPerson() { }
		public ContactPerson(ContactType type)
		{
			Type = type;
		}
    }
}
