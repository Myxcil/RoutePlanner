//----------------------------------------------------------------------------------------------------------------------------------------
namespace FlightPlanner.RoutePlanning
{
    //------------------------------------------------------------------------------------------------------------------------------------
    class FlightPlanLeg
    {
        //--------------------------------------------------------------------------------------------------------------------------------
        public readonly AirportData airportData;
        public readonly ulong loaded;
        public int cargo;
        public int passenger;

        //--------------------------------------------------------------------------------------------------------------------------------
        public FlightPlanLeg(AirportData airportData, ulong loaded)
        {
            this.airportData = airportData;
            this.loaded = loaded;
        }
    }
}
