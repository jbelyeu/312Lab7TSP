using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;

namespace TSP
{
    class ProblemAndSolver
    {
               #region private members
        private const int DEFAULT_SIZE = 25;
        
        private const int CITY_ICON_SIZE = 5;

        /// <summary>
        /// the cities in the current problem.
        /// </summary>
        private City[] Cities;
        /// <summary>
        /// a route through the current problem, useful as a temporary variable. 
        /// </summary>
        private ArrayList Route;
        /// <summary>
        /// best solution so far. 
        /// </summary>
        private TSPSolution bssf;

        /// <summary>
        /// optimal starting point for B&B, found by the greedy algorithm
        /// </summary>
        private City startCity;

        /// <summary>
        /// Holds the bound from the reduced cost matrix, lower bound
        /// </summary>
        private double classBound;

        /// <summary>
        /// holds the bound from the reduced cost matrix of the bssf, upper bound
        /// </summary>
        private double bssfBound;

        /// <summary>
        /// agenda of childen to expand
        /// </summary>
        private MinPriorityQueue<int, City> agenda;


        /// <summary>
        /// how to color various things. 
        /// </summary>
        private Brush cityBrushStartStyle;
        private Brush cityBrushStyle;
        private Pen routePenStyle;

        private const int TIMELIMIT = 60;
        private const int DEBUGTIMELIMIT = Int16.MaxValue;


        /// <summary>
        /// keep track of the seed value so that the same sequence of problems can be 
        /// regenerated next time the generator is run. 
        /// </summary>
        private int _seed;
        /// <summary>
        /// number of cities to include in a problem. 
        /// </summary>
        private int _size;

        /// <summary>
        /// random number generator. 
        /// </summary>
        private Random rnd;
        #endregion

        #region public members.
        public int Size
        {
            get { return _size; }
        }

        public int Seed
        {
            get { return _seed; }
        }
        #endregion

        public const int DEFAULT_SEED = -1;

        #region Constructors
        public ProblemAndSolver()
        {
            initialize(DEFAULT_SEED, DEFAULT_SIZE);
        }

        public ProblemAndSolver(int seed)
        {
            initialize(seed, DEFAULT_SIZE);
        }

        public ProblemAndSolver(int seed, int size)
        {
            initialize(seed, size);
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// reset the problem instance. 
        /// </summary>
        private void resetData()
        {
            Cities = new City[_size];
            Route = new ArrayList(_size);
            bssf = null; 

            for (int i = 0; i < _size; i++)
                Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble());

            cityBrushStyle = new SolidBrush(Color.Black);
            cityBrushStartStyle = new SolidBrush(Color.Red);
            routePenStyle = new Pen(Color.LightGray,1);
            routePenStyle.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
        }

        private void initialize(int seed, int size)
        {
            this._seed = seed;
            this._size = size;
            if (seed != DEFAULT_SEED)
                this.rnd = new Random(seed);
            else
                this.rnd = new Random();
            this.resetData();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// make a new problem with the given size.
        /// </summary>
        /// <param name="size">number of cities</param>
        public void GenerateProblem(int size)
        {
            this._size = size;
            resetData(); 
        }

        /// <summary>
        /// return a copy of the cities in this problem. 
        /// </summary>
        /// <returns>array of cities</returns>
        public City[] GetCities()
        {
            City[] retCities = new City[Cities.Length];
            Array.Copy(Cities, retCities, Cities.Length);
            return retCities;
        }

        /// <summary>
        /// draw the cities in the problem.  if the bssf member is defined, then
        /// draw that too. 
        /// </summary>
        /// <param name="g">where to draw the stuff</param>
        public void Draw(Graphics g)
        {
            float width  = g.VisibleClipBounds.Width-45F;
            float height = g.VisibleClipBounds.Height-15F;
            Font labelFont = new Font("Arial", 10);

            g.DrawString("n(c) means this node is the nth node in the current solution and incurs cost c to travel to the next node.", labelFont, cityBrushStartStyle, new PointF(0F, 0F)); 

            // Draw lines
            if (bssf != null)
            {
                // make a list of points. 
                Point[] ps = new Point[bssf.Route.Count];
                int index = 0;
                foreach (City c in bssf.Route)
                {
                    if (index < bssf.Route.Count -1)
                        g.DrawString(" " + index +"("+c.costToGetTo(bssf.Route[index+1]as City)+")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    else 
                        g.DrawString(" " + index +"("+c.costToGetTo(bssf.Route[0]as City)+")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    ps[index++] = new Point((int)(c.X * width) + CITY_ICON_SIZE / 2, (int)(c.Y * height) + CITY_ICON_SIZE / 2);
                }

                if (ps.Length > 0)
                {
                    g.DrawLines(routePenStyle, ps);
                    g.FillEllipse(cityBrushStartStyle, (float)Cities[0].X * width - 1, (float)Cities[0].Y * height - 1, CITY_ICON_SIZE + 2, CITY_ICON_SIZE + 2);
                }

                // draw the last line. 
                g.DrawLine(routePenStyle, ps[0], ps[ps.Length - 1]);
            }

            // Draw city dots
            foreach (City c in Cities)
            {
                g.FillEllipse(cityBrushStyle, (float)c.X * width, (float)c.Y * height, CITY_ICON_SIZE, CITY_ICON_SIZE);
            }

        }

        /// <summary>
        ///  return the cost of the best solution so far. 
        /// </summary>
        /// <returns></returns>
        public double costOfBssf ()
        {
            if (bssf != null)
                return (bssf.costOfRoute());
            else
                return -1D; 
        }

        /// <summary>
        ///  solve the problem.  This is the entry point for the solver when the run button is clicked
        /// right now it just picks a simple solution. 
        /// </summary>
        //public void solveProblem()
        //{
        //    int x;
        //    Route = new ArrayList();
        //    // this is the trivial solution. 
        //    for (x = 0; x < Cities.Length; x++)
        //    {
        //        Route.Add(Cities[Cities.Length - x - 1]);
        //    }
        //    // call this the best solution so far.  bssf is the route that will be drawn by the Draw method. 
        //    bssf = new TSPSolution(Route);
        //    // update the cost of the tour. 
        //    Program.MainForm.tbCostOfTour.Text = " " + bssf.costOfRoute();
        //    // do a refresh. 
        //    Program.MainForm.Invalidate();
        //}


        /// <summary>
        ///  solve the problem.  This is the entry point for the solver when the run button is clicked
        /// </summary>
        public void solveProblem()
        {
            Stopwatch timer = Stopwatch.StartNew(); //sixty second timer
            //create state 
            State initState = new State();
            //set reduced matrix and bound for the state
            initState.buildMatrix(this.Cities); //TODO: add end to bound

            //greedily find BSSF
            int startIndex = this.getBSSF(initState.ReducedMatrix, initState.LowerBound); //TODO: add end to bssf bound
            //create comparer and agenda
            VertexComparer comparer = new VertexComparer();
            MinPriorityQueue<Pair, State> agenda = new MinPriorityQueue<Pair, State>(comparer);

            //populate agenda with best start position (found by BSSF tour, first index in chosen bssf)
            initState.RouteSoFar.Route.Add(this.Cities[startIndex]);
            initState.VisitedCityIndices.Add(startIndex);

            Pair priority = new Pair(0, initState.LowerBound);
            agenda.enqueue(priority, initState);

            //loop while agenda has something, time remains, and cost of the cheapest in the agenda is not equal to that found already
            while (!agenda.isEmpty && timer.Elapsed.TotalSeconds < TIMELIMIT && bssfBound != agenda.seeNextVal().LowerBound)
            {
                KeyValuePair<Pair, State> next = agenda.seeNext();
                agenda.dequeue(next.Key, next.Value);

                //this means a route based on the state next could be better than current BSSF
                if (next.Value.LowerBound < this.bssfBound) 
                {
                    //generate a list of all successor state to this next state
                    List<State> children = this.getSuccessors(next.Value);

                    //for each child state, chech potential
                    foreach (State child in children)
                    {
                        if (timer.Elapsed.TotalSeconds >= TIMELIMIT)
                        {
                            break; //out of time
                        }

                        if (child.LowerBound < this.bssfBound )
                        {
                            //if childStateroutesoFar includes all cities
                            if (child.RouteSoFar.Route.Count == this.Cities.Length)
                            {
                                //bssf.Route = child.RouteSoFar.Route;
                                //this.bssfBound = child.LowerBound;
                            }
                            else
                            {
                                Pair childPriority = new Pair(child.RouteSoFar.Route.Count, child.LowerBound);
                                agenda.enqueue(childPriority, child);
                            }                            
                        }
                    }            
                }
            }

            timer.Stop();

            // update the cost of the tour. 
            Program.MainForm.tbCostOfTour.Text = " " + this.costOfBssf();
            //set time used
            Program.MainForm.tbElapsedTime.Text = timer.Elapsed.TotalSeconds.ToString();
            // do a refresh. 
            Program.MainForm.Invalidate();
        }
        #endregion

        //greedily tours the set of cities from every starting point and finds the cheapest path
        private int getBSSF(Double[,] matrix, double lowerBound)
        {
            double localBound = Double.PositiveInfinity;
            int startIndex = -1;

            //initialize class var bssf, create main bound variable
            //For all cities in the Cities list (to compute the BSSF for all possible starting points)
            for (int i = 0; i < this.Cities.Length; ++i)
            {
                //create temp bound variable and temp array or list of cities for bssf
                ArrayList tempbssf = new ArrayList();
                Dictionary<int, bool> visitedCities = new Dictionary<int, bool>();
                Double tempBound = 0;
                int lastIndex = -1;

                tempBound = this.getBSSFFromCity(matrix, tempbssf, tempBound, visitedCities, i, ref lastIndex);
                tempBound += lowerBound; //should get the cost of going from the last city to the first again
                if (tempBound < localBound)
                {
                    if (localBound == 0)
                    {
                        throw new Exception("Invalid tempBound");
                    }

                    localBound = tempBound;
                    this.bssf = new TSPSolution(tempbssf);
                    startIndex = i;
                }
            }
            this.bssfBound = localBound;
            
            //Add closest city to bssf, mark as visited, add cost to bound.
            //When all cities are in the temp bssf list, stop. Check the bound against the main bound. If smaller, overwrite the main bound with the lesser bound
            //and the bssf with the temp bssf.
            //When done, the first city in the bssf should be the starting point for B&B and the bound should be tightish
            return startIndex;
        }

        /// <summary>
        ///  saves the BSSF by ref into tempbssf, bound as well. i says where to find initial city
        /// </summary>
        /// <param name="tempbssf"></param>
        /// <param name="tempBound"></param>
        /// <param name="i"></param>
        private double getBSSFFromCity(Double[,] reducedMatrix, ArrayList tempbssf, Double tempBound, Dictionary<int, bool> visitedCities, int i, ref int lastIndex)
        {
            //start at city i, mark city as visited
            visitedCities[i] = true;
            lastIndex = i;

            //add to initial bssf
            tempbssf.Add(Cities[i]);

            //loop back through cities and find closest city to i that has not been visited.
            double minDist = Double.PositiveInfinity;
            int nextCity = -1;

            for (int j = 0; j < this.Cities.Length; ++j)
            {
                //if the index is there already, we've been there before
                if (visitedCities.ContainsKey(j)) 
                {
                    continue;
                }

                if (reducedMatrix[i, j] < minDist) //reduced cost of going from city i to city j
                {
                    nextCity = j;
                    minDist = reducedMatrix[i, j];
                }

                if (minDist <= 0) //this would have to be the smallest distance possible
                {
                    break;
                }
            }

            if (minDist != Double.PositiveInfinity) //means we have not visited all of the cities
            {
                tempBound = getBSSFFromCity(reducedMatrix, tempbssf, tempBound + minDist, visitedCities, nextCity, ref lastIndex);
            }
            
            return tempBound; //we're done if we get past previous check
        }

        private List<State> getSuccessors(State parent)
        {
            //get last city in the route so far. Route should never be empty when this is called
            List<State> successors = new List<State>();

            for (int i = 0; i < this.Cities.Length; ++i )
            {
                //if this i is not in the state's visited set, it is fair game
                if (! parent.VisitedCityIndices.Contains(i))
                {
                    State newState = new State();
                    for (int j = 0; j < parent.RouteSoFar.Route.Count; ++j)
                    {
                        newState.RouteSoFar.Route.Add(parent.RouteSoFar.Route[j]); //slightly deeper copy than just copying the route
                    }
                    
                    newState.RouteSoFar.Route.Add(this.Cities[i]); //expand to the next city
                    newState.VisitedCityIndices.Add(i); //mark it visited

                    newState.buildMatrix(this.Cities);
                    successors.Add(newState);
                }
            }
            return successors;
        }
    }

    class VertexComparer : IComparer<Pair>
    {
        public int Compare(Pair a, Pair b)
        {
            //depth has higher prority than bound
            //we want the larger depth value, though
            if (a.Depth != b.Depth)
            {
                return (int)((b.Depth - a.Depth) + 0.5); //swapped order to make deeper depth a negative value
            }
            else
            {
                return (int)((a.Bound - b.Bound) + 0.5);
            }
        }
    }
}
