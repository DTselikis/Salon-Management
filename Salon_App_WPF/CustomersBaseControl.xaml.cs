using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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
    /// Interaction logic for CustomersBaseControl.xaml
    /// </summary>
    public partial class CustomersBaseControl : UserControl
    {
        private string connStr = Properties.Settings.Default.DBConnStr;
        private ObservableCollection<Customer> Customers { get; set; }
        public CustomersBaseControl()
        {
            InitializeComponent();

            using (SqlConnection dbConn = new SqlConnection(connStr))
            {
                try
                {
                    dbConn.Open();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Παρουσιάστηκε πρόβλημα κατά στη σύνδεση. Παρακαλούμε επικοινωνήστε με το τεχνικό τμήμα.",
                        "Προέκυψε πρόβλημα",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.OK);

                    return;
                }

                string query = "SELECT * FROM dbo.Customers;";

                SqlCommand command = new SqlCommand(query, dbConn);

                SqlDataReader dataReader = command.ExecuteReader();

                // Use ObservableCollection so everytime there is a change to the collection
                // it will appear to the GUI as well
                Customers = new ObservableCollection<Customer>();
                while (dataReader.Read())
                {
                    int customerID;
                    string firstName;
                    string lastName;
                    string phone;
                    string email;
                    Nullable<DateTime> dateTime = null;
                    char gender;

                    customerID = dataReader.GetInt32(0);
                    if (dataReader[1] != System.DBNull.Value) firstName = dataReader.GetString(1); else firstName = String.Empty;
                    if (dataReader[2] != System.DBNull.Value) lastName = dataReader.GetString(2); else lastName = String.Empty;
                    if (dataReader[3] != System.DBNull.Value) phone = dataReader.GetString(3); else phone = String.Empty;
                    if (dataReader[4] != System.DBNull.Value) email = dataReader.GetString(4); else email = String.Empty;
                    if (dataReader[5] != System.DBNull.Value) dateTime = dataReader.GetDateTime(5); else dateTime = null;
                    if (dataReader[6] != System.DBNull.Value) gender = Char.Parse(dataReader.GetString(6).Substring(0, 1)); else gender = '\0';

                    Customers.Add(new Customer(customerID, firstName, lastName, phone, email, dateTime, gender));
                }


                CustomersBase.ItemsSource = Customers;

                dataReader.Close();
                dbConn.Close();
            }
        }

        private void DeleteRecord(Customer customer)
        {
            using (SqlConnection dbConn = new SqlConnection(connStr))
            {

                try
                {
                    dbConn.Open();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Παρουσιάστηκε πρόβλημα κατά στη σύνδεση. Παρακαλούμε επικοινωνήστε με το τεχνικό τμήμα.",
                        "Προέκυψε πρόβλημα",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.OK);

                    return;
                }

                string query = "DELETE dbo.Customers WHERE CustomerID = @ID";

                SqlCommand command = new SqlCommand(query, dbConn);

                command.Parameters.Add("@ID", System.Data.SqlDbType.Int);
                command.Parameters["@ID"].Value = customer.CustomerID;

                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                dataAdapter.DeleteCommand = command;
                dataAdapter.DeleteCommand.ExecuteNonQuery();

                command.Dispose();
                dataAdapter.Dispose();
                dbConn.Close();
            }

            MessageBox.Show(
                "Ο πελάτης διαγράφτηκε.",
                "Επιτυχία",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                MessageBoxResult.OK);

        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        { 
            DeleteRecord((Customer)CustomersBase.SelectedItems[0]);
            Customers.RemoveAt(CustomersBase.SelectedIndex);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Customers.Clear();
        }
    }
}
