using System;
using System.Xml.Serialization;

namespace Salon_App_WPF
{
    [XmlRoot("customer")]
    public class Customer
    {
        [XmlElement("ID")]
        private int _customerID;
        [XmlElement("FirstName")]
        private string _firstName;
        [XmlElement("LastName")]
        private string _lastName;
        [XmlElement("NickName")]
        private string _nickName;
        [XmlElement("Phone")]
        private string _phone;
        [XmlElement("Email")]
        private string _email;
        [XmlElement("CreationDate")]
        private Nullable<DateTime> _firstVisit;
        [XmlElement("Gender")]
        private char _gender;

        
        public int CustomerID { get { return this._customerID; } set { this._customerID = value; } }
        public string FirstName { get { return this._firstName; } set { this._firstName = value; } }
        public string LastName { get { return this._lastName; } set { this._lastName = value; } }
        public string NickName { get { return this._nickName; } set { this._nickName = value; } }
        public string Phone { get { return this._phone; } set { this._phone = value; } }
        public string Email { get { return _email; } set { this._email= value; } }
        public Nullable<DateTime> FirstVisit { get { return this._firstVisit; } }
        public char Gender { get { return _gender; } set { this._gender = value; } }

        public Customer()
        {

        }
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
