using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindyGridWorld
{
    public class StepResult
    {
        public int[,] State { get; set; }
        public double Reward { get; set; }

    }
}
