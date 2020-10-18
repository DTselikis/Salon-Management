using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salon_App_WPF
{
    public class CustomerGrid : Customer
    {
        private string _lastNote;

        public string LastNote { get { return this._lastNote; } set { this._lastNote = value; } }

        public CustomerGrid(int customerID, string firstName, string lastName, string phone, string email, Nullable<DateTime> firstVisit, char gender, string lastNote): base(customerID, firstName, lastName, phone, email, firstVisit, gender)
        {
            this._lastNote = lastNote;
        }
    }
}
