using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Salon_App_WPF
{
    /// <summary>
    /// Interaction logic for CustomerControl.xaml
    /// </summary>
    public partial class CustomerControl : UserControl
    {

        private string connStr = Properties.Settings.Default.DBConnStr;
        private SqlConnection dbConn;
        private Customer customer;

        public CustomerControl()
        {
            InitializeComponent();

            enableControls(null, null);

            firstVisitDatePicker.SelectedDate = DateTime.Now;

            OptionsLeftBtn.ToolTip = "Καταχώρηση νέου πελάτη";
            OptionsLeftBtn.Content = "Αποθήκευση";
            OptionsLeftBtn.Click += submitRecord;
            OptionsLeftBtn.Visibility = Visibility.Visible;

            try
            {
                dbConn = new SqlConnection(connStr);
                dbConn.Open();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Παρουσιάστηκε πρόβλημα κατά στη σύνδεση. Παρακαλούμε επικοινωνήστε με το τεχνικό τμήμα.",
                    "ΠροέκυψεΠπρόβλημα",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK);
            }

        }
        public CustomerControl(Customer customer)
        {
            this.customer = customer;

            InitializeComponent();

            NameTextBox.Text = customer.FirstName;
            LastNameTextBox.Text = customer.LastName;
            PhoneTextBox.Text = customer.Phone;
            EmailTextBox.Text = customer.Email;
            firstVisitDatePicker.Text = customer.FirstVisit;
            if (customer.Gender == 'M') MaleRadioBtn.IsChecked = true;
            else if (customer.Gender == 'F') FemaleRadioBtn.IsChecked = false;

            OptionsLeftBtn.ToolTip = "Ενεργοποίηση επεξεργασίας στοιχείων";
            OptionsLeftBtn.Content = "Επεξεργασία";
            OptionsLeftBtn.Click += enableControls;
            OptionsLeftBtn.Visibility = Visibility.Visible;

            OptionsRightBtn.ToolTip = "Καταχώρηση αλλαγών";
            OptionsRightBtn.Content = "Αποθήκευση";
            OptionsRightBtn.Click += submitChanges;
            OptionsRightBtn.Visibility = Visibility.Visible;

            try
            {
                dbConn = new SqlConnection(connStr);
                dbConn.Open();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Παρουσιάστηκε πρόβλημα κατά στη σύνδεση. Παρακαλούμε επικοινωνήστε με το τεχνικό τμήμα.",
                    "ΠροέκυψεΠπρόβλημα",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK);
            }
        }

        private void submitChanges(object sender, System.EventArgs e)
        {
            string query = "UPDATE dbo.Customers SET FirstName = @Name, LastName = @LName, Phone = @Phone, Email = @Email, FirstVisit = @FVisit, Gender = @Gender WHERE CustomerID = @ID";

            SqlCommand command = new SqlCommand(query, dbConn);

            command.Parameters.Add("@Name", System.Data.SqlDbType.NVarChar);
            command.Parameters.Add("@LName", System.Data.SqlDbType.NVarChar);
            command.Parameters.Add("@Phone", System.Data.SqlDbType.NVarChar);
            command.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar);
            command.Parameters.Add("@FVisit", System.Data.SqlDbType.DateTime2);
            command.Parameters.Add("@Gender", System.Data.SqlDbType.NChar);
            command.Parameters.Add("@ID", System.Data.SqlDbType.Int);

            command.Parameters["@Name"].Value = NameTextBox.Text;

            if (!LastNameTextBox.Text.Equals(string.Empty)) command.Parameters["@LName"].Value = LastNameTextBox.Text;
            else command.Parameters["@LName"].Value = DBNull.Value;

            if (!PhoneTextBox.Text.Equals(string.Empty)) command.Parameters["@Phone"].Value = PhoneTextBox.Text;
            else command.Parameters["@Phone"].Value = DBNull.Value;

            if (!EmailTextBox.Text.Equals(string.Empty)) command.Parameters["@Email"].Value = EmailTextBox.Text;
            else command.Parameters["@Email"].Value = DBNull.Value;

            if (firstVisitDatePicker.SelectedDate.HasValue) command.Parameters["@FVisit"].Value = firstVisitDatePicker.SelectedDate;
            else command.Parameters["@FVisit"].Value = DBNull.Value;

            if (MaleRadioBtn.IsChecked == true) command.Parameters["@Gender"].Value = 'M';
            else if (FemaleRadioBtn.IsChecked == true) command.Parameters["@Gender"].Value = 'F';
            else command.Parameters["@Gender"].Value = DBNull.Value;

            command.Parameters["@ID"].Value = customer.CustomerID;

            SqlDataAdapter adapter = new SqlDataAdapter();

            adapter.UpdateCommand = command;
            adapter.UpdateCommand.ExecuteNonQuery();

            adapter.Dispose();
            dbConn.Close();

            MessageBox.Show(
                "Επιτυχής αλλαγή στοιχείων!",
                "Επιτυχία",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                MessageBoxResult.OK);
        }

        private void submitRecord(object sender, System.EventArgs e)
        {
            if (NameTextBox.Text.Equals(string.Empty))
            {
                MessageBox.Show("Το πεδίο \"Όνομα\" δε μπορεί να είναι κενό!",
                    "Κενό πεδίο",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning,
                    MessageBoxResult.OK);

                return;
            }

            // If record is unique
            if (checkIfRecordExists() == 0)
            {
                String query = "INSERT INTO dbo.Customers(FirstName, LastName, Phone, Email, FirstVisit, Gender) VALUES (@Name, @LName, @Phone, @Email, @FVisit, @Gender)";

                SqlCommand command = new SqlCommand(query, dbConn);

                command.Parameters.Add("@Name", System.Data.SqlDbType.NVarChar);
                command.Parameters.Add("@LName", System.Data.SqlDbType.NVarChar);
                command.Parameters.Add("@Phone", System.Data.SqlDbType.NVarChar);
                command.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar);
                command.Parameters.Add("@FVisit", System.Data.SqlDbType.DateTime2);
                command.Parameters.Add("@Gender", System.Data.SqlDbType.NChar);

                command.Parameters["@Name"].Value = NameTextBox.Text;

                if (!LastNameTextBox.Text.Equals(string.Empty)) command.Parameters["@LName"].Value = LastNameTextBox.Text;
                else command.Parameters["@LName"].Value = DBNull.Value;

                if (!PhoneTextBox.Text.Equals(string.Empty)) command.Parameters["@Phone"].Value = PhoneTextBox.Text;
                else command.Parameters["@Phone"].Value = DBNull.Value;

                if (!EmailTextBox.Text.Equals(string.Empty)) command.Parameters["@Email"].Value = EmailTextBox.Text;
                else command.Parameters["@Email"].Value = DBNull.Value;

                if (firstVisitDatePicker.SelectedDate.HasValue) command.Parameters["@FVisit"].Value = firstVisitDatePicker.SelectedDate;
                else command.Parameters["@FVisit"].Value = DateTime.Now;

                if (MaleRadioBtn.IsChecked == true) command.Parameters["@Gender"].Value = 'M';
                else if (FemaleRadioBtn.IsChecked == true) command.Parameters["@Gender"].Value = 'F';
                else command.Parameters["@Gender"].Value = DBNull.Value;

                SqlDataAdapter adapter = new SqlDataAdapter();

                adapter.InsertCommand = command;
                adapter.InsertCommand.ExecuteNonQuery();

                adapter.Dispose();
                dbConn.Close();

                MessageBox.Show(
                    "Επιτυχής καταχώρηση!",
                    "Επιτυχία",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information,
                    MessageBoxResult.OK);
            }
            
        }

        private int checkIfRecordExists()
        {
            // Check if record already exitsts
            string query;
            bool hasRows;
            int queryIndex;

            if (!PhoneTextBox.Text.Equals(string.Empty))
            {
                query = "SELECT Phone FROM dbo.Customers WHERE Phone LIKE @Phone";
                hasRows = executeQuery(query, "@Phone", PhoneTextBox.Text);
                queryIndex = 0;
            }
            else if (!LastNameTextBox.Text.Equals(string.Empty))
            {
                query = "SELECT FirstName FROM dbo.Customers WHERE FirstName LIKE @Name AND LastName LIKE @LName";
                hasRows = executeQuery(query, "@Name", "@LName", NameTextBox.Text, LastNameTextBox.Text);
                queryIndex = 1;
            }
            else
            {
                query = "SELECT FirstName FROM dbo.Customers WHERE FirstName LIKE @Name";
                hasRows = executeQuery(query, "@Name", NameTextBox.Text);
                queryIndex = 2;
            }

            // If no record retrieved from DB it means the record is unique
            if (hasRows)
            { 

                StringBuilder msg = new StringBuilder();
                msg.Append("Βρέθηκε καταχώρηση με το ίδιο ");

                switch (queryIndex)
                {
                    case 0: msg.Append("τηλέφωνο"); break;
                    case 1: msg.Append("ονοματεπώνυμο"); break;
                    case 2: msg.Append("όνομα"); break;
                }

                msg.Append(".").Append(Environment.NewLine).Append("Θέλετε να συνεχίσετε με την καταχώρηση;");

                MessageBoxResult result =  MessageBox.Show(
                    msg.ToString(),
                    "Βρέθηκε διπλότυπο",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.No);

                if (result == MessageBoxResult.Yes) return 0;
                else return 1;
            }

            return 0;
        }

        private bool executeQuery(string query, string field, string value)
        {
            SqlCommand command = new SqlCommand(query, dbConn);
            SqlDataReader dataReader;

            command.Parameters.Add(field, System.Data.SqlDbType.NVarChar);
            command.Parameters[field].Value = value;

            dataReader = command.ExecuteReader();

            bool hasRows = dataReader.Read();

            command.Dispose();
            dataReader.Close();

            return hasRows;
        }

        private bool executeQuery(string query, string firstField, string secondField, string firstValue, string secondValue)
        {
            SqlCommand command = new SqlCommand(query, dbConn);
            SqlDataReader dataReader;

            command.Parameters.Add(firstField, System.Data.SqlDbType.NVarChar);
            command.Parameters.Add(secondField, System.Data.SqlDbType.NVarChar);
            command.Parameters[firstField].Value = firstValue;
            command.Parameters[secondField].Value = secondValue;

            dataReader = command.ExecuteReader();

            bool hasRows = dataReader.Read();

            command.Dispose();
            dataReader.Close();

            return hasRows;
        }

        private void enableControls(object sender, System.EventArgs e)
        {
            NameTextBox.IsReadOnly = false;
            LastNameTextBox.IsReadOnly = false;
            PhoneTextBox.IsReadOnly = false;
            EmailTextBox.IsReadOnly = false;
            firstVisitDatePicker.IsEnabled = true;
            MaleRadioBtn.IsEnabled = true;
            FemaleRadioBtn.IsEnabled = true;
        }
    }
}
