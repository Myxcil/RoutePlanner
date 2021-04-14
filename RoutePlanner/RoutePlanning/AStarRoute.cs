using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
        private readonly List<AStarNode> openList = new List<AStarNode>();
        private AStarNode targetNode = null;
        private ulong allRoutesFlag = 0;
        private int avgDistance = 0;

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
            if (Verbose) DebugPrintRoutes(routes, airports);

            CreateUniqueAirports(routes, airports);

            openList.Clear();
            targetNode = null;

            avgDistance = DetermineAvgDistance() / CostScale;

            ContinueSearch = true;
            Search(routes);

            List<FlightPlanLeg> flightPlan = new List<FlightPlanLeg>();
            if (targetNode != null)
            {
                AStarNode node = targetNode;
                while(node != null)
                {
                    flightPlan.Add(new FlightPlanLeg(node.airportData, node.loaded));
                    node = node.parent;
                }
                flightPlan.Reverse();

                int cargo = 0;
                int passenger = 0;
                for (int i = 0; i < flightPlan.Count; ++i)
                {
                    EvaluateCargoChange(flightPlan[i].airportData, flightPlan[i].loaded, routes, ref cargo, ref passenger);
                    flightPlan[i].cargo = cargo;
                    flightPlan[i].passenger = passenger;
                }
                                
                if (Verbose) DebugPrintFlightPlan(flightPlan);
            }
            return flightPlan;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private int DetermineAvgDistance()
        {
            string[] keys = uniqueAirports.Keys.ToArray();

            int total = 0;
            int count = 0;
            for (int i = 0; i < keys.Length - 1; ++i)
            {
                AirportData a = uniqueAirports[keys[i]];
                for(int j=i+1; j < keys.Length; ++j)
                {
                    AirportData b = uniqueAirports[keys[j]];
                    total += a.DistanceTo(b);
                    ++count;
                }
            }
            return total / count;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void EvaluateCargoChange(AirportData airportData, ulong loaded, IList<Route> routes, ref int cargo, ref int passenger)
        {
            for(int i=0; i < routes.Count; ++i)
            {
                Route route = routes[i];
                if (route.from == airportData)
                {
                    cargo += route.cargo;
                    passenger += route.passenger;
                }
                else if (route.to == airportData && (loaded & (BIT << i)) != 0)
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
            for (int i=1; i < flightPlan.Count; ++i)
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
        private void DebugPrintRoutes(IList<Route> routes, IDictionary<string, AirportData> airports)
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
                    if (routes[i-1].to != from)
                    {
                        totalDistance += routes[i - 1].to.DistanceTo(from);
                    }
                }
            }
            Debug.WriteLine(string.Format("        total : {0,5}nm", totalDistance));
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void CreateUniqueAirports(IList<Route> routes, IDictionary<string, AirportData> airports)
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
        private void Search(IList<Route> routes)
        {
            if (Verbose) Debug.WriteLine("------------------------------------------------------------");

            AStarNode startNode = new AStarNode(null, routes[0].from);
            EvaluateLoad(startNode.airportData, routes, ref startNode.loaded, ref startNode.unloaded);

            startNode.costFromStart = 0;
            startNode.estimatedCost = EstimateCost(startNode, routes);
            startNode.totalCost = startNode.costFromStart + startNode.estimatedCost;

            openList.Add(startNode);

            while (ContinueSearch)
            {
                AStarNode node = PopFromOpenList();
                if (node == null)
                    break;

                if (Verbose) Debug.WriteLine(node.ToString(routes.Count));

                if (IsPlanValid(node))
                {
                    targetNode = node;
                    break;
                }

                foreach(var kv in uniqueAirports)
                {
                    AirportData airportData = kv.Value;
                    if (node.airportData == airportData)
                        continue;

                    ulong loaded = node.loaded;
                    ulong unloaded = node.unloaded;
                    if (EvaluateLoad(airportData, routes, ref loaded, ref unloaded))
                    {
                        AStarNode nextNode = new AStarNode(node, airportData);

                        nextNode.loaded = loaded;
                        nextNode.unloaded = unloaded;

                        nextNode.costFromStart = CalculateCost(nextNode);
                        nextNode.estimatedCost = EstimateCost(nextNode, routes);
                        nextNode.totalCost = nextNode.costFromStart + nextNode.estimatedCost;
                        
                        openList.Add(nextNode);
                    }
                }

                openList.Sort(CompareNodes);
            }

            if (Verbose) Debug.WriteLine("------------------------------------------------------------");
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private const ulong BIT = 1;

        //--------------------------------------------------------------------------------------------------------------------------------
        private bool EvaluateLoad(AirportData airportData, IList<Route> routes, ref ulong loaded, ref ulong unloaded)
        {
            bool changedLoad = false;

            ulong flag;

            for(int i=0; i < routes.Count; ++i)
            {
                Route route = routes[i];
                flag = BIT << i;
                if ((unloaded & flag) != 0)
                    continue;

                if (route.from == airportData && ((loaded & flag) == 0))
                {
                    loaded |= flag;
                    changedLoad = true;
                }

                if ((loaded & flag) == 0)
                    continue;

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
            return CountOpenRoutes(node, routes) * avgDistance + GetFarthest(node, routes);
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private int GetFarthest(AStarNode node, IList<Route> routes)
        {
            int farthest = 0;
            for (int i = 0; i < routes.Count; ++i)
            {
                Route route = routes[i];
                if (route.from == node.airportData || route.to == node.airportData)
                    continue;

                ulong flag = BIT << i;
                if ((node.loaded & flag) == 0)
                {
                    farthest = Math.Max(farthest, node.airportData.DistanceTo(route.from));
                }
                else if ((node.unloaded & flag) == 0)
                {
                    farthest = Math.Max(farthest, node.airportData.DistanceTo(route.to));
                }
            }
            return farthest;
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
    }
}
