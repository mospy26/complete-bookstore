using System;
using System.Runtime.Serialization;

namespace BookStore.Business.Entities
{
    public partial class LoginCredential
    {

        public LoginCredential()
        {
        }

        [DataMember]
        public String Password
        {
            get
            {
                return EncryptedPassword;
            }

            set
            {
                String lSetValue = Common.Cryptography.sha512encrypt(value);
                if (/*!this.IsDeserializing && */this.EncryptedPassword != lSetValue)
                {
                    this.EncryptedPassword = lSetValue;
                }
            }
        }

    }
}
