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
            List<Task> tasks = _dbGateway.LoadTasks();
            foreach (Task task in tasks)
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
            if (TaskViewModels.Any(t => t.Description.Equals(taskDescription, StringComparison.CurrentCultureIgnoreCase)))
            {
                return;
            }

            // Insert new task to database
            Task newTask = _dbGateway.InsertNewTask(taskDescription, Utilities.ConvertToUnixTime(DateTime.Now));

            // Update collection
            TaskViewModels.Add(new TaskViewModel(newTask, _dbGateway));
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
    }
}
