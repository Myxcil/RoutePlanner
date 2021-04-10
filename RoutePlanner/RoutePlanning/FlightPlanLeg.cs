//----------------------------------------------------------------------------------------------------------------------------------------
namespace FlightPlanner.RoutePlanning
{
    //------------------------------------------------------------------------------------------------------------------------------------
    class FlightPlanLeg
    {
        public readonly AirportData airportData;
        public readonly int cargo;
        public readonly int passenger;

        public FlightPlanLeg(AirportData airportData, int cargo, int passenger)
        {
            this.airportData = airportData;
            this.cargo = cargo;
            this.passenger = passenger;
        }
    }
}
