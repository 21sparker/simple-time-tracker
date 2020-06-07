﻿using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TimeTracker.TaskPage.DialogBox;

namespace TimeTracker
{
    public class TaskPageViewModel : ObservableObject, IPageViewModel
    {
        public string Name { get { return "Tasks"; } }

        private DatabaseGateway _dbGateway;
        private TimerAsync _timer;
        public ObservableCollection<TaskViewModel> TaskViewModels { get; set; }
        public ObservableCollection<WBSViewModel> WBSViewModels { get; set; }

        public TaskPageViewModel(DatabaseGateway dbGateway, ObservableCollection<TaskViewModel> taskVMs,
            ObservableCollection<WBSViewModel> wbsVMs, TimerAsync timer)
        {
            _dbGateway = dbGateway;
            _timer = timer;

            TaskViewModels = taskVMs;
            WBSViewModels = wbsVMs;
        }

        private string _taskToAdd;
        public string TaskToAdd
        {
            get { return _taskToAdd; }
            set
            {
                _taskToAdd = value;
                Trace.WriteLine(value);
                OnPropertyChanged("TaskToAdd");
            }
        }

        private bool _isTaskToAddFocused;
        public bool IsTaskToAddFocused
        {
            get { return _isTaskToAddFocused; }
            set
            {
                Trace.WriteLine("Setting value" + value);
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
                    _timer.Unsubscribe(_currentSubscriber);
                    EndTaskBeingTracked();
                    return;
                }
            }

            SetupNewTaskToTrack(task);
            Action<object> action = (obj) =>
            {
                _trackedTask.SecondsTracked += 1;
                _trackedSeconds += 1;
            };

            _currentSubscriber = new TimerObject(action);
            _timer.Subscribe(_currentSubscriber);
        }
    }
}
