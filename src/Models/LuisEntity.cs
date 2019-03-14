using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BasicBot.Models
{
    public class LuisEntity
    {
        public string Text { get; set; }

        public string Type { get; set; }

        public double Score { get; set; }
    }
}
