using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salon_App_WPF
{
    public class Customer
    {
        private int _customerID;
        private string _firstName;
        private string _lastName;
        private string _nickName;
        private string _phone;
        private string _email;
        private Nullable<DateTime> _firstVisit;
        private char _gender;

        
        public int CustomerID { get { return this._customerID; } set { this._customerID = value; } }
        public string FirstName { get { return this._firstName; } set { this._firstName = value; } }
        public string LastName { get { return this._lastName; } set { this._lastName = value; } }
        public string NickName { get { return this._nickName; } set { this._nickName = value; } }
        public string Phone { get { return this._phone; } set { this._phone = value; } }
        public string Email { get { return _email; } set { this._email= value; } }
        public Nullable<DateTime> FirstVisit { get { return this._firstVisit; } }
        public char Gender { get { return _gender; } set { this._gender = value; } }

        public Customer(int customerID, string firstName, string lastName, string phone, string email, Nullable<DateTime> firstVisit, char gender)
        public Customer(int customerID, string firstName, string lastName, string nickName, string phone, string email, Nullable<DateTime> firstVisit, char gender)
        {
            this._customerID = customerID;
            this._firstName = firstName;
            this._lastName = lastName;
            this._nickName = nickName;
            this._phone = phone;
            this._email = email;
            this._firstVisit = firstVisit;
            this._gender = gender;
        }

    }

    
}
