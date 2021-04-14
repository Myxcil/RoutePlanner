using Microsoft.VisualStudio.TestTools.UnitTesting;
using FlightPlanner.RoutePlanning;
using System.Collections.Generic;
using System.Diagnostics;

namespace RoutePlannerTest
{
    [TestClass]
    public class UnitTest
    {
        private static readonly Route[] testRouteShort =
        {
            new Route("MYNN", "KMIA", 368, 0),
            new Route("KMIA", "MYCB", 282, 0),
        };

        private static readonly Route[] testRouteOverlaps =
        {
            new Route("MYNN", "KMIA", 368, 0),
            new Route("KMIA", "MYCB", 282, 0),
            new Route("MYCB", "KHST", 204, 0),
            new Route("KHST", "MYAN", 427, 0),
            new Route("MYAN", "MYGF", 515, 0),
            new Route("MYNN", "MYAM", 19, 0),
            new Route("MYAM", "KFLL", 104, 0),
            new Route("KFLL", "MYAT", 479, 0),
            new Route("MYAT", "KMIA", 278, 0),
            new Route("KMIA", "KOPF", 0, 2),
        };
        
        private static readonly Route[] testRouteNoOverlaps =
        {
            new Route("EDDM", "LOWI", 0, 3),
            new Route("LOWI", "EGNT", 146, 0),
            new Route("EGNT", "EDDF", 487, 0),
            new Route("EDDF", "LFML", 0, 2),
            new Route("LFML", "EHAM", 344, 0),
            new Route("EHAM", "LFPG", 184, 0),
            new Route("LFPG", "EHBK", 194, 0),
            new Route("EHBK", "LIML", 567, 0),
            
            new Route("EGMC", "EGGP", 500, 0),
            new Route("EGGP", "EFTP", 0, 3),
            new Route("EFTP", "LMML", 249, 0),
            new Route("LMML", "LERT", 415, 0),
            new Route("LERT", "EDLV", 443, 0),
            new Route("EDLV", "LEVT", 410, 0),
            new Route("LEVT", "EPKT", 485, 0),
            new Route("EPKT", "LIRP", 0, 1),
        };

        [TestMethod]
        public void TestMethod1()
        {
            IList<Route> routes = testRouteShort;

            AStarRoute search = new AStarRoute();
            IList<FlightPlanLeg> legs = search.CalculateRoute(routes, AirportData.Airports);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            int totalDistance = 0;
            for (int i = 1; i < legs.Count; ++i)
            {
                FlightPlanLeg prev = legs[i - 1];
                FlightPlanLeg curr = legs[i];
                int distance = prev.airportData.DistanceTo(curr.airportData);
                sb.AppendFormat("{0} -> {1} {2,5}nm {3,6}lb {4,4}p", prev.airportData.ICAO, curr.airportData.ICAO, distance, prev.cargo, prev.passenger);
                sb.AppendLine();
                totalDistance += distance;
            }

            Debug.WriteLine(sb.ToString());
            Debug.WriteLine("Total: " + totalDistance);
        }
    }
}
