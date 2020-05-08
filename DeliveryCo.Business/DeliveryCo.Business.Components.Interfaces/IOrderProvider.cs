using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryCo.Business.Components.Interfaces
{
    public class OrderMessage
    {
        String message { get; set; }
    }
    public interface IOrderProvider
    {
        void NotifyMessage(String message);
    }
}
