using System;
using System.Collections.Generic;

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
