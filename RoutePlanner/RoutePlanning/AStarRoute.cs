using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------------------------------------------------------------------
namespace FlightPlanner.RoutePlanning
{
    //------------------------------------------------------------------------------------------------------------------------------------
    class AStarRoute
    {
        //--------------------------------------------------------------------------------------------------------------------------------
        private class Node
        {
            public readonly Node parent;
            public readonly AirportData airportData;
            
            public int routesCovered;
            public int cargoLoad;
            public int passengerLoad;

            public int costFromStart;
            public int estimatedCost;
            public int TotalCost { get { return costFromStart + estimatedCost; } }


            public Node(Node parent, AirportData airportData)
            {
                this.parent = parent;
                this.airportData = airportData;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private readonly Dictionary<string, AirportData> uniqueAirports = new Dictionary<string, AirportData>();
        private readonly List<Node> openList = new List<Node>();

        private Node targetNode = null;

        //--------------------------------------------------------------------------------------------------------------------------------
        public IList<FlightPlanLeg> CalculateRoute(IList<Route> routes, IDictionary<string, AirportData> airports)
        {
            DebugPrintRoutes(routes, airports);

            CreateUniqueAirpots(routes, airports);

            openList.Clear();

            Node startNode = new Node(null, airports[routes[0].from]);

            startNode.routesCovered = 0;
            CalculateCargo(startNode.airportData, routes, out startNode.cargoLoad, out startNode.passengerLoad);

            startNode.costFromStart = 0;
            startNode.estimatedCost = CountOpenRoutes(startNode, routes);

            openList.Add(startNode);

            targetNode = null;

            Search(routes);

            List<FlightPlanLeg> flightPlan = new List<FlightPlanLeg>();
            if (targetNode != null)
            {
                Node node = targetNode;
                while(node != null)
                {
                    flightPlan.Add(new FlightPlanLeg(node.airportData, node.cargoLoad, node.passengerLoad));
                    node = node.parent;
                }
                flightPlan.Reverse();
                DebugPrintFlightPlan(flightPlan);
            }
            return flightPlan;
        }

        //--------------------------------------------------------------------------------------------------------------------------------

        private void DebugPrintFlightPlan(IList<FlightPlanLeg> flightPlan)
        {
            Console.WriteLine("Legs: {0}", flightPlan.Count - 1);

            int totalDistance = 0;
            for (int i=1; i < flightPlan.Count; ++i)
            {
                FlightPlanLeg parent = flightPlan[i - 1];
                AirportData from = flightPlan[i - 1].airportData;
                AirportData to = flightPlan[i].airportData;
                int distance = from.DistanceTo(to);
                Console.WriteLine(" {0} -> {1} : {2,5}nm {3,6}lb {4,3}pax", from.ICAO, to.ICAO, distance, parent.cargo, parent.passenger);
                totalDistance += distance;
            }
            Console.WriteLine("        total : {0}nm", totalDistance);
            Console.WriteLine();
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void DebugPrintRoutes(IList<Route> routes, IDictionary<string, AirportData> airports)
        {
            Console.WriteLine("Routes: {0}", routes.Count);
            int totalDistance = 0;
            for (int i = 0; i < routes.Count; ++i)
            {
                Route route = routes[i];
                AirportData from = airports[route.from];
                AirportData to = airports[route.to];
                int distance = from.DistanceTo(to);
                Console.WriteLine(" {0} -> {1} : {2,5}nm {3,6}lb {4,3}pax", from.ICAO, to.ICAO, distance, route.cargo, route.passenger);
                totalDistance += distance;
            }
            Console.WriteLine("        total : {0,5}nm", totalDistance);
            Console.WriteLine();
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void CreateUniqueAirpots(IList<Route> routes, IDictionary<string, AirportData> airports)
        {
            uniqueAirports.Clear();
            for (int i = 0; i < routes.Count; ++i)
            {
                Route route = routes[i];
                if (!uniqueAirports.ContainsKey(route.from))
                {
                    uniqueAirports.Add(route.from, airports[route.from]);
                }
                if (!uniqueAirports.ContainsKey(route.to))
                {
                    uniqueAirports.Add(routes[i].to, airports[route.to]);
                }
            }

            Console.WriteLine("Unique airports: {0}", uniqueAirports.Count);
            foreach (var kv in uniqueAirports)
            {
                AirportData airportData = kv.Value;
                Console.WriteLine(" {0} {1}", airportData.ICAO, airportData.name);
            }
            Console.WriteLine();
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void Search(IList<Route> routes)
        {
            for(; ;)
            {
                Node node = PopFromOpenList();
                if (node == null)
                    break;

                if (IsPlanValid(node, routes))
                {
                    targetNode = node;
                    break;
                }

                foreach(var kv in uniqueAirports)
                {
                    AirportData airportData = kv.Value;
                    if (node.airportData == airportData)
                        continue;

                    Node nextNode = new Node(node, airportData);

                    int numCovered = CountCovertedRoutes(nextNode, routes);
                    if (numCovered > node.routesCovered)
                    {
                        CalculateCargo(nextNode.airportData, routes, out nextNode.cargoLoad, out nextNode.passengerLoad);
                        nextNode.cargoLoad += node.cargoLoad;
                        nextNode.passengerLoad += node.passengerLoad;

                        nextNode.routesCovered = numCovered;
                        
                        nextNode.costFromStart = CalculateCost(nextNode);
                        nextNode.estimatedCost = CountOpenRoutes(nextNode, routes);
                        
                        openList.Add(nextNode);
                    }
                }

                openList.Sort(CompareNodes);
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private int CalculateCost(Node node)
        {
            if (node.parent != null)
            {
                return node.parent.costFromStart + node.parent.airportData.DistanceTo(node.airportData);
            }
            else
            {
                return 1;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private bool IsPlanValid(Node node, IList<Route> routes)
        {
            for (int i = 0; i < routes.Count; ++i)
            {
                Route route = routes[i];
                AirportData from = uniqueAirports[route.from];
                AirportData to = uniqueAirports[route.to];
                if (!IsRouteCovered(node, from, to))
                {
                    return false;
                }
            }
            return true;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private int CountCovertedRoutes(Node node, IList<Route> routes)
        {
            return routes.Count - CountOpenRoutes(node, routes);
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private int CountOpenRoutes(Node node, IList<Route> routes)
        {
            int numOpenRoutes = 0;
            for (int i = 0; i < routes.Count; ++i)
            {
                Route route = routes[i];
                AirportData from = uniqueAirports[route.from];
                AirportData to = uniqueAirports[route.to];
                if (!IsRouteCovered(node, from, to))
                {
                    ++numOpenRoutes;
                }
            }
            return numOpenRoutes;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private bool IsRouteCovered(Node node, AirportData from, AirportData to)
        {
            bool hasFrom = false;
            bool hasTo = false;

            while (node != null)
            {
                if (!hasTo && node.airportData == to)
                {
                    hasTo = true;
                }
                else if (!hasFrom && node.airportData == from)
                {
                    hasFrom = true;
                    break;
                }
                node = node.parent;
            }

            return hasFrom && hasTo;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void CalculateCargo(AirportData airportData, IList<Route> routes, out int cargo, out int passenger)
        {
            cargo = 0;
            passenger = 0;
            for(int i=0; i < routes.Count; ++i)
            {
                Route route = routes[i];
                AirportData from = uniqueAirports[route.from];
                AirportData to = uniqueAirports[route.to];
                if (from == airportData)
                {
                    cargo += route.cargo;
                    passenger += route.passenger;
                }
                else if (to == airportData)
                {
                    cargo -= route.cargo;
                    passenger -= route.passenger;
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private Node PopFromOpenList()
        {
            Node node = null;
            if (openList.Count > 0)
            {
                int last = openList.Count - 1;
                node = openList[last];
                openList.RemoveAt(last);
            }
            return node;
        }

        //------------------------------------------------------------------------------------------------------------------------------------------
        private static int CompareNodes(Node a, Node b)
        {
            int diff = b.TotalCost - a.TotalCost;
            return diff;
        }
    }
}
