using System;
using System.Globalization;
using System.Collections.Generic;
using Microsoft.VisualBasic.FileIO;

//----------------------------------------------------------------------------------------------------------------------------------------
namespace FlightPlanner.RoutePlanning
{
    //------------------------------------------------------------------------------------------------------------------------------------
    class AirportData
    {
        //--------------------------------------------------------------------------------------------------------------------------------
        public readonly int airportId;
        public readonly string name;
        public readonly string city;
        public readonly string country;
        public readonly string IATA;
        public readonly string ICAO;
        public readonly double latitude;
        public readonly double longitude;
        public readonly int altitude;
        public readonly float timeZone;
        public readonly char daylightSavings;
        public readonly string tzOlson;
        public readonly string type;

        //--------------------------------------------------------------------------------------------------------------------------------
        private static readonly Dictionary<string, AirportData> airports = new Dictionary<string, AirportData>();
        public static IDictionary<string, AirportData> Airports { get { return airports; } }

        //--------------------------------------------------------------------------------------------------------------------------------
        static AirportData()
        {
            airports.Clear();
            using (TextFieldParser textFieldParser = new TextFieldParser("airports.dat"))
            {
                textFieldParser.SetDelimiters(",");
                while (!textFieldParser.EndOfData)
                {
                    string[] elements = textFieldParser.ReadFields();
                    if (elements != null)
                    {
                        AirportData airportData = new AirportData(elements);
                        airports.Add(airportData.ICAO, airportData);
                    }
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private static readonly CultureInfo cultureInfo = new CultureInfo("en-US");

        //--------------------------------------------------------------------------------------------------------------------------------
        private AirportData(string[] elements)
        {
            airportId = int.Parse(elements[0]);
            name = elements[1];
            city = elements[2];
            country = elements[3];
            IATA = elements[4];
            ICAO = elements[5];
            latitude = double.Parse(elements[6], cultureInfo.NumberFormat);
            longitude = double.Parse(elements[7], cultureInfo.NumberFormat);
            altitude = int.Parse(elements[8]);
            float.TryParse(elements[9], out timeZone);
            daylightSavings = elements[10][0];
            tzOlson = elements[11];
            type = elements[12];
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return ICAO;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public int DistanceTo(AirportData other)
        {
            var d1 = latitude * (Math.PI / 180.0);
            var num1 = longitude * (Math.PI / 180.0);
            var d2 = other.latitude * (Math.PI / 180.0);
            var num2 = other.longitude * (Math.PI / 180.0) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);
            var dist = 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
            return (int)Math.Round(dist * 0.000539957);
        }
    }
}