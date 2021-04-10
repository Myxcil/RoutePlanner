using System.Windows.Forms;
using FlightPlanner.RoutePlanning;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace RoutePlanner
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private IDictionary<string, AirportData> airports;
        private string[] airportIDs;

        private void MainForm_Load(object sender, System.EventArgs e)
        {
            airports = AirportData.Parse("airports.dat");
            airportIDs = airports.Keys.ToArray();

            comboBoxFrom.Items.AddRange(airportIDs);
            comboBoxTo.Items.AddRange(airportIDs);

            RefreshTotalDistance();

            ValidateInputs();

//            CreateTestData();
        }

        private void btnNewRoute_Click(object sender, System.EventArgs e)
        {
            checkedListBoxRoutes.Items.Clear();
            comboBoxFrom.SelectedIndex = -1;
            comboBoxTo.SelectedIndex = -1;

            numericUpDownCargo.Value = 0;
            numericUpDownPassenger.Value = 0;

            ValidateInputs();
        }

        private static readonly Route[] testRoutes =
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

        private void CreateTestData()
        {
            for(int i=0; i < testRoutes.Length; ++i)
            {
                Route route = testRoutes[i];
                AirportData from = airports[route.from];
                AirportData to = airports[route.to];
                AddNewRoute(from, to, route.cargo, route.passenger);
            }
        }

        private void buttonAdd_Click(object sender, System.EventArgs e)
        {
            AirportData from = airports[comboBoxFrom.Text];
            AirportData to = airports[comboBoxTo.Text];

            comboBoxFrom.Focus();

        }

        private void AddNewRoute(AirportData from, AirportData to, int cargo, int passenger)
        {
            RouteEx newRoute = new RouteEx(from.ICAO, to.ICAO, from.DistanceTo(to), cargo, passenger);
            checkedListBoxRoutes.Items.Add(newRoute, true);

            RefreshTotalDistance();

            ValidateInputs();
        }

        private void comboBoxFrom_Leave(object sender, System.EventArgs e)
        {
            AutoselectAirport(comboBoxFrom);
        }

        private void comboBoxTo_Leave(object sender, System.EventArgs e)
        {
            AutoselectAirport(comboBoxTo);
        }

        private void AutoselectAirport(ComboBox comboBox)
        {
            comboBox.SelectedIndex = System.Array.IndexOf(airportIDs, comboBox.Text.ToUpper());
            ValidateInputs();
        }

        private void ValidateInputs()
        {
            bool isValid = true;

            if (!ValidateAirport(comboBoxFrom, out AirportData from))
                isValid = false;

            if (!ValidateAirport(comboBoxTo, out AirportData to))
                isValid = false;

            if (from == to)
                isValid = false;

            if (isValid)
            {
                for (int i = 0; i < checkedListBoxRoutes.Items.Count; ++i)
                {
                    RouteEx route = (RouteEx)checkedListBoxRoutes.Items[i];
                    if (route.from == from.ICAO || route.to == to.ICAO)
                    {
                        isValid = false;
                        break;
                    }
                }
            }

            buttonAdd.Enabled = isValid;
            buttonSearch.Enabled = checkedListBoxRoutes.Items.Count > 1;
        }

        private bool ValidateAirport(ComboBox comboBox, out AirportData airportData)
        {
            airportData = null;

            if (comboBox.SelectedItem == null)
                return false;

            if (!airports.TryGetValue(comboBox.Text, out airportData))
                return false;

            return true;
        }

        private void RefreshTotalDistance()
        {
            int totalDistance = 0;
            for (int i = 0; i < checkedListBoxRoutes.Items.Count; ++i)
            {
                RouteEx route = (RouteEx)checkedListBoxRoutes.Items[i];
                totalDistance += route.distance;
            }
            labelTotalDistance.Text = string.Format("Total: {0}nm", totalDistance);
        }

        private void buttonSearch_Click(object sender, System.EventArgs e)
        {
            listBoxSearch.Items.Clear();

            List<Route> routes = new List<Route>();
            for (int i = 0; i < checkedListBoxRoutes.CheckedItems.Count; ++i)
            {
                RouteEx route = (RouteEx)checkedListBoxRoutes.CheckedItems[i];
                routes.Add(route);
            }

            AStarRoute search = new AStarRoute();
            IList<FlightPlanLeg> legs = search.CalculateRoute(routes, airports);

            int totalDistance = 0;
            for (int i = 1; i < legs.Count; ++i)
            {
                FlightPlanLeg prev = legs[i - 1];
                FlightPlanLeg curr = legs[i];
                int distance = prev.airportData.DistanceTo(curr.airportData);
                string text = string.Format("{0} -> {1} {2,5}nm {3,6}lb {4,4}p", prev.airportData.ICAO, curr.airportData.ICAO, distance, prev.cargo, prev.passenger);
                listBoxSearch.Items.Add(text);
                totalDistance += distance;
            }
            labelTotalSearch.Text = string.Format("Total: {0}nm", totalDistance);
        }

        private static readonly System.Globalization.CultureInfo cultureInfo = new System.Globalization.CultureInfo("en-US");

        private void buttonPasteOnAir_Click(object sender, System.EventArgs e)
        {
            string clipText = Clipboard.GetText();
            if (string.IsNullOrEmpty(clipText))
                return;

            string[] elements = clipText.Split('\t');
            if (!airports.TryGetValue(elements[0], out AirportData from))
                return;

            if (!airports.TryGetValue(elements[1], out AirportData to))
                return;

            int cargo = 0;
            int passenger = 0;
            if (!string.IsNullOrEmpty(elements[4]))
            {
                string tmp = new string(elements[4].Where(char.IsDigit).ToArray());
                if (!int.TryParse(tmp, out cargo))
                    return;
            }

            if (!string.IsNullOrEmpty(elements[5]))
            {
                string tmp = new string(elements[5].Where(char.IsDigit).ToArray());
                if (!int.TryParse(tmp, out passenger))
                    return;
            }

            AddNewRoute(from, to, cargo, passenger);
        }
    }
}
