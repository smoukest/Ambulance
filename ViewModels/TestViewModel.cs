using Ambulance.Models;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Tmds.DBus.Protocol;

namespace Ambulance.ViewModels
{
    public class TestViewModel : ReactiveObject
    {
        static string connectionString = "Server=localhost;Port=5432;Username=postgres;Password=123;Database=amb;";
        DatabaseService _dt = new DatabaseService(connectionString);

        private bool isProcessing = false;
        private List<DataGroup> statesTemplates;
        public ObservableCollection<DataGroup> StatesOnDisplay { get; set; }
        public ObservableCollection<DataGroup> SelectedStates { get; set; }


        [Reactive]
        public DataGroup AllStates { get; set; }
        [Reactive]
        public bool AllStatesIsSelected { get; set; }
        [Reactive]
        public bool? AllStatesIsChecked { get; set; }

        public TestViewModel()
        {
            statesTemplates = new List<DataGroup>()
            {
               new DataGroup(1, "Burgenland", 0, false, false),
               new DataGroup(2, "Kärnten", 0, false, false),
               new DataGroup(3, "Niederösterreich", 0, false, false),
               new DataGroup(4, "Oberösterreich", 0, false, false),
               new DataGroup(5, "Salzburg", 0, false, false),
               new DataGroup(6, "Steiermark", 0, false, false),
               new DataGroup(7, "Tirol", 0, false, false),
               new DataGroup(8, "Vorarlberg", 0, false, false),
               new DataGroup(9, "Wien", 0, false, false)
            };

            StatesOnDisplay = new ObservableCollection<DataGroup>(statesTemplates);

            AllStates = new DataGroup(0, "Alle Bundesländer", 0, false, false);
            AllStatesIsChecked = false;

            SelectedStates = new ObservableCollection<DataGroup>();
            SelectedStates.CollectionChanged += (sender, e) => OnSelectedStatesChanged(sender, e);

            this.WhenAnyValue(x => x.AllStatesIsChecked).Subscribe(x =>
            {
                if (x != null)
                {
                    UpdateCheckBoxes(x);
                }
            });
            this.WhenAnyValue(x => x.AllStatesIsSelected).Subscribe(x =>
            {
                if ((AllStatesIsChecked == null || AllStatesIsChecked == false) && x == true)
                {
                    AllStatesIsChecked = true;
                }
                else if (AllStatesIsChecked == true && x == false)
                {
                    AllStatesIsChecked = false;
                }
            });
        }

        private void OnSelectedStatesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateAllStates();
        }

        private void UpdateCheckBoxes(bool? isChecked)
        {
            if (!isProcessing)
            {
                isProcessing = true;

                if (isChecked == true)
                {
                    foreach (var state in StatesOnDisplay)
                    {
                        if (state.IsSelected == false) state.IsSelected = true;
                    }
                }
                else if (isChecked == false)
                {
                    foreach (var state in StatesOnDisplay)
                    {
                        if (state.IsSelected == true) state.IsSelected = false;
                    }
                }

                isProcessing = false;
            }
        }

        private void UpdateAllStates()
        {
            var selectedStatesCount = SelectedStates.Count;
            Debug.WriteLine($"CheckBoxes: {selectedStatesCount}/{StatesOnDisplay.Count}");

            if (selectedStatesCount == StatesOnDisplay.Count)
            {
                AllStatesIsSelected = true;
                AllStatesIsChecked = true;
            }
            else if (selectedStatesCount == 0)
            {
                AllStatesIsSelected = false;
                AllStatesIsChecked = false;
            }
            else
            {
                AllStatesIsChecked = null;
            }
        }


        /*private async Task ImportFileAsync()
        {

            //Importing an Excel-File to a DataTable

            //var importTestModels = new List<TestModel>();
            //var distinctZOs = new Dictionary<int, int>();

            string[,] importTable = _dt.GetAllPatient("", "", "", "", "", "", "", "");

            for (int i = 1; i < importTable.Rows.Count; i++)
            {
                ...

                    if (!distinctZOs.ContainsKey(zoCode)) distinctZOs.Add(zoCode, 1);
                else distinctZOs[zoCode]++;

                importTestModels.Add(new TestModel(id, name, email, address, zoCode));
            }

            AllStatesIsChecked = true;

            DataGridWorkData.Add(importTestModels);
            DataGridDisplayData.Add(importTestModels);

            var statesNotPresentInImportData = StatesOnDisplay.Where(x => !distinctZOs.ContainsKey(x.Id)).ToList();

            StatesOnDisplay.Remove(statesNotPresentInImportData);
            foreach (var state in StatesOnDisplay)
            {
                state.Count = distinctZOs[state.Id];
            }

            AllStates.Count = distinctZOs.Values.Sum();

        }

        private async Task UpdateStateFilter()
        {
            var selectedStateIds = new List<int>();
            foreach (var s in SelectedStates) selectedStateIds.Add(s.Id);

            var currentlyDisplayedData = DataGridDisplayData.ToList();
            var dataToDisplay = DataGridWorkData.Where(x => selectedStateIds.Contains(x.ZOCode)).ToList();

            DataGridDisplayData.Remove(currentlyDisplayedData.Except(dataToDisplay));
            DataGridDisplayData.Add(dataToDisplay.Except(currentlyDisplayedData));
        }*/
    }
}
