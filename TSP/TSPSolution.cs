using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP
{
    public class TSPSolution
    {
        /// <summary>
        /// we use the representation [cityB,cityA,cityC] 
        /// to mean that cityB is the first city in the solution, cityA is the second, cityC is the third 
        /// and the edge from cityC to cityB is the final edge in the path.  
        /// you are, of course, free to use a different representation if it would be more convenient or efficient 
        /// for your node data structure and search algorithm. 
        /// </summary>
        public ArrayList Route;

        public TSPSolution(ArrayList iroute)
        {
            Route = new ArrayList(iroute);
        }

        public TSPSolution()
        {
            Route = new ArrayList();
        }

        /// <summary>
        ///  compute the cost of the current route.  Does not check that the route is complete, btw.
        /// assumes that the route passes from the last city back to the first city. 
        /// </summary>
        /// <returns></returns>
        public double costOfRoute()
        {
            // go through each edge in the route and add up the cost. 
            int x;
            City here;
            double cost = 0D;

            for (x = 0; x < Route.Count - 1; x++)
            {
                here = Route[x] as City;
                cost += here.costToGetTo(Route[x + 1] as City);
            }
            // go from the last city to the first. 
            here = Route[Route.Count - 1] as City;
            cost += here.costToGetTo(Route[0] as City);
            return cost;
        }

        public double costOfRouteSoFar()
        {
            // go through each edge in the route and add up the cost. 
            int x;
            City here;
            double cost = 0D;

            for (x = 0; x < Route.Count - 1; x++)
            {
                here = Route[x] as City;
                cost += here.costToGetTo(Route[x + 1] as City);
            }
            return cost;
        }
    }
}
