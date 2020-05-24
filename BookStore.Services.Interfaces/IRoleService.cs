using BookStore.Services.MessageTypes;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace BookStore.Services.Interfaces
{
    [ServiceContract]
    public interface IRoleService
    {
        [OperationContract]
        List<Role> GetRolesForUser(User pUser);

        [OperationContract]
        List<Role> GetRolesForUserName(String pUserName);
    }
}
