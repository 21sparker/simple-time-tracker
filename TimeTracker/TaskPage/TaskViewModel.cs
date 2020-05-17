using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker
{
    public class TaskViewModel : ObservableObject, IPageViewModel
    {
        public string Name { get { return "Tasks"; } }

        private DatabaseGateway _dbGateway;

        public TaskViewModel(DatabaseGateway dbGateway)
        {
            _dbGateway = dbGateway;
        }

    }
}
