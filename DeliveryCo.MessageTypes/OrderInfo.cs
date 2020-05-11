using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryCo.MessageTypes
{
    public class OrderInfo
    {

        public List<Tuple<String, List<String>>> OrderItem { get; } 

        public void AddOrderItem(String pOrderitem, List<String> pWarehouses)
        {
            OrderItem.Add(new Tuple<String, List<String>>(pOrderitem, pWarehouses));
        }

        public OrderInfo()
        {
            OrderItem = new List<Tuple<String, List<String>>>();
        }

        
    }
}
