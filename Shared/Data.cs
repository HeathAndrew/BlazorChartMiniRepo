using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorTestApp.Shared
{
    public class Data
    {
        public double xValue { get; set; }
        public double yValue { get; set; }

        public Data(int x, float y)
        {
            xValue = x;
            yValue = y;
        }
    }
}
