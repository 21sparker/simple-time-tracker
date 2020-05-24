using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TimeTracker
{
    public class WBSPageViewModel : ObservableObject, IPageViewModel
    {
        public string Name { get { return "WBS Codes"; } }

        private DatabaseGateway _dbGateway;
        public ObservableCollection<WBSViewModel> WBSViewModels { get; set; }
        public ObservableCollection<TaskViewModel> TaskViewModels { get; set; }

        public WBSPageViewModel(DatabaseGateway dbGateway, ObservableCollection<WBSViewModel> wbsItems,
            ObservableCollection<TaskViewModel> taskItems)
        {
            _dbGateway = dbGateway;

            WBSViewModels = wbsItems;
            TaskViewModels = taskItems;
        }

        private string _wbsNameToAdd;
        public string WBSNameToAdd
        {
            get { return _wbsNameToAdd; }
            set
            {
                _wbsNameToAdd = value;
                OnPropertyChanged("WBSNameToAdd");
            }
        }

        private string _wbsCodeToAdd;
        public string WBSCodeToAdd
        {
            get { return _wbsCodeToAdd; }
            set
            {
                _wbsCodeToAdd = value;
                OnPropertyChanged("WBSCodeToAdd");
            }
        }

        private ICommand _addWBSCommand;
        public ICommand AddWBSCommand
        {
            get
            {
                if (_addWBSCommand == null)
                {
                    _addWBSCommand = new RelayCommand(w => AddWBS());
                }

                return _addWBSCommand;
            }
        }

        private void AddWBS()
        {
            string wbsName = _wbsNameToAdd;
            string wbsCode = _wbsCodeToAdd;

            // Check if name is empty
            if (wbsName == null || wbsName.Replace(" ", String.Empty) == "")
            {
                return;
            }

            // Check if code is empty
            if (wbsCode == null || wbsCode.Replace(" ", String.Empty) == "")
            {
                return;
            }

            // Check if name is already in use among active wbs codes
            if (WBSViewModels.Any(w => w.Name.Equals(wbsName, StringComparison.CurrentCultureIgnoreCase)))
            {
                return;
            }

            // Insert into database
            WBS newWBS = _dbGateway.InsertNewWBS(wbsName, wbsCode, Utilities.ConvertToUnixTime(DateTime.Now));

            // Update Collection
            WBSViewModels.Add(new WBSViewModel(newWBS, _dbGateway));

            // Clear user input
            WBSCodeToAdd = "";
            WBSNameToAdd = "";
        }

        private ICommand _deleteWBSCommand;
        public ICommand DeleteWBSCommand
        {
            get
            {
                if (_deleteWBSCommand == null)
                {
                    _deleteWBSCommand = new RelayCommand(t => DeleteWBS((WBSViewModel)t));
                }

                return _deleteWBSCommand;
            }
        }

        public void DeleteWBS(WBSViewModel wbs)
        {
            // Check if currently used by task
            foreach (TaskViewModel taskVM in TaskViewModels)
            {
                if (taskVM.WBSVM == null)
                {
                    continue;
                }
                else if (taskVM.WBSVM.WBSItem.WBSId == wbs.WBSItem.WBSId)
                {
                    return;
                }
            }

            // Delete task from database
            _dbGateway.DeleteWBS(wbs.WBSItem);

            // Remove task from collection
            WBSViewModels.Remove(wbs);
        }

    }
}
