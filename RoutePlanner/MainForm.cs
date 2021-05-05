using System;
using System.Text;
using System.Windows.Forms;
using FlightPlanner.RoutePlanning;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;

//----------------------------------------------------------------------------------------------------------------------------------------
namespace RoutePlanner
{
    //------------------------------------------------------------------------------------------------------------------------------------
    public partial class MainForm : Form
    {
        //--------------------------------------------------------------------------------------------------------------------------------
        public MainForm()
        {
            InitializeComponent();
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private string[] airportIDs;

        //--------------------------------------------------------------------------------------------------------------------------------
        private StringBuilder sb = null;
        private int totalDistance = 0;
        private int maxCargo = 0;
        private int maxPassenger = 0;

        //--------------------------------------------------------------------------------------------------------------------------------
        private AStarRoute aStarSearch = null;
        private delegate void OnFinished();
        private Thread threadSearch;
        private System.Windows.Forms.Timer updateTimer;

        //--------------------------------------------------------------------------------------------------------------------------------
        private void MainForm_Load(object sender, EventArgs e)
        {
            airportIDs = AirportData.Airports.Keys.ToArray();

            comboBoxFrom.Items.AddRange(airportIDs);
            comboBoxTo.Items.AddRange(airportIDs);

            RefreshTotalDistance();

            ValidateInputs();

#if DEBUG
            CreateTestData();
#endif
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void btnNewRoute_Click(object sender, System.EventArgs e)
        {
            checkedListBoxRoutes.Items.Clear();
            comboBoxFrom.SelectedIndex = -1;
            comboBoxTo.SelectedIndex = -1;

            numericUpDownCargo.Value = 0;
            numericUpDownPassenger.Value = 0;

            ValidateInputs();
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        /**
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
        /**/
        private static readonly Route[] testRoutes =
        {
            new Route("EDDM", "LOWI", 0, 3),
            new Route("LOWI", "EGNT", 146, 0),
            new Route("EGNT", "EDDF", 487, 0),
            new Route("EDDF", "LFML", 0, 2),
            new Route("LFML", "EHAM", 344, 0),
            new Route("EHAM", "LFPG", 184, 0),
            new Route("LFPG", "EHBK", 194, 0),
            new Route("EHBK", "LIME", 567, 0),
            
            new Route("EGMC", "EGGP", 500, 0),
            new Route("EGGP", "EFTU", 0, 3),
            new Route("EFTU", "LMML", 249, 0),
            new Route("LMML", "LEJR", 415, 0),
            new Route("LEJR", "EDLV", 443, 0),
            new Route("EDLV", "LEXJ", 410, 0),
            new Route("LEXJ", "EPKT", 485, 0),
            new Route("EPKT", "LIRP", 0, 1),
        };
        /**/

        //--------------------------------------------------------------------------------------------------------------------------------
        private void CreateTestData()
        {
            for(int i=0; i < testRoutes.Length; ++i)
            {
                Route route = testRoutes[i];
                AddNewRoute(route.from, route.to, route.cargo, route.passenger);
            }
            AStarRoute.Verbose = true;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void buttonAdd_Click(object sender, System.EventArgs e)
        {
            AirportData from = AirportData.Airports[comboBoxFrom.Text];
            AirportData to = AirportData.Airports[comboBoxTo.Text];

            Route newRoute = new Route(from, to, (int)numericUpDownCargo.Value, (int)numericUpDownPassenger.Value);
            checkedListBoxRoutes.Items.Add(newRoute, true);

            comboBoxFrom.Focus();
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void AddNewRoute(AirportData from, AirportData to, int cargo, int passenger)
        {
            Route newRoute = new Route(from, to, cargo, passenger);
            checkedListBoxRoutes.Items.Add(newRoute, true);

            RefreshTotalDistance();

            ValidateInputs();

            RefreshCostScale();
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void comboBoxFrom_Leave(object sender, System.EventArgs e)
        {
            AutoselectAirport(comboBoxFrom);
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void comboBoxTo_Leave(object sender, System.EventArgs e)
        {
            AutoselectAirport(comboBoxTo);
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void AutoselectAirport(ComboBox comboBox)
        {
            comboBox.SelectedIndex = System.Array.IndexOf(airportIDs, comboBox.Text.ToUpper());
            ValidateInputs();
        }

        //--------------------------------------------------------------------------------------------------------------------------------
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
                    Route route = (Route)checkedListBoxRoutes.Items[i];
                    if (route.from == from || route.to == to)
                    {
                        isValid = false;
                        break;
                    }
                }
            }

            buttonAdd.Enabled = isValid;
            buttonSearch.Enabled = checkedListBoxRoutes.Items.Count > 1;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private bool ValidateAirport(ComboBox comboBox, out AirportData airportData)
        {
            airportData = null;

            if (comboBox.SelectedItem == null)
                return false;

            if (!AirportData.Airports.TryGetValue(comboBox.Text, out airportData))
                return false;

            return true;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void RefreshTotalDistance()
        {
            int totalDistance = 0;
            int additionalDistance = 0;
            for (int i = 0; i < checkedListBoxRoutes.Items.Count; ++i)
            {
                Route route = (Route)checkedListBoxRoutes.Items[i];
                totalDistance += route.Distance;
                if (i > 0)
                {
                    Route prev = (Route)checkedListBoxRoutes.Items[i - 1];
                    if (prev.to != route.from)
                    {
                        additionalDistance += prev.to.DistanceTo(route.from);
                    }
                }
            }
            labelTotalDistance.Text = string.Format("Total: {0}nm ({1}nm)", totalDistance + additionalDistance, totalDistance);
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void buttonSearch_Click(object sender, System.EventArgs e)
        {
            textBoxResult.Clear();

            List<Route> routes = new List<Route>();
            for (int i = 0; i < checkedListBoxRoutes.CheckedItems.Count; ++i)
            {
                Route route = (Route)checkedListBoxRoutes.CheckedItems[i];
                routes.Add(route);
            }

            buttonSearch.Enabled = false;
            buttonStop.Enabled = true;

            progressBarSearch.MarqueeAnimationSpeed = 10;

            threadSearch = new Thread(LaunchRouteSearch);
            threadSearch.Start(routes);

            if (updateTimer == null)
            {
                updateTimer = new System.Windows.Forms.Timer();
                updateTimer.Interval = 100;
                updateTimer.Tick += new EventHandler(RefreshSearchStats);
            }
            updateTimer.Enabled = true;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void buttonStop_Click(object sender, EventArgs e)
        {
            OnSearchDone();
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void RefreshSearchStats(object sender, EventArgs args)
        {
            if (aStarSearch == null)
                return;

            labelSearchStats.Text = string.Format("Open: {0}  Routes: {1:0.0}%", aStarSearch.NumOpen, aStarSearch.RoutesComplete);
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void RefreshCostScale()
        {
            int distance = 0;
            for (int i = 0; i < checkedListBoxRoutes.Items.Count; ++i)
            {
                Route route = (Route)checkedListBoxRoutes.Items[i];
                distance += route.from.DistanceTo(route.to);
            }
            if (checkedListBoxRoutes.Items.Count > 0)
            {
                distance /= checkedListBoxRoutes.Items.Count;
            }
            numericUpDownScale.Value = distance / 2;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void LaunchRouteSearch(object data)
        {
            Debug.WriteLine("launched search thread");
            long startTicks = DateTime.Now.Ticks;

            IList<Route> routes = (IList<Route>)data;

            totalDistance = 0;
            maxCargo = 0;
            maxPassenger = 0;
            sb = new StringBuilder();

            using (aStarSearch = new AStarRoute())
            {
                aStarSearch.CostScale = (int)numericUpDownScale.Value;
                
                IList<FlightPlanLeg> legs = aStarSearch.CalculateRoute(routes, AirportData.Airports);

                for (int i = 1; i < legs.Count; ++i)
                {
                    FlightPlanLeg prev = legs[i - 1];
                    FlightPlanLeg curr = legs[i];
                    int distance = prev.airportData.DistanceTo(curr.airportData);
                    totalDistance += distance;

                    maxCargo = Math.Max(maxCargo, prev.cargo);
                    maxPassenger = Math.Max(maxPassenger, prev.passenger);

                    sb.AppendFormat("{0} -> {1} {2,5}nm {3,6}lb {4,4}p", prev.airportData.ICAO, curr.airportData.ICAO, distance, prev.cargo, prev.passenger);
                    sb.AppendLine();
                }
            }

            var duration = TimeSpan.FromTicks(DateTime.Now.Ticks - startTicks).TotalSeconds;
            Debug.WriteLine(string.Format("search done: {0}s", duration));

            Invoke(new OnFinished(OnSearchDone));
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void OnSearchDone()
        {
            if (threadSearch != null && threadSearch.IsAlive)
            {
                AStarRoute.ContinueSearch = false;
                threadSearch = null;
            }

            textBoxResult.Text = sb != null ? sb.ToString() : string.Empty;
            labelTotalSearch.Text = string.Format("Total: {0}nm  MaxC: {1}lb  MaxP: {2}", totalDistance, maxCargo, maxPassenger);
           
            progressBarSearch.Value = 0;
            progressBarSearch.MarqueeAnimationSpeed = 0;

            updateTimer.Enabled = false;
            RefreshSearchStats(null, null);

            buttonSearch.Enabled = true;
            buttonStop.Enabled = false;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void buttonPasteOnAir_Click(object sender, EventArgs e)
        {
            PasteFromClipboard();
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void PasteFromClipboard()
        {
            string clipText = Clipboard.GetText();
            if (string.IsNullOrEmpty(clipText))
                return;

            string[] elements = clipText.Split('\t');
            if (!AirportData.Airports.TryGetValue(elements[0], out AirportData from))
                return;

            if (!AirportData.Airports.TryGetValue(elements[1], out AirportData to))
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

        //--------------------------------------------------------------------------------------------------------------------------------
        private void buttonDeleteSelected_Click(object sender, System.EventArgs e)
        {
            DeleteSelectedRoute();
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void DeleteSelectedRoute()
        {
            if (checkedListBoxRoutes.SelectedIndex >= 0 && checkedListBoxRoutes.SelectedIndex < checkedListBoxRoutes.Items.Count)
            {
                checkedListBoxRoutes.Items.RemoveAt(checkedListBoxRoutes.SelectedIndex);
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.P)
            {
                PasteFromClipboard();
            }
            else if (e.KeyCode == Keys.Delete)
            {
                DeleteSelectedRoute();
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (threadSearch != null)
            {
                threadSearch.Abort();
                threadSearch = null;
            }
        }
    }
}
