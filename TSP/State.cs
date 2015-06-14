using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace TSP
{
    class State
    {
        public State()
        {
            lowerBound = double.PositiveInfinity;
            routeSoFar = new TSPSolution();
            visitedCityIndices = new HashSet<int>();
        }


        private Double[,] reducedMatrix;
        private Double lowerBound;
        private TSPSolution routeSoFar;
        private HashSet<int> visitedCityIndices;


        public Double[,] ReducedMatrix
        {
            get { return reducedMatrix; }
            set { reducedMatrix = value; }
        }

        public Double LowerBound
        {
            get { return lowerBound; }
            set { lowerBound = value; }
        }

        public TSPSolution RouteSoFar
        {
            get { return routeSoFar; }
            set { routeSoFar = value; }
        }

        public HashSet<int> VisitedCityIndices
        {
            get { return visitedCityIndices; }
            set { visitedCityIndices = value; }
        }

        /// <summary>
        /// This function will find a reduced cost matrix for the state 
        /// with bound of cost of route so far plus lower bound of what's left
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public void buildMatrix(City[] Cities)
        {
            double[,] reducedMatrix = new double[Cities.Length, Cities.Length]; //init matrix
            double bound = 0;
            double costSoFar = this.costSoFar();

            for (int i = 0; i < Cities.Length; ++i)
            {
                //if (this.VisitedCityIndices.Contains(i))
                //{
                //    reducedMatrix[i, 0] = double.PositiveInfinity;
                //    continue; //this city has been settled
                //}

                double minDist = Double.PositiveInfinity;

                for (int j = 0; j < Cities.Length; ++j)
                {
                    if (this.VisitedCityIndices.Contains(j) || this.VisitedCityIndices.Contains(i))
                    {
                        reducedMatrix[i, j] = Double.PositiveInfinity;
                        continue; //this city has been settled
                    }

                    //loop through the Cities array ^2 times and find the distance between the higher level city (i)
                    //and the lower level city (j). Put the distance into the reduced matrix in the array at i.
                    if (i == j)
                    {
                        reducedMatrix[i, j] = Double.PositiveInfinity;
                        continue; //it's the same city, infinite distance to itself
                    }
                    reducedMatrix[i, j] = Cities[i].costToGetTo(Cities[j]); //put the distance i-j in the matrix
                    if (reducedMatrix[i, j] < minDist) //if this is the smallest distance from i so far
                    {
                        minDist = reducedMatrix[i, j]; //remember the smallest distance so far
                    }
                }
                //once we have the min distance from city i, if it's not 0, reloop through the matrix and reduce 
                //all distances by the minDist, then add to bound
                if (minDist > 0 && minDist < Double.PositiveInfinity)
                {
                    //For each j, remember the cost and loop back through after finishing the initial n loops of j
                    for (int j = 0; j < Cities.Length; ++j)
                    {
                        reducedMatrix[i, j] -= minDist;
                    }
                    bound += minDist;
                }
            }
            //to subtract the smallest cost from i to j from all i to j values. Store all of these min values
            //in a bound variable.

            //TODO: There may be a faster way to do this, but for now, once that is done (all rows are reduced), 
            //loop back through the matrix and find all columns without a 0 value (find min of column and if not 0,
            //it needs to be reduced further). Loop back through if necessary and subtract min from all distances,
            //add that min to the bound.
            for (int i = 0; i < Cities.Length; ++i)
            {
                if (this.VisitedCityIndices.Contains(i))
                {
                    continue; //this city has been settled
                }
                double minDist = Double.PositiveInfinity;
                for (int j = 0; j < Cities.Length; ++j)
                {
                    if (VisitedCityIndices.Contains(j))
                    {
                        continue; //this city has been settled
                    }
                    if (reducedMatrix[j, i] < minDist)  //reversing the indices makes the loop traverse down the columns
                    {
                        minDist = reducedMatrix[j, i];
                        if (minDist <= 0)
                        {
                            break; //already reached a zero, no need to keep looking
                        }
                    }
                }
                if (minDist > 0)
                {
                    for (int j = 0; j < Cities.Length; ++j)
                    {
                        reducedMatrix[j, i] -= minDist; //actually perform the reduction
                    }
                    bound += minDist;
                }
            }

            this.LowerBound = bound + costSoFar;
            this.ReducedMatrix = reducedMatrix;
            //Now we have the reduced matrix and the bound is set
        }

        private double costSoFar()
        {
            double cost = 0;
            if (this.visitedCityIndices.Count > 0) 
            {
                for (int i = 0; i < visitedCityIndices.Count -1; ++i )
                {
                    //i is the start point each time, going to i+1
                    cost += this.reducedMatrix[i, i + 1]; 
                }
            }
            return cost;
        }
    }
}
