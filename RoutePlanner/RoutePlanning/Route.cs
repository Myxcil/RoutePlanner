//----------------------------------------------------------------------------------------------------------------------------------------
namespace FlightPlanner.RoutePlanning
{
    //------------------------------------------------------------------------------------------------------------------------------------
    class Route
    {
        //--------------------------------------------------------------------------------------------------------------------------------
        public readonly string from;
        public readonly string to;
        public readonly int cargo;
        public readonly int passenger;

        //--------------------------------------------------------------------------------------------------------------------------------
        public Route(string from, string to, int cargo, int passenger) { this.from = from; this.to = to; this.cargo = cargo; this.passenger = passenger; }
    }

    class RouteEx : Route
    {
        public readonly int distance;

        //--------------------------------------------------------------------------------------------------------------------------------
        public RouteEx(string from, string to, int distance, int cargo, int passenger) : base(from,to,cargo,passenger) { this.distance = distance; }

        //--------------------------------------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return string.Format("{0} -> {1} {2,5}nm {3,6}lb {4,4}p", from, to, distance, cargo, passenger);
        }
    }
}
