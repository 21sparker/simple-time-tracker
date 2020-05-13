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

namespace TimeTracker
{
    public class MainWindowViewModel : ObservableObject
    {

        //TODO: Need to add something that closes the database when the window closes
        // See here: https://stackoverflow.com/questions/3683450/handling-the-window-closing-event-with-wpf-mvvm-light-toolkit


        private ICommand _changePageCommand;

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


            // Add available pages


            // Set starting page
            //CurrentPageViewModel = PageViewModels[0];

        }


        public ICommand ChangePageCommand
        {
            get
            {
                if (_changePageCommand == null)
                {
                    _changePageCommand = new RelayCommand(
                        p => ChangeViewModel((IPageViewModel)p),
                        p => p is IPageViewModel);
                }

                return _changePageCommand;
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

        private void ChangeViewModel(IPageViewModel viewModel)
        {
            if (!PageViewModels.Contains(viewModel))
            {
                PageViewModels.Add(viewModel);
            }

            CurrentPageViewModel = PageViewModels.FirstOrDefault(vm => vm == viewModel);
        }

    }
}
