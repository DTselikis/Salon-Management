using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
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

namespace Salon_App_WPF
{
    /// <summary>
    /// Interaction logic for CustomersBaseControl.xaml
    /// </summary>
    public partial class CustomersBaseControl : UserControl
    {
        private string connStr = Properties.DefaultSettings.Default.DBConnStr.Replace("Path", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Salon Management", "Resources"));

        private Logger logger;
        private ObservableCollection<CustomerGrid> Customers { get; set; }
        public CustomersBaseControl()
        {
            InitializeComponent();

            logger = new Logger();

            logger.Section("CustomerBaseControl: Constructor");

            using (SqlConnection dbConn = new SqlConnection(connStr))
            {
                try
                {
                    logger.Log("Connectin to DB.");
                    dbConn.Open();
                }
                catch (SqlException ex)
                {
                    logger.Log("Error while connecting to DB: " + ex.ToString());

                    MessageBox.Show("Παρουσιάστηκε πρόβλημα κατά στη σύνδεση. Παρακαλούμε επικοινωνήστε με το τεχνικό τμήμα.",
                        "Προέκυψε πρόβλημα",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.OK);

                    return;
                }

                string query = "SELECT * FROM dbo.Customers;";

                SqlCommand command = new SqlCommand(query, dbConn);

                logger.Log("Executing query.");
                SqlDataReader dataReader = command.ExecuteReader();

                // Use ObservableCollection so everytime there is a change to the collection
                // it will appear to the GUI as well
                Customers = new ObservableCollection<CustomerGrid>();
                while (dataReader.Read())
                {
                    int customerID;
                    string firstName;
                    string lastName;
                    string nickName;
                    string phone;
                    string email;
                    Nullable<DateTime> dateTime = null;
                    char gender;
                    string lastNote;

                    customerID = dataReader.GetInt32(0);
                    if (dataReader[1] != System.DBNull.Value) firstName = dataReader.GetString(1); else firstName = String.Empty;
                    if (dataReader[2] != System.DBNull.Value) lastName = dataReader.GetString(2); else lastName = String.Empty;
                    if (dataReader[3] != System.DBNull.Value) nickName = dataReader.GetString(3); else nickName = String.Empty;
                    if (dataReader[4] != System.DBNull.Value) phone = dataReader.GetString(4); else phone = String.Empty;
                    if (dataReader[5] != System.DBNull.Value) email = dataReader.GetString(5); else email = String.Empty;
                    if (dataReader[6] != System.DBNull.Value) dateTime = dataReader.GetDateTime(6); else dateTime = null;
                    if (dataReader[7] != System.DBNull.Value) gender = Char.Parse(dataReader.GetString(7).Substring(0, 1)); else gender = '\0';
                    lastNote = getLastNote(customerID);

                    Customers.Add(new CustomerGrid(customerID, firstName, lastName, nickName, phone, email, dateTime, gender, lastNote));
                }

                logger.Log("Data retrieved.");

                CustomersBase.ItemsSource = Customers;

                dataReader.Close();
                dbConn.Close();
            }
        }

        private void DeleteRecord(Customer customer)
        {
            logger.Section("CustomerBaseControl: DeleteRecord");

            using (SqlConnection dbConn = new SqlConnection(connStr))
            {

                try
                {
                    logger.Log("Connecting to DB.");
                    dbConn.Open();
                }
                catch (SqlException ex)
                {
                    logger.Log("Error while connecting to DB: " + ex.ToString());

                    MessageBox.Show("Παρουσιάστηκε πρόβλημα κατά στη σύνδεση. Παρακαλούμε επικοινωνήστε με το τεχνικό τμήμα.",
                        "Προέκυψε πρόβλημα",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.OK);

                    return;
                }

                string noteQuery = "DELETE dbo.Notes WHERE CustomerID = @ID";

                SqlCommand noteCommand= new SqlCommand(noteQuery, dbConn);
                noteCommand.Parameters.AddWithValue("@ID", System.Data.SqlDbType.Int);
                noteCommand.Parameters["@ID"].Value = customer.CustomerID;

                SqlDataAdapter noteAdapter = new SqlDataAdapter();
                noteAdapter.DeleteCommand = noteCommand;
                logger.Log("Deleting notes.");
                noteAdapter.DeleteCommand.ExecuteNonQuery();

                noteCommand.Dispose();
                noteAdapter.Dispose();

                string imageQuery = "DELETE dbo.ProFileImages WHERE CustomerID = @ID";

                SqlCommand imageCommand = new SqlCommand(imageQuery, dbConn);
                noteCommand.Parameters.AddWithValue("@ID", System.Data.SqlDbType.Int);
                noteCommand.Parameters["@ID"].Value = customer.CustomerID;

                SqlDataAdapter imageAdapter = new SqlDataAdapter();
                imageAdapter.DeleteCommand = imageCommand;
                logger.Log("Deleting image.");
                imageAdapter.DeleteCommand.ExecuteNonQuery();

                noteCommand.Dispose();
                noteAdapter.Dispose();

                string query = "DELETE dbo.Customers WHERE CustomerID = @ID";

                SqlCommand command = new SqlCommand(query, dbConn);

                command.Parameters.Add("@ID", System.Data.SqlDbType.Int);
                command.Parameters["@ID"].Value = customer.CustomerID;

                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                dataAdapter.DeleteCommand = command;
                logger.Log("Deleting Customer.");
                dataAdapter.DeleteCommand.ExecuteNonQuery();

                command.Dispose();
                dataAdapter.Dispose();
                dbConn.Close();
            }

            logger.Log("Success.");

            MessageBox.Show(
                "Ο πελάτης διαγράφτηκε.",
                "Επιτυχία",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                MessageBoxResult.OK);

        }

        private string getLastNote(int customerID)
        {
            logger.Section("CustomerBaseControl: getLastNote");
            using (SqlConnection dbConn = new SqlConnection(connStr))
            {

                try
                {
                    logger.Log("Connecting to DB.");
                    dbConn.Open();
                }
                catch (SqlException ex)
                {
                    logger.Log("Error whilec connecting to DB: " + ex.ToString());

                    MessageBox.Show("Παρουσιάστηκε πρόβλημα κατά στη σύνδεση. Παρακαλούμε επικοινωνήστε με το τεχνικό τμήμα.",
                        "Προέκυψε πρόβλημα",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.OK);

                    return string.Empty;
                }

                string query = "SELECT Note FROM dbo.Notes WHERE CustomerID = @ID AND NoteID = (SELECT MAX(NoteID) FROM dbo.Notes WHERE CustomerID = @ID);";

                SqlCommand command = new SqlCommand(query, dbConn);
                command.Parameters.AddWithValue("@ID", System.Data.SqlDbType.Int);
                command.Parameters["@ID"].Value = customerID;

                logger.Log("Executing query");
                SqlDataReader dataReader = command.ExecuteReader();

                string lastNote;
                if (dataReader.Read()) lastNote = dataReader.GetString(0);
                else lastNote = string.Empty;

                command.Dispose();
                dataReader.Close();
                dbConn.Close();

                return lastNote;
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            logger.Section("CustomerBaseControl: DeleteBtn");

            if (CustomersBase.SelectedIndex > -1)
            {
                logger.Log("Deleting customer: " + Customers[CustomersBase.SelectedIndex].CustomerID);
                DeleteRecord((Customer)CustomersBase.SelectedItems[0]);
                Customers.RemoveAt(CustomersBase.SelectedIndex);
            }
            else
            {
                logger.Log("No customer was selected.");
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Customers.Clear();
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            logger.Section("CustomerBaseControl: EditBtn");

            if (CustomersBase.SelectedIndex > -1)
            {
                CustomerGrid customer = (CustomerGrid)CustomersBase.SelectedItem;
                int id = customer.CustomerID;
                string firstaName = customer.FirstName;
                string lastName = customer.LastName;
                string nickName = customer.NickName;
                string phone = customer.Phone;
                string email = customer.Email;
                Nullable<DateTime> firstVisit = customer.FirstVisit;
                char gender = customer.Gender;

                logger.Log("Showing customer: " + id);
                MainWindow.OpenUserControl(new CustomerControl(new Customer(id, firstaName, lastName, nickName, phone, email, firstVisit, gender)));
            }
            else
            {
                logger.Log("No customer was selected.");
            }
        }
    }
}
