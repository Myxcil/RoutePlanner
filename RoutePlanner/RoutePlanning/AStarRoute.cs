using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

//----------------------------------------------------------------------------------------------------------------------------------------
namespace FlightPlanner.RoutePlanning
{
    //------------------------------------------------------------------------------------------------------------------------------------
    class AStarRoute : IDisposable
    {
        //--------------------------------------------------------------------------------------------------------------------------------
        public static bool Verbose { set; get; }

        //--------------------------------------------------------------------------------------------------------------------------------
        private readonly Dictionary<string, AirportData> uniqueAirports = new Dictionary<string, AirportData>();

        //--------------------------------------------------------------------------------------------------------------------------------
        private readonly List<AStarNode> openList = new List<AStarNode>();
        private readonly object listLock = new object();
        public int NumOpen { get { return openList.Count; } }
        public float RoutesComplete { get; private set; }

        //--------------------------------------------------------------------------------------------------------------------------------
        private IList<Route> routes;
        private AStarNode targetNode = null;
        private ulong allRoutesFlag = 0;

        //--------------------------------------------------------------------------------------------------------------------------------
        private Thread[] workerThreads;

        class ThreadData
        {
            public AirportData airportData;
            public AStarNode node;
            public bool isDone;
        }
        private ThreadData[] threadData;
        private object[] lockThreadData;

        //--------------------------------------------------------------------------------------------------------------------------------
        public int CostScale { set; get; } = 1;
        public static bool ContinueSearch { set; get; } = true;

        //------------------------------------------------------------------------------------------------------------------------------------------
        public void Dispose()
        {
            uniqueAirports.Clear();
            openList.Clear();
            targetNode = null;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public IList<FlightPlanLeg> CalculateRoute(IList<Route> routes, IDictionary<string, AirportData> airports)
        {
            this.routes = routes;

            if (Verbose) DebugPrintRoutes();

            CreateUniqueAirports(airports);

            openList.Clear();
            targetNode = null;

            ContinueSearch = true;
            RoutesComplete = 0;

            // create start node
            AStarNode startNode = new AStarNode(null, routes[0].from);
            EvaluateLoad(startNode.airportData, routes, ref startNode.loaded, ref startNode.unloaded);

            startNode.costFromStart = 0;
            startNode.estimatedCost = EstimateCost(startNode, routes);
            startNode.totalCost = startNode.costFromStart + startNode.estimatedCost;

            openList.Add(startNode);

            if (Verbose) Debug.WriteLine("------------------------------------------------------------");

            Search();

            return CreateFlightPlan(routes);
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void CreateUniqueAirports(IDictionary<string, AirportData> airports)
        {
            uniqueAirports.Clear();
            for (int i = 0; i < routes.Count; ++i)
            {
                Route route = routes[i];
                if (!uniqueAirports.ContainsKey(route.from.ICAO))
                {
                    uniqueAirports.Add(route.from.ICAO, airports[route.from.ICAO]);
                }
                if (!uniqueAirports.ContainsKey(route.to.ICAO))
                {
                    uniqueAirports.Add(routes[i].to.ICAO, airports[route.to.ICAO]);
                }
            }

            allRoutesFlag = 0;
            for(int i=0; i < routes.Count; ++i)
            {
                allRoutesFlag |= BIT << i;
            }

            if (Verbose)
            {
                Debug.WriteLine(string.Format("Unique airports: {0}", uniqueAirports.Count));
                foreach (var kv in uniqueAirports)
                {
                    AirportData airportData = kv.Value;
                    Debug.WriteLine(string.Format(" {0} {1}", airportData.ICAO, airportData.name));
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void Search()
        {
            workerThreads = new Thread[uniqueAirports.Count - 1];
            threadData = new ThreadData[workerThreads.Length];
            lockThreadData = new object[workerThreads.Length];

            for (int i=0; i < workerThreads.Length; ++i)
            {
                workerThreads[i] = new Thread(EvaluateAirportThread);
                threadData[i] = new ThreadData();
                lockThreadData[i] = new object();

                workerThreads[i].Start(threadData[i]);
            }

            while (ContinueSearch)
            {
                AStarNode node = PopFromOpenList();
                if (node == null)
                    break;

                RoutesComplete = 100.0f * CountCompleteRoutes(node) / routes.Count;

                if (Verbose) Debug.WriteLine(node.ToString(routes.Count));

                if (IsPlanValid(node))
                {
                    targetNode = node;
                    break;
                }

                int index = 0;
                foreach (var kv in uniqueAirports)
                {
                    AirportData airportData = kv.Value;
                    if (node.airportData == airportData)
                        continue;

                    lock (lockThreadData[index])
                    {
                        threadData[index].airportData = airportData;
                        threadData[index].node = node;
                    }
                    ++index;
                }

                bool isDone = false;
                while(!isDone)
                {
                    isDone = true;
                    for(int i=0; i < threadData.Length; ++i)
                    {
                        if (!threadData[i].isDone)
                        {
                            isDone = false;
                            break;
                        }
                    }
                    if (!isDone)
                    {
                        Thread.Sleep(0);
                    }
                }
                
                openList.Sort(CompareNodes);

                for (int i = 0; i < threadData.Length; ++i)
                {
                    lock (lockThreadData[i])
                    {
                        threadData[i].airportData = null;
                        threadData[i].node = null;
                        threadData[i].isDone = false;
                    }
                }

                Thread.Sleep(0);
            }

            ContinueSearch = false;

            if (Verbose) Debug.WriteLine("------------------------------------------------------------");
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private float CountCompleteRoutes(AStarNode node)
        {
            float numCompleted = 0;
            for(int i=0; i < routes.Count; ++i)
            {
                ulong flag = BIT << i;
                if ((node.loaded & flag) != 0)
                {
                    numCompleted += 0.5f;
                }
                if ((node.unloaded & flag) != 0)
                {
                    numCompleted += 0.5f;
                }
            }
            return numCompleted;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void EvaluateAirportThread(object obj)
        {
            ThreadData workData = (ThreadData)obj;

            while (ContinueSearch)
            {
                while(workData.airportData == null && ContinueSearch)
                {
                    Thread.Sleep(0);
                }

                EvaluateAirport(workData.airportData, workData.node);
                
                workData.isDone = true;

                while (workData.isDone && ContinueSearch)
                {
                    Thread.Sleep(0);
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void EvaluateAirport(AirportData airportData, AStarNode node)
        {
            if (airportData == null || node == null)
                return;

            ulong loaded = node.loaded;
            ulong unloaded = node.unloaded;
            if (EvaluateLoad(airportData, routes, ref loaded, ref unloaded))
            {
                AStarNode nextNode = new AStarNode(node, airportData)
                {
                    loaded = loaded,
                    unloaded = unloaded
                };

                nextNode.costFromStart = CalculateCost(nextNode);
                nextNode.estimatedCost = EstimateCost(nextNode, routes);
                nextNode.totalCost = nextNode.costFromStart + nextNode.estimatedCost;

                lock (listLock)
                {
                    openList.Add(nextNode);
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private const ulong BIT = 1;

        //--------------------------------------------------------------------------------------------------------------------------------
        // Check if this airport will do anything useful, e.g. loading or unloading cargo/passengers for a route
        //--------------------------------------------------------------------------------------------------------------------------------
        private bool EvaluateLoad(AirportData airportData, IList<Route> routes, ref ulong loaded, ref ulong unloaded)
        {
            bool changedLoad = false;

            for(int i=0; i < routes.Count; ++i)
            {
                Route route = routes[i];
                ulong flag = BIT << i;

                // already 'done' this route (unloaded cargo) ?
                if ((unloaded & flag) != 0)
                    continue;

                // load cargo/passengers here?
                if (route.from == airportData && ((loaded & flag) == 0))
                {
                    loaded |= flag;
                    changedLoad = true;
                }

                // nothing loaded for this route, no need to test it any further
                if ((loaded & flag) == 0)
                    continue;

                // unload cargo here?
                if (route.to == airportData && ((unloaded & flag) == 0))
                {
                    unloaded |= flag;
                    changedLoad = true;
                }
            }

            return changedLoad;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private int CalculateCost(AStarNode node)
        {
            Debug.Assert(node.parent != null);
            return node.parent.costFromStart + node.parent.airportData.DistanceTo(node.airportData);
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private bool IsPlanValid(AStarNode node)
        {
            return node.loaded == allRoutesFlag && node.unloaded == allRoutesFlag;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private int EstimateCost(AStarNode node, IList<Route> routes)
        {
            return CountOpenRoutes(node, routes) * CostScale;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private int CountOpenRoutes(AStarNode node, IList<Route> routes)
        {
            int numOpenRoutes = 0;
            for (int i = 0; i < routes.Count; ++i)
            {
                ulong flag = BIT << i;
                if ((node.loaded & flag) == 0)
                {
                    ++numOpenRoutes;
                }
                if ((node.unloaded & flag) == 0)
                {
                    ++numOpenRoutes;
                }
            }
            return numOpenRoutes;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private AStarNode PopFromOpenList()
        {
            AStarNode node = null;
            if (openList.Count > 0)
            {
                int last = openList.Count - 1;
                node = openList[last];
                openList.RemoveAt(last);
            }
            return node;
        }

        //------------------------------------------------------------------------------------------------------------------------------------------
        private static int CompareNodes(AStarNode a, AStarNode b)
        {
            int diff = b.totalCost - a.totalCost;
            return diff;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private IList<FlightPlanLeg> CreateFlightPlan(IList<Route> routes)
        {
            List<FlightPlanLeg> flightPlan = new List<FlightPlanLeg>();
            if (targetNode != null)
            {
                AStarNode node = targetNode;
                while (node != null)
                {
                    flightPlan.Add(new FlightPlanLeg(node.airportData, node.loaded));
                    node = node.parent;
                }
                flightPlan.Reverse();

                int cargo = 0;
                int passenger = 0;
                ulong loaded = 0;
                for (int i = 0; i < flightPlan.Count; ++i)
                {
                    EvaluateCargoChange(flightPlan[i].airportData, routes, ref loaded, ref cargo, ref passenger);
                    flightPlan[i].cargo = cargo;
                    flightPlan[i].passenger = passenger;
                }
                if (Verbose) DebugPrintFlightPlan(flightPlan);
            }
            return flightPlan;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void EvaluateCargoChange(AirportData airportData, IList<Route> routes, ref ulong loaded, ref int cargo, ref int passenger)
        {
            for (int i = 0; i < routes.Count; ++i)
            {
                Route route = routes[i];
                bool isLoaded = (loaded & (BIT << i)) != 0;
                if (!isLoaded && route.from == airportData)
                {
                    cargo += route.cargo;
                    passenger += route.passenger;
                    loaded |= BIT << i;
                }
                else if (isLoaded && route.to == airportData)
                {
                    cargo -= route.cargo;
                    passenger -= route.passenger;
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void DebugPrintFlightPlan(IList<FlightPlanLeg> flightPlan)
        {
            Debug.WriteLine(string.Format("Legs: {0}", flightPlan.Count - 1));

            int totalDistance = 0;
            for (int i = 1; i < flightPlan.Count; ++i)
            {
                FlightPlanLeg parent = flightPlan[i - 1];
                AirportData from = flightPlan[i - 1].airportData;
                AirportData to = flightPlan[i].airportData;
                int distance = from.DistanceTo(to);
                Debug.WriteLine(string.Format(" {0} -> {1} : {2,5}nm {3,6}lb {4,3}pax", from.ICAO, to.ICAO, distance, parent.cargo, parent.passenger));
                totalDistance += distance;
            }
            Debug.WriteLine(string.Format("        total : {0}nm", totalDistance));
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void DebugPrintRoutes()
        {
            Debug.WriteLine(string.Format("Routes: {0}", routes.Count));
            int totalDistance = 0;
            for (int i = 0; i < routes.Count; ++i)
            {
                Route route = routes[i];
                AirportData from = route.from;
                AirportData to = route.to;
                int distance = route.Distance;
                Debug.WriteLine(string.Format(" {0} -> {1} : {2,5}nm {3,6}lb {4,3}pax", from.ICAO, to.ICAO, distance, route.cargo, route.passenger));
                totalDistance += distance;
                if (i > 0)
                {
                    if (routes[i - 1].to != from)
                    {
                        totalDistance += routes[i - 1].to.DistanceTo(from);
                    }
                }
            }
            Debug.WriteLine(string.Format("        total : {0,5}nm", totalDistance));
        }
    }
}
