﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderLibrary.Models
{
    public class StatusChangeRequest : Order
    {
        public string IsOrderStatus { get; set; }
    }
}
