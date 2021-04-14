//------------------------------------------------------------------------------------------------------------------------------------
namespace FlightPlanner.RoutePlanning
{
    //--------------------------------------------------------------------------------------------------------------------------------
    class AStarNode
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        public int totalCost;

        //-----------------------------------------------------------------------------------------------------------------------------
        public readonly AStarNode parent;
        public readonly AirportData airportData;

        //-----------------------------------------------------------------------------------------------------------------------------
        public ulong loaded;
        public ulong unloaded;

        //-----------------------------------------------------------------------------------------------------------------------------
        public int costFromStart;
        public int estimatedCost;

        //-----------------------------------------------------------------------------------------------------------------------------
        public AStarNode(AStarNode parent, AirportData airportData)
        {
            this.parent = parent;
            this.airportData = airportData;
        }

        public string ToString(int numRoutes)
        {
            int done = CountMatches(loaded, unloaded, numRoutes);
            if (parent != null)
            {
                return string.Format("{0} ({1}) {2} {3} {4}/{5} T={6}", airportData.ICAO, parent.airportData.ICAO, ToBinaryString(loaded, numRoutes), ToBinaryString(unloaded, numRoutes), done, numRoutes, totalCost);
            }
            else
            {
                return string.Format("{0} (----) {1} {2} {3}/{4} T={5}", airportData.ICAO, ToBinaryString(loaded, numRoutes), ToBinaryString(unloaded, numRoutes), done, numRoutes, totalCost);
            }
        }

        private static string ToBinaryString(ulong value, int numRoutes)
        {
            return System.Convert.ToString((long)value, 2).PadLeft(numRoutes, '0');
        }

        private static int CountMatches(ulong a, ulong b, int numBits)
        {
            int numMatches = 0;
            for(int i=0; i < numBits; ++i)
            {
                ulong flag = (ulong)(1 << i);
                if ((a & flag) != 0 && (b & flag) != 0)
                {
                    ++numMatches;
                }
            }
            return numMatches;
        }
    }
}