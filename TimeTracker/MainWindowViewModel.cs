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
using Notification.Wpf;

namespace TimeTracker
{
    public class MainWindowViewModel : ObservableObject
    {

        //TODO: Need to add something that closes the database when the window closes
        // See here: https://stackoverflow.com/questions/3683450/handling-the-window-closing-event-with-wpf-mvvm-light-toolkit



        private ICommand _changePageByNameCommand;

        private IPageViewModel _currentPageViewModel;
        private Dictionary<string, IPageViewModel> _pageViewModels;
        //private List<IPageViewModel> _pageViewModels;


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

            TimerAsync ta = new TimerAsync();
            ta.StartTimer();

            // Add available pages
            PageViewModels.Add("task", new TaskPageViewModel(_DBGateway, taskItems, wbsItems, ta));
            PageViewModels.Add("wbs", new WBSPageViewModel(_DBGateway, wbsItems, taskItems));

            // Set starting page
            CurrentPageViewModel = PageViewModels["task"];
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
                        p => ChangeViewModelByName((string)p)
                        );
                }

                return _changePageByNameCommand;
            }

        }

        private void ChangeViewModelByName(string page)
        {
            NotificationManager _notificationManager = new NotificationManager();
            NotificationContent content = new NotificationContent
            {
                Title = $"You clicked on {page}!",
                Message = "Thank you for doing that",
                Type = NotificationType.Information
            };
            _notificationManager.Show(content);

            IPageViewModel pageVM;

            if (PageViewModels.TryGetValue(page, out pageVM))
            {
                CurrentPageViewModel = pageVM;
            }
        }


        public Dictionary<string, IPageViewModel> PageViewModels
        {
            get
            {
                if (_pageViewModels == null)
                {
                    _pageViewModels = new Dictionary<string, IPageViewModel>();
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
    }
}
