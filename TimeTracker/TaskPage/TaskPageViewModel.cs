﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TimeTracker
{
    public class TaskPageViewModel : ObservableObject, IPageViewModel
    {
        public string Name { get { return "Tasks"; } }

        private DatabaseGateway _dbGateway;
        public ObservableCollection<TaskViewModel> TaskViewModels { get; set; }

        public TaskPageViewModel(DatabaseGateway dbGateway)
        {
            _dbGateway = dbGateway;

            // Loads active tasks
            TaskViewModels = new ObservableCollection<TaskViewModel>();
            List<TaskItem> tasks = _dbGateway.LoadTasks();
            foreach (TaskItem task in tasks)
            {
                TaskViewModels.Add(new TaskViewModel(task, _dbGateway));
            }
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
            if (taskDescription == null)
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
            _dbGateway.DeleteTask(task.TaskItem);

            // Remove task from collection
            TaskViewModels.Remove(task);
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


        private Task<bool> _pendingTask = null;
        private CancellationTokenSource _cts = null;
        private TaskViewModel _trackedTask;
        private int _trackedSeconds = 0;


        // TODO: Refactor these async functions
        public async void TrackTask(TaskViewModel task)
        {
            // If _trackedTask is null then the user is stopping the timer
            if (_trackedTask != null)
            {
                _cts.Cancel();
                return;
            }

            _trackedTask = task;

            try 
            { 
                await TrackTaskAsync(); 
            } catch 
            {
                _trackedTask.AddTrackedTime(_trackedSeconds);
                _trackedTask = null;
                _trackedSeconds = 0;
            }
        }

        public async Task<bool> TrackTaskAsync()
        {
            CancellationTokenSource previousCts = _cts;
            _cts = new CancellationTokenSource();

            if (previousCts != null)
            {
                // cancel the previous session and wait for its termination
                previousCts.Cancel();
                try { await _pendingTask;  } catch { }
            }

            _cts.Token.ThrowIfCancellationRequested();
            this._pendingTask = TrackTaskHelper(_cts.Token);

            return await _pendingTask;
        }

        private async Task<bool> TrackTaskHelper(CancellationToken token)
        {
            bool doMore = true;
            while(doMore)
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(1000);
                _trackedSeconds += 1;
                Trace.WriteLine(_trackedSeconds);
            }
            return doMore;
        }

    }
}
