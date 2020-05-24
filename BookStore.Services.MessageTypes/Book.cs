﻿using System;

namespace BookStore.Services.MessageTypes
{
    public class Book : MessageType
    {
        public String Title { get; set; }
        public String Author { get; set; }
        public String Genre { get; set; }
        public double Price { get; set; }

        public int StockCount { get; set; }
    }
}
