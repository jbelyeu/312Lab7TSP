using System;
using System.Collections.Generic;
using System.Text;

namespace TSP
{
    class Pair
    {
        public Pair(double iDepth, double iBound)
        {
            depth = iDepth;
            bound = iBound;
        }
        public Pair() { }

        private double depth;
        private double bound;


        public double Depth
        {
            get { return depth; }
            set { depth = value; }
        }

        public double Bound
        {
            get { return bound; }
            set { bound = value; }
        }
    }
}
