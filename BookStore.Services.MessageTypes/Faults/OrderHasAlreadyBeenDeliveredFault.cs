using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Services.MessageTypes
{
    public class OrderHasAlreadyBeenDeliveredFault
    {
        public int OrderId { get; set; }
    }
}
