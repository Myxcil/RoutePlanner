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
        public readonly string ident;
        public readonly string type;
        public readonly string name;
        public readonly int elevation;
        public readonly string continent;
        public readonly string country;
        public readonly string region;
        public readonly string municipality;
        public readonly string gps;
        public readonly string iata;
        public readonly string local;
        public readonly double longitude;
        public readonly double latitude;

        //--------------------------------------------------------------------------------------------------------------------------------
        private static readonly Dictionary<string, AirportData> airports = new Dictionary<string, AirportData>();
        public static IDictionary<string, AirportData> Airports { get { return airports; } }

        //--------------------------------------------------------------------------------------------------------------------------------
        private static readonly string airportData = "airport-codes_csv.csv";

        //--------------------------------------------------------------------------------------------------------------------------------
        static AirportData()
        {
            airports.Clear();
            using (TextFieldParser textFieldParser = new TextFieldParser(airportData))
            {
                textFieldParser.SetDelimiters(",");
                while (!textFieldParser.EndOfData)
                {
                    string[] elements = textFieldParser.ReadFields();
                    if (elements != null && elements.Length == 12 && elements[0].Length <= 4)
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
            ident = elements[0];
            type = elements[1];
            name = elements[2];
            int.TryParse(elements[3], out elevation);
            continent = elements[4];
            country = elements[5];
            region = elements[6];
            municipality = elements[7];
            gps = elements[8];
            iata = elements[9];
            local = elements[10];

            string[] coords = elements[11].Split(',');
            latitude = double.Parse(coords[0], cultureInfo.NumberFormat);
            longitude = double.Parse(coords[1], cultureInfo.NumberFormat);
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return ident;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public string ICAO => ident;

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