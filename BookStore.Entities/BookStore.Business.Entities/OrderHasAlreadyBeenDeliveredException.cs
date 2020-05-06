using System;
using System.Collections.Generic;using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Business.Entities
{
    public class OrderHasAlreadyBeenDeliveredException : Exception
    {
        public int OrderId { get; set; }
    }
}
