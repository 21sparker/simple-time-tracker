using MaterialDesignThemes.Wpf;
using Notification.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using TimeTracker.TaskPage.DialogBox;

namespace TimeTracker
{
    public class TaskPageViewModel : ObservableObject, IPageViewModel
    {
        private DatabaseGateway _dbGateway;
        private TimerAsync _timer;
        private NotificationManager _notificationManager;

        public string Name { get { return "Tasks"; } }
        public ObservableCollection<TaskViewModel> TaskViewModels { get; set; }
        public ObservableCollection<WBSViewModel> WBSViewModels { get; set; }

        public TaskPageViewModel(DatabaseGateway dbGateway, ObservableCollection<TaskViewModel> taskVMs,
            ObservableCollection<WBSViewModel> wbsVMs, TimerAsync timer)
        {
            _dbGateway = dbGateway;
            _timer = timer;

            _notificationManager = new NotificationManager();

            WBSViewModels = wbsVMs;
            TaskViewModels = taskVMs;

            StartNotificationTracking();
        }

        private string _taskToAdd;
        public string TaskToAdd
        {
            get { return _taskToAdd; }
            set
            {
                _taskToAdd = value;
                OnPropertyChanged("TaskToAdd");
            }
        }

        private bool _isTaskToAddFocused;
        public bool IsTaskToAddFocused
        {
            get { return _isTaskToAddFocused; }
            set
            {
                if(_isTaskToAddFocused == value)
                {
                    _isTaskToAddFocused = false;
                    OnPropertyChanged("IsTaskToAddFocused");
                }

                _isTaskToAddFocused = value;
                OnPropertyChanged("IsTaskToAddFocused");
            }
        }

        private ICommand _focusTaskToAdd;
        public ICommand FocusTaskToAdd
        {
            get
            {
                if (_focusTaskToAdd == null)
                {
                    _focusTaskToAdd = new RelayCommand(t => {
                        IsTaskToAddFocused = true;
                    });
                }

                return _focusTaskToAdd;
            }
        }


        private ICommand _addTaskCommand;
        public ICommand AddTaskCommand
        {
            get
            {
                if (_addTaskCommand == null)
                {
                    _addTaskCommand = new RelayCommand(t => AddTask());
                }

                return _addTaskCommand;
            }
        }

        public void AddTask()
        {
            string taskDescription = _taskToAdd;
            //TaskItemField is empty
            if (taskDescription == null || taskDescription.Replace(" ", String.Empty) == "")
            {
                return;
            }

            // TODO: Check for field that is just full of spaces

            //TaskItemDescription is not already uses, case-insensitive
            if (TaskViewModels.Any(t => t.Description.Equals(taskDescription, StringComparison.CurrentCultureIgnoreCase)))
            {
                return;
            }

            // Insert new task to database
           TaskItem newTask = _dbGateway.InsertNewTask(taskDescription, Utilities.ConvertToUnixTime(DateTime.Now));

            // Update collection
            TaskViewModels.Add(new TaskViewModel(newTask, _dbGateway));

            // Clear user input
            TaskToAdd = "";
        }


        private ICommand _deleteTaskCommand;
        public ICommand DeleteTaskCommand
        {
            get
            {
                if (_deleteTaskCommand == null)
                {
                    _deleteTaskCommand = new RelayCommand(t => DeleteTask((TaskViewModel)t));
                }

                return _deleteTaskCommand;
            }
        }

        public void DeleteTask(TaskViewModel task)
        {
            // Ignore delete if task is currently being tracked
            if (task == _trackedTask)
            {
                return;
            }

            // Delete task from database
            _dbGateway.DeleteTask(task.MainTask);

            // Remove task from collection
            TaskViewModels.Remove(task);
        }


        private ICommand _executeTimeDialogCommand;
        public ICommand ExecuteTimeDialogCommand
        {
            get
            {
                if (_executeTimeDialogCommand == null)
                {
                    _executeTimeDialogCommand = new RelayCommand(t => ExecuteTimeDialog((TaskViewModel)t));
                }

                return _executeTimeDialogCommand;
            }
        }

        private async void ExecuteTimeDialog(TaskViewModel task)
        {
            // set up time dialog
            TimeDialogView view = new TimeDialogView
            {
                DataContext = new TimeDialogViewModel()
            };

            // show the dialog
            var result = await DialogHost.Show(view, "RootDialog");

            bool selectedOk = (bool)result;

            if (selectedOk)
            {
                long totalSeconds = 0;

                int minutes;
                if (Int32.TryParse(((TimeDialogViewModel)view.DataContext).Minutes, out minutes))
                {
                    totalSeconds += (long)(minutes * 60);
                }

                int hours;
                if (Int32.TryParse(((TimeDialogViewModel)view.DataContext).Hours, out hours))
                {
                    totalSeconds += (long)(hours * 60 * 60);
                }

                if (totalSeconds > 0)
                {
                    task.AddTrackedTime(totalSeconds);
                    task.SecondsTracked += totalSeconds;
                }
            }
        }

        private ICommand _trackTaskCommand;
        public ICommand TrackTaskCommand
        {
            get
            {
                if (_trackTaskCommand == null)
                {
                    _trackTaskCommand = new RelayCommand(t => TrackTask((TaskViewModel)t));
                }

                return _trackTaskCommand;
            }
        }

        private bool _isTracking;
        public bool IsTracking
        {
            get { return _isTracking; }
            set
            {
                _isTracking = value;
                OnPropertyChanged("IsTracking");
            }
        }

        private int _trackedSeconds = 0;
        private TaskViewModel _trackedTask;
        private ISubscriber _currentSubscriber;
        private ISubscriber _notificationSubscriber;

        private void SetupNewTaskToTrack(TaskViewModel task)
        {
            _trackedTask = task;
            _trackedTask.IsTracking = true;
            IsTracking = true;
        }

        private void EndTaskBeingTracked()
        {
            _trackedTask.AddTrackedTime(_trackedSeconds);
            _trackedTask.IsTracking = false;
            _trackedTask = null;
            _trackedSeconds = 0;
            IsTracking = false;
        }
        
        private void SendNotification()
        {
            if (_trackedSeconds == 60)
            {
                NotificationContent content = new NotificationContent
                {
                    Title = "Time Tracker",
                    Message = "The last 15 minutes have not been tracked",
                    Type = NotificationType.Notification
                };
                _notificationManager.Show(content);
            }
        }

        private void StartNotificationTracking()
        {
            Action<object> notifyAction = (obj) =>
            {
                _trackedSeconds += 1;
                SendNotification();
            };
            _notificationSubscriber = new TimerObject(notifyAction);
            _timer.Subscribe(_notificationSubscriber);
        }

        public void TrackTask(TaskViewModel task)
        {
            if (_trackedTask != null)
            {
                if (_trackedTask != task)
                {
                    return;
                }
                else
                {
                    // Stop task from being tracked
                    _timer.Unsubscribe(_currentSubscriber);
                    EndTaskBeingTracked();

                    // Start background notify task
                    StartNotificationTracking();

                    return;
                }
            }

            _timer.Unsubscribe(_notificationSubscriber);
            _trackedSeconds = 0;

            SetupNewTaskToTrack(task);
            Action<object> action = (obj) =>
            {
                _trackedTask.SecondsTracked += 1;
                _trackedSeconds += 1;
            };

            _currentSubscriber = new TimerObject(action);
            _timer.Subscribe(_currentSubscriber);
        }

        private ICommand _writeToCSVCommand;
        public ICommand WriteToCSVCommand
        {
            get
            {
                if (_writeToCSVCommand == null)
                {
                    _writeToCSVCommand = new RelayCommand(p => WriteToCSV());
                }

                return _writeToCSVCommand;
            }
        }

        private void WriteToCSV()
        {
            // Don't write if a task is currently being tracked
            if (IsTracking)
            {
                return;
            }

            CsvExport myExport = new CsvExport(includeColumnSeparatorDefinitionPreamble: false);
            string todaysDate = DateTime.Today.ToString("MM/dd");
            int numRows = 0;

            foreach (TaskViewModel taskVM in TaskViewModels)
            {
                TaskItem ti = taskVM.MainTask;
                if (ti.WBSCode != null)
                {
                    myExport.AddRow();
                    myExport["Date"] = todaysDate;
                    myExport["WBS Code"] = ti.WBSCode.Code;
                    myExport["Description"] = ti.Description;
                    myExport["Hours"] = Math.Round((double)ti.SecondsTracked / 3600, 1);

                    numRows += 1;
                }
            }

            if (numRows > 0) myExport.ExportToFile("sample.csv");
        }
    }
}
