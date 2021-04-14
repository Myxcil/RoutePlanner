//----------------------------------------------------------------------------------------------------------------------------------------
namespace FlightPlanner.RoutePlanning
{
    //------------------------------------------------------------------------------------------------------------------------------------
    class Route
    {
        //--------------------------------------------------------------------------------------------------------------------------------
        public readonly AirportData from;
        public readonly AirportData to;
        public readonly int cargo;
        public readonly int passenger;

        //--------------------------------------------------------------------------------------------------------------------------------
        public Route(AirportData from, AirportData to, int cargo, int passenger) { this.from = from; this.to = to; this.cargo = cargo; this.passenger = passenger; }

        public Route(string from, string to, int cargo, int passenger) { this.from = AirportData.Airports[from]; this.to = AirportData.Airports[to]; this.cargo = cargo; this.passenger = passenger; }

        //--------------------------------------------------------------------------------------------------------------------------------
        public int Distance => from.DistanceTo(to);

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool Equals(AirportData from, AirportData to, int cargo, int passenger)
        {
            return this.from == from && this.to == to && this.cargo == cargo && this.passenger == passenger;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return string.Format("{0} -> {1} {2,5}nm {3,6}lb {4,4}p", from, to, Distance, cargo, passenger);
        }
    }
}
