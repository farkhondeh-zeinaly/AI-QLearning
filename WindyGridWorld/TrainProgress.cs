using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindyGridWorld
{
    public class TrainProgress
    {
        public int[,] State { get; set; }
        public int episode { get; set; }
        public double SumRewards { get; set; }
    }
}
