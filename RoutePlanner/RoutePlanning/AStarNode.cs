//------------------------------------------------------------------------------------------------------------------------------------
namespace FlightPlanner.RoutePlanning
{
    //--------------------------------------------------------------------------------------------------------------------------------
    class AStarNode
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        public readonly AStarNode parent;
        public readonly AirportData airportData;

        //-----------------------------------------------------------------------------------------------------------------------------
        public ulong loaded;
        public ulong unloaded;

        //-----------------------------------------------------------------------------------------------------------------------------
        public int costFromStart;
        public int estimatedCost;
        public int TotalCost { get { return costFromStart + estimatedCost; } }

        //-----------------------------------------------------------------------------------------------------------------------------
        public AStarNode(AStarNode parent, AirportData airportData)
        {
            this.parent = parent;
            this.airportData = airportData;
        }

        public override string ToString()
        {
            if (parent != null)
            {
                return string.Format("{0} ({1}) {2} {3} T={4}", airportData.ICAO, parent.airportData.ICAO, ToBinaryString(loaded), ToBinaryString(unloaded), TotalCost);
            }
            else
            {
                return string.Format("{0} (----) {1} {2} T={3}", airportData.ICAO, ToBinaryString(loaded), ToBinaryString(unloaded), TotalCost);
            }
        }

        private static string ToBinaryString(ulong value)
        {
            return System.Convert.ToString((long)value, 2).PadLeft(16, '0');
        }
    }
}