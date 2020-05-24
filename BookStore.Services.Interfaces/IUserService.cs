using BookStore.Services.MessageTypes;
using System.ServiceModel;

namespace BookStore.Services.Interfaces
{
    [ServiceContract]
    public interface IUserService
    {
        [OperationContract]
        void CreateUser(User pUser);

        [OperationContract]
        User ReadUserById(int pUserId);

        [OperationContract]
        void UpdateUser(User pUser);

        [OperationContract]
        void DeleteUser(User pUser);

        [OperationContract]
        bool ValidateUserLoginCredentials(string username, string password);

        [OperationContract]
        User GetUserByUserNamePassword(string username, string password);

        [OperationContract]
        User GetUserByUserName(string username);

        [OperationContract]
        User GetUserByEmail(string email);
    }
}
