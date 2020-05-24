using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Data.SqlClient;
using System.ComponentModel;
using System.Diagnostics;
using Dapper;
using System.Data.SQLite;
using System.Threading;
using System;
using System.Collections.ObjectModel;

namespace TimeTracker
{
    public class MainWindowViewModel : ObservableObject
    {

        //TODO: Need to add something that closes the database when the window closes
        // See here: https://stackoverflow.com/questions/3683450/handling-the-window-closing-event-with-wpf-mvvm-light-toolkit



        private ICommand _changePageByNameCommand;

        private IPageViewModel _currentPageViewModel;
        private List<IPageViewModel> _pageViewModels;

        /// <summary>
        /// Data application EF Core context
        /// </summary>
        private DatabaseGateway _DBGateway;


        public MainWindowViewModel()
        {
            // Connect to database
            _DBGateway = new DatabaseGateway($"Data Source={App.databasePath};");

            // Load all data
            _DBGateway.LoadAllData();

            // Convert data to VMs
            ObservableCollection<TaskViewModel> taskItems = new ObservableCollection<TaskViewModel>();
            ObservableCollection<WBSViewModel> wbsItems = new ObservableCollection<WBSViewModel>();

            // Need to convert each task item to a view model, but its corresponding wbs object needs to be a VM
            // we'll add wbs VMs to their list as well
            foreach (TaskItem taskItem in _DBGateway.TaskItems)
            {
                TaskViewModel taskVM = new TaskViewModel(taskItem, _DBGateway);

                if (taskItem.WBSCode != null)
                {
                    WBSViewModel wbsVM = new WBSViewModel(taskItem.WBSCode, _DBGateway);
                    taskVM.WBSVM = wbsVM;

                    if (!wbsItems.Any(w => w.WBSItem.WBSId == wbsVM.WBSItem.WBSId))
                    {
                        wbsItems.Add(wbsVM);
                    }
                }

                taskItems.Add(taskVM);
            }

            // Add any wbs codes that haven't been added in the previous loop
            foreach (WBS wbs in _DBGateway.WBSs)
            {
                WBSViewModel wbsVM = new WBSViewModel(wbs, _DBGateway);
                if (!wbsItems.Any(w => w.WBSItem.WBSId == wbsVM.WBSItem.WBSId))
                {
                    wbsItems.Add(wbsVM);
                }
            }


            // Add available pages
            PageViewModels.Add(new TaskPageViewModel(_DBGateway, taskItems, wbsItems));
            PageViewModels.Add(new WBSPageViewModel(_DBGateway, wbsItems, taskItems));

            // Set starting page
            CurrentPageViewModel = PageViewModels[0];
            HiddenPageName = "WBS Codes";

        }

        

        private string _hiddenPageName;
        public string HiddenPageName
        {
            get { return _hiddenPageName; }
            set
            {
                if (_hiddenPageName != value)
                {
                    _hiddenPageName = value;
                    OnPropertyChanged("HiddenPageName");
                }
            }
        }


        public ICommand ChangePageByNameCommand
        {
            get
            {
                if (_changePageByNameCommand == null)
                {
                    _changePageByNameCommand = new RelayCommand(
                        p => ChangeViewModelByName());
                }

                return _changePageByNameCommand;
            }

        }


        private void ChangeViewModelByName()
        {
            if (HiddenPageName == "WBS Codes")
            {
                CurrentPageViewModel = PageViewModels.FirstOrDefault(vm => vm.Name == "WBS Codes");
                HiddenPageName = "Tasks";
            } else
            {
                CurrentPageViewModel = PageViewModels.FirstOrDefault(vm => vm.Name == "Tasks");
                HiddenPageName = "WBS Codes";
            }
            
        }


        

        public List<IPageViewModel> PageViewModels
        {
            get
            {
                if (_pageViewModels == null)
                {
                    _pageViewModels = new List<IPageViewModel>();
                }

                return _pageViewModels;
            }
        }

        public IPageViewModel CurrentPageViewModel
        {
            get
            {
                return _currentPageViewModel;
            }
            set
            {
                if (_currentPageViewModel != value)
                {
                    _currentPageViewModel = value;
                    OnPropertyChanged("CurrentPageViewModel");
                }
            }
        }


        // ----------------------------------------------------------------------------------
        // NOTE THAT THE CODE ABOVE IS MY HACK TO SIMPLY GET THE APP WORKING,
        // ONCE IT WORKS, MAKE SURE TO TRANSITION TO SOMETHING MORE APPROPRIATE LIKE BELOW
        //
        //private ICommand _changePageCommand;
        //
        //public ICommand ChangePageCommand
        //{
        //    get
        //    {
        //        if (_changePageCommand == null)
        //        {
        //            _changePageCommand = new RelayCommand(
        //                p => ChangeViewModel((IPageViewModel)p),
        //                p => p is IPageViewModel);
        //        }

        //        return _changePageCommand;
        //    }
        //}

        //private void ChangeViewModel(IPageViewModel viewModel)
        //{
        //    if (!PageViewModels.Contains(viewModel))
        //    {
        //        PageViewModels.Add(viewModel);
        //    }

        //    CurrentPageViewModel = PageViewModels.FirstOrDefault(vm => vm == viewModel);
        //}
        //
        // -----------------------------------------------------------------------------------------

    }
}
