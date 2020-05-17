using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker
{
    public class TaskViewModel : ObservableObject
    {
        private DatabaseGateway _databaseGateway;

        public TaskViewModel(Task task, DatabaseGateway databaseGateway)
        {
            TaskItem = task;
            _databaseGateway = databaseGateway;
        }

        public Task TaskItem { get; private set; }

        public string Description
        {
            get { return TaskItem.Description; }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    TaskItem.Description = value;
                    _databaseGateway.UpdateTask(TaskItem);
                    OnPropertyChanged("Description");
                }
            }
        }

        public long? SecondsTracked
        {
            get { return TaskItem.SecondsTracked; }
            set
            {
                TaskItem.SecondsTracked = value;
                OnPropertyChanged("SecondsTracked");
            }
        }

        public void AddTrackedTime(long seconds)
        {
            long dateTracked = Utilities.ConvertToUnixTime(DateTime.Today);
            _databaseGateway.InsertNewTaskHistoryItem(TaskItem.TaskId, dateTracked, seconds);
            SecondsTracked += seconds;
        }


    }
}
