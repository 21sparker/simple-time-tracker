

namespace TimeTracker.TaskPage.DialogBox
{
    public class TimeDialogViewModel : ObservableObject
    {
        private string _hours;
        public string Hours
        {
            get { return _hours; }
            set
            {
                _hours = value;
                OnPropertyChanged("Hours");
            }
        }

        private string _minutes;
        public string Minutes
        {
            get { return _minutes; }
            set
            {
                _minutes = value;
                OnPropertyChanged("Minutes");
            }
        }
    }
}
