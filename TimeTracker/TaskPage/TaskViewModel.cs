using System;

namespace TimeTracker
{
    public class TaskViewModel : ObservableObject
    {
        private DatabaseGateway _databaseGateway;

        public TaskViewModel(TaskItem task, DatabaseGateway databaseGateway)
        {
            MainTask = task;
            _databaseGateway = databaseGateway;
        }

        public TaskItem MainTask { get; private set; }

        public string Description
        {
            get { return MainTask.Description; }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    MainTask.Description = value;
                    _databaseGateway.UpdateTask(MainTask);
                    OnPropertyChanged("Description");
                }
            }
        }

        public long? SecondsTracked
        {
            get { return MainTask.SecondsTracked ?? 0; }
            set
            {
                MainTask.SecondsTracked = value;
                OnPropertyChanged("SecondsTracked");
            }
        }

        public DateTime CreatedDateTime
        {
            get { return Utilities.ConvertUnixSecondsToDateTime(MainTask.CreatedDateTime); }
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

        public void AddTrackedTime(long seconds)
        {
            long dateTracked = Utilities.ConvertToUnixTime(DateTime.Today);
            _databaseGateway.InsertNewTaskHistoryItem(MainTask.TaskId, dateTracked, seconds);
        }

        private WBSViewModel _wbsVM;
        public WBSViewModel WBSVM
        {
            get { return _wbsVM; }
            set
            {
                if (value != null & value != _wbsVM)
                {
                    _wbsVM = value;
                    MainTask.WBSId = _wbsVM.WBSItem.WBSId;
                    MainTask.WBSCode = _wbsVM.WBSItem;
                    _databaseGateway.UpdateTask(MainTask);
                    OnPropertyChanged("WBSVM");
                }
            }
        }
    }
}
