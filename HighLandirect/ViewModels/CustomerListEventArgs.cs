using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighLandirect.ViewModels
{
    public class CustomerListEventArgs : EventArgs
    {
        public CustomerViewModel CustomerViewModel { get; set; }
    }
}
