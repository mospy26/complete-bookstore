using BookStore.Business.Entities;
using System;
using System.Collections.Generic;

namespace BookStore.Business.Components.Interfaces
{
    public interface IRoleProvider
    {
        List<Role> GetRolesForUser(User pUser);
        List<Role> GetRolesForUserName(String pUserName);
    }
}
