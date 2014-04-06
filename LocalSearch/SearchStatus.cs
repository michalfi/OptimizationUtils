using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalSearch
{
    public class SearchValueEventArgs
    {
        public double Value { get; private set; }
        public int? WorkerId { get; private set; }
        public SearchValueEventArgs(double value)
        {
            Value = value;
            WorkerId = null;
        }
        public SearchValueEventArgs(double value, int worker)
        {
            Value = value;
            WorkerId = worker;
        }
    }

    public delegate void SearchValueEventHandler(object sender, SearchValueEventArgs eventArgs);
}
