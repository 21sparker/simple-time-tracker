using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TimeTracker
{
    public class TaskViewModel : ObservableObject, IPageViewModel
    {
        public string Name { get { return "Tasks"; } }

        private DatabaseGateway _dbGateway;
        public ObservableCollection<Task> Tasks { get; set; }

        public TaskViewModel(DatabaseGateway dbGateway)
        {
            _dbGateway = dbGateway;

            // Loads active tasks
            Tasks = new ObservableCollection<Task>(_dbGateway.LoadTasks());
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
            // Task Field is empty
            if (taskDescription == null)
            {
                return;
            }

            // TODO: Check for field that is just full of spaces

            // Task Description is not already uses, case-insensitive
            if (Tasks.Any(t => t.Description.Equals(taskDescription, StringComparison.CurrentCultureIgnoreCase)))
            {
                return;
            }

            // Insert new task to database
            Task newTask = _dbGateway.InsertNewTask(taskDescription, Utilities.ConvertToUnixTime(DateTime.Now));

            // Update collection
            Tasks.Add(newTask);
        }


        private ICommand _deleteTaskCommand;
        public ICommand DeleteTaskCommand
        {
            get
            {
                if (_deleteTaskCommand == null)
                {
                    _deleteTaskCommand = new RelayCommand(t => DeleteTask((Task)t));
                }

                return _deleteTaskCommand;
            }
        }

        public void DeleteTask(Task task)
        {
            // Delete task from database
            _dbGateway.DeleteTask(task);

            // Remove task from collection
            Tasks.Remove(task);
        }

    }
}
