using Microsoft.Practices.ServiceLocation;

namespace EmailService.Services
{
    public class ServiceFactory
    {
        public static T GetService<T>()
        {
            return ServiceLocator.Current.GetInstance<T>();
        }
    }
}
