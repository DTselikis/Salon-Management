using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using MaterialDesignThemes;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics.Eventing.Reader;

namespace Salon_App_WPF
{
    /// <summary>
    /// Interaction logic for CustomerControl.xaml
    /// </summary>
    public partial class CustomerControl : UserControl
    {

        private string connStr = Properties.DefaultSettings.Default.DBConnStr.Replace("Path", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Salon Management", "Resources"));
        private Customer customer;
        private string imageFilePath;
        private bool hadProfileImage = false;
        private Logger logger;

        private ObservableCollection<TextBox> Notes { get; set; }
        private IDictionary<int, int> NotesID = new Dictionary<int, int>();

        public CustomerControl()
        {
            logger = new Logger();

            logger.Section("CustoemrControl: Default Constructor");

            InitializeComponent();

            firstVisitDatePicker.SelectedDate = DateTime.Now;

            OptionsLeftBtn.ToolTip = "Καταχώρηση νέου πελάτη";
            OptionsLeftBtn.Content = "Αποθήκευση";
            OptionsLeftBtn.Click += submitRecord;
            OptionsLeftBtn.Visibility = Visibility.Visible;

            

            toggleControls(null, null);

        }
        public CustomerControl(Customer customer)
        {
            logger = new Logger();
            logger.Section("CustoemrControl: Default Constructor");

            this.customer = customer;
            this.imageFilePath = string.Empty;

            InitializeComponent();

            NameTextBox.Text = customer.FirstName;
            LastNameTextBox.Text = customer.LastName;
            NickNameTextBox.Text = customer.NickName;
            PhoneTextBox.Text = customer.Phone;
            EmailTextBox.Text = customer.Email;
            firstVisitDatePicker.Text = customer.FirstVisit.ToString();
            if (customer.Gender == 'M') MaleRadioBtn.IsChecked = true;
            else if (customer.Gender == 'F') FemaleRadioBtn.IsChecked = true;

            loadProfileImage();

            OptionsLeftBtn.ToolTip = "Ενεργοποίηση επεξεργασίας στοιχείων";
            OptionsLeftBtn.Content = "Επεξεργασία";
            OptionsLeftBtn.Click += toggleControls;
            OptionsLeftBtn.Visibility = Visibility.Visible;

            OptionsRightBtn.ToolTip = "Αφαίρεση πελάτη από το πελατολόγιο";
            OptionsRightBtn.Content = "Διαγραφή";
            OptionsRightBtn.Click += deleteRecord;
            OptionsRightBtn.Visibility = Visibility.Visible;

            GetNotes();

            NewNoteTB.IsEnabled = true;
        }

        private void submitChanges(object sender, System.EventArgs e)
        {
            logger.Section("CustomerControl: SumbitChange");
            // If phone has changed and it wasn't null
            if (!PhoneTextBox.Text.Equals(string.Empty) && !PhoneTextBox.Text.Equals(customer.Phone)) {
                bool hasRows;
                string queryPhone = "SELECT CustomerID FROM dbo.Customers WHERE Phone LIKE @Phone";

                hasRows = executeQuery(queryPhone, "@Phone", PhoneTextBox.Text);

                if (hasRows)
                {
                    logger.Log("Duplicate detected.");

                   MessageBoxResult result =  MessageBox.Show(
                        "Υπάρχει ήδη μία καταχώρηση με αυτό το τηλέφωνο.\nΘέλετε να συνεχίσετε με την αποθήκυεση των αλλαγών;",
                        "Διπλότυπο",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning,
                        MessageBoxResult.No);

                    if (result == MessageBoxResult.No) return;
                }
            }

            
            string query = "UPDATE dbo.Customers SET FirstName = @Name, LastName = @LName, NickName = @NickName, Phone = @Phone, Email = @Email, FirstVisit = @FVisit, Gender = @Gender WHERE CustomerID = @ID";

            using (SqlConnection dbConn = new SqlConnection(connStr))
            {

                try
                {
                    logger.Log("Opening connection to DB.");
                    dbConn.Open();
                }
                catch (SqlException ex)
                {
                    logger.Log("Error while trying to connect: " + ex.ToString());

                    MessageBox.Show("Παρουσιάστηκε πρόβλημα κατά στη σύνδεση. Παρακαλούμε επικοινωνήστε με το τεχνικό τμήμα.",
                        "Προέκυψε πρόβλημα",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.OK);

                    return;
                }

                SqlCommand command = new SqlCommand(query, dbConn);

                command.Parameters.Add("@Name", System.Data.SqlDbType.NVarChar);
                command.Parameters.Add("@LName", System.Data.SqlDbType.NVarChar);
                command.Parameters.Add("@NickName", System.Data.SqlDbType.NVarChar);
                command.Parameters.Add("@Phone", System.Data.SqlDbType.NVarChar);
                command.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar);
                command.Parameters.Add("@FVisit", System.Data.SqlDbType.DateTime2);
                command.Parameters.Add("@Gender", System.Data.SqlDbType.NChar);
                command.Parameters.Add("@ID", System.Data.SqlDbType.Int);

                command.Parameters["@Name"].Value = NameTextBox.Text;

                if (!LastNameTextBox.Text.Equals(string.Empty)) command.Parameters["@LName"].Value = LastNameTextBox.Text;
                else command.Parameters["@LName"].Value = DBNull.Value;

                if (!NickNameTextBox.Text.Equals(string.Empty)) command.Parameters["@NickName"].Value = NickNameTextBox.Text;
                else command.Parameters["@NickName"].Value = DBNull.Value;

                if (!PhoneTextBox.Text.Equals(string.Empty)) command.Parameters["@Phone"].Value = PhoneTextBox.Text;
                else command.Parameters["@Phone"].Value = DBNull.Value;

                if (!EmailTextBox.Text.Equals(string.Empty)) command.Parameters["@Email"].Value = EmailTextBox.Text;
                else command.Parameters["@Email"].Value = DBNull.Value;

                if (firstVisitDatePicker.SelectedDate.HasValue)
                {
                    logger.Log("Taking date");

                    string date = DateTime.Parse(firstVisitDatePicker.SelectedDate.ToString()).ToString("d");
                    string time = DateTime.Now.ToString("T");
                    command.Parameters["@FVisit"].Value = DateTime.Parse(date + " " + time);
                }
                else command.Parameters["@FVisit"].Value = DBNull.Value;

                if (MaleRadioBtn.IsChecked == true) command.Parameters["@Gender"].Value = 'M';
                else if (FemaleRadioBtn.IsChecked == true) command.Parameters["@Gender"].Value = 'F';
                else command.Parameters["@Gender"].Value = DBNull.Value;

                command.Parameters["@ID"].Value = customer.CustomerID;

                SqlDataAdapter adapter = new SqlDataAdapter();

                logger.Log("Executing query");
                adapter.UpdateCommand = command;
                adapter.UpdateCommand.ExecuteNonQuery();

                adapter.Dispose();

                if (CustomerPicture.Visibility == Visibility.Visible)
                {
                    SaveProfileImage();
                }

                logger.Log("All good.");

                MessageBox.Show(
                    "Επιτυχής αλλαγή στοιχείων!",
                    "Επιτυχία",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information,
                    MessageBoxResult.OK);

                toggleControls(null, null);
            }

        }

        private void submitRecord(object sender, System.EventArgs e)
        {
            logger.Section("CustomerControl: submitRecord");

            if (NameTextBox.Text.Equals(string.Empty))
            {
                logger.Log("Name field was empty.");

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
                logger.Log("Checking if record exists.");

                String query = "INSERT INTO dbo.Customers(FirstName, LastName, NickName, Phone, Email, FirstVisit, Gender) VALUES (@Name, @LName, @NickName, @Phone, @Email, @FVisit, @Gender)";

                using (SqlConnection dbConn = new SqlConnection(connStr))
                {

                    try
                    {
                        logger.Log("Openig connection to DB.");
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

                    SqlCommand command = new SqlCommand(query, dbConn);

                    command.Parameters.Add("@Name", System.Data.SqlDbType.NVarChar);
                    command.Parameters.Add("@LName", System.Data.SqlDbType.NVarChar);
                    command.Parameters.Add("@NickName", System.Data.SqlDbType.NVarChar);
                    command.Parameters.Add("@Phone", System.Data.SqlDbType.NVarChar);
                    command.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar);
                    command.Parameters.Add("@FVisit", System.Data.SqlDbType.DateTime);
                    command.Parameters.Add("@Gender", System.Data.SqlDbType.NChar);

                    command.Parameters["@Name"].Value = NameTextBox.Text;

                    if (!LastNameTextBox.Text.Equals(string.Empty)) command.Parameters["@LName"].Value = LastNameTextBox.Text;
                    else command.Parameters["@LName"].Value = DBNull.Value;

                    if (!NickNameTextBox.Text.Equals(string.Empty)) command.Parameters["@NickName"].Value = NickNameTextBox.Text;
                    else command.Parameters["@NickName"].Value = DBNull.Value;

                    if (!PhoneTextBox.Text.Equals(string.Empty)) command.Parameters["@Phone"].Value = PhoneTextBox.Text;
                    else command.Parameters["@Phone"].Value = DBNull.Value;

                    if (!EmailTextBox.Text.Equals(string.Empty)) command.Parameters["@Email"].Value = EmailTextBox.Text;
                    else command.Parameters["@Email"].Value = DBNull.Value;

                    if (firstVisitDatePicker.SelectedDate.HasValue)
                    {
                        logger.Log("Taking date.");

                        string date = DateTime.Parse(firstVisitDatePicker.SelectedDate.ToString()).ToString("d");
                        string time = DateTime.Now.ToString("T");
                        command.Parameters["@FVisit"].Value = DateTime.Parse(date + " " + time);
                    }
                    else command.Parameters["@FVisit"].Value = DBNull.Value;    

                    if (MaleRadioBtn.IsChecked == true) command.Parameters["@Gender"].Value = 'M';
                    else if (FemaleRadioBtn.IsChecked == true) command.Parameters["@Gender"].Value = 'F';
                    else command.Parameters["@Gender"].Value = DBNull.Value;

                    SqlDataAdapter adapter = new SqlDataAdapter();

                    adapter.InsertCommand = command;
                    adapter.InsertCommand.ExecuteNonQuery();

                    adapter.Dispose();
                    dbConn.Close();

                    logger.Log("Success.");

                    MessageBox.Show(
                        "Επιτυχής καταχώρηση!",
                        "Επιτυχία",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information,
                        MessageBoxResult.OK);
                }

                // Disable controls and set window state for edit mode
                toggleControls(null, null);

                OptionsLeftBtn.ToolTip = "Ενεργοποίηση επεξεργασίας στοιχείων";
                OptionsLeftBtn.Content = "Επεξεργασία";
                OptionsLeftBtn.Click -= submitRecord;
                OptionsLeftBtn.Click += toggleControls;
                OptionsLeftBtn.Visibility = Visibility.Visible;

                OptionsRightBtn.ToolTip = "Αφαίρεση πελάτη από το πελατολόγιο";
                OptionsRightBtn.Content = "Διαγραφή";
                OptionsRightBtn.Click += deleteRecord;
                OptionsRightBtn.Visibility = Visibility.Visible;

                updateCustomerObject();

                NewNoteTB.IsEnabled = true;
            }
            
        }

        private void deleteRecord(object sender, System.EventArgs e)
        {
            logger.Section("CustomerControl: deleteRecord");

            using (SqlConnection dbConn = new SqlConnection(connStr))
            {

                try
                {
                    logger.Log("Opening connection to DB.");
                    dbConn.Open();
                }
                catch (SqlException ex)
                {
                    logger.Log("Error while connection to DB: " + ex.ToString());

                    MessageBox.Show("Παρουσιάστηκε πρόβλημα κατά στη σύνδεση. Παρακαλούμε επικοινωνήστε με το τεχνικό τμήμα.",
                        "ΠροέκυψεΠπρόβλημα",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.OK);

                    return;
                }

                SqlDataAdapter dataAdapter = new SqlDataAdapter();

                if (hadProfileImage && imageFilePath.Contains("AppData"))
                {

                    string picQuery = "DELETE dbo.ProfileImages WHERE CustomerID = @ID";

                    SqlCommand notesCommand = new SqlCommand(picQuery, dbConn);
                    notesCommand.Parameters.Add("@ID", System.Data.SqlDbType.Int);
                    notesCommand.Parameters["@ID"].Value = this.customer.CustomerID;

                    dataAdapter.DeleteCommand = notesCommand;
                    dataAdapter.DeleteCommand.ExecuteNonQuery();

                    hadProfileImage = false;
                    try
                    {
                        logger.Log("Deleting image: " + imageFilePath);
                        File.Delete(imageFilePath);
                    }
                    catch (Exception ex)
                    {
                        logger.Log("Error while deleting image: " + imageFilePath);
                        logger.Log(ex.ToString());
                    }
                    
                }

                if (Notes != null)
                {
                    logger.Log("Deleting notes");

                    string notesQuery = "DELETE dbo.Notes WHERE CustomerID = @ID";

                    SqlCommand notesCommand = new SqlCommand(notesQuery, dbConn);
                    notesCommand.Parameters.Add("@ID", System.Data.SqlDbType.Int);
                    notesCommand.Parameters["@ID"].Value = this.customer.CustomerID;

                    dataAdapter.DeleteCommand = notesCommand;
                    dataAdapter.DeleteCommand.ExecuteNonQuery();

                    Notes.Clear();
                    NotesID.Clear();
                }
               

                string query = "DELETE dbo.Customers WHERE CustomerID = @ID";

                SqlCommand command = new SqlCommand(query, dbConn);

                command.Parameters.Add("@ID", System.Data.SqlDbType.Int);
                command.Parameters["@ID"].Value = this.customer.CustomerID;

                logger.Log("Deleting customer.");

                dataAdapter.DeleteCommand = command;
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

            MainWindow.OpenUserControl(new HomeControl());
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
            logger.Section("CustomerControl: execureQuery");
            logger.Log("Args: " + query + " " + field + " " + value);
            bool hasRows;
            using (SqlConnection dbConn = new SqlConnection(connStr))
            {
                SqlCommand command = new SqlCommand(query, dbConn);
                SqlDataReader dataReader;

                command.Parameters.Add(field, System.Data.SqlDbType.NVarChar);
                command.Parameters[field].Value = value;
                try
                {
                    logger.Log("Connection to DB");
                    dbConn.Open();
                }
                catch (SqlException ex)
                {
                    logger.Log("Error while connection to DB: " + ex.ToString());

                    MessageBox.Show("Παρουσιάστηκε πρόβλημα κατά στη σύνδεση. Παρακαλούμε επικοινωνήστε με το τεχνικό τμήμα.",
                        "ΠροέκυψεΠπρόβλημα",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.OK);
                }
                

                dataReader = command.ExecuteReader();

                hasRows = dataReader.Read();

                command.Dispose();
                dataReader.Close();
            }

            return hasRows;
        }

        private bool executeQuery(string query, string firstField, string secondField, string firstValue, string secondValue)
        {
            logger.Section("CustomerControl: execureQuery");
            logger.Log("Args: " + query + " " + firstField + " " + secondField + " " + firstValue + " " + secondValue);

            bool hasRows;
            using (SqlConnection dbConn = new SqlConnection(connStr))
            {
                try
                {
                    logger.Log("Connection to DB");
                    dbConn.Open();
                }
                catch (SqlException ex)
                {
                    logger.Log("Error while connection to DB: " + ex.ToString());

                    MessageBox.Show("Παρουσιάστηκε πρόβλημα κατά στη σύνδεση. Παρακαλούμε επικοινωνήστε με το τεχνικό τμήμα.",
                        "Προέκυψε πρόβλημα",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.OK);

                    return false;
                }

                SqlCommand command = new SqlCommand(query, dbConn);
                SqlDataReader dataReader;

                command.Parameters.Add(firstField, System.Data.SqlDbType.NVarChar);
                command.Parameters.Add(secondField, System.Data.SqlDbType.NVarChar);
                command.Parameters[firstField].Value = firstValue;
                command.Parameters[secondField].Value = secondValue;

                dataReader = command.ExecuteReader();

                hasRows = dataReader.Read();

                command.Dispose();
                dataReader.Close();
            }

            return hasRows;
        }

        private void toggleControls(object sender, System.EventArgs e)
        {
            NameTextBox.IsReadOnly = !NameTextBox.IsReadOnly;
            LastNameTextBox.IsReadOnly = !LastNameTextBox.IsReadOnly;
            NickNameTextBox.IsReadOnly = !NickNameTextBox.IsReadOnly;
            PhoneTextBox.IsReadOnly = !PhoneTextBox.IsReadOnly;
            EmailTextBox.IsReadOnly = !EmailTextBox.IsReadOnly;
            firstVisitDatePicker.IsEnabled = !firstVisitDatePicker.IsEnabled;
            MaleRadioBtn.IsEnabled = !MaleRadioBtn.IsEnabled;
            FemaleRadioBtn.IsEnabled = !FemaleRadioBtn.IsEnabled;
            if (this.customer != null) ChangePicBtn.IsEnabled = !ChangePicBtn.IsEnabled;

            if (this.customer != null)
            {
                if (OptionsLeftBtn.Content.Equals("Επεξεργασία"))
                {
                    OptionsLeftBtn.ToolTip = "Αποθήκευση αλλαγών";
                    OptionsLeftBtn.Content = "Αποθήκευση";
                    OptionsLeftBtn.Click -= toggleControls;
                    OptionsLeftBtn.Click += submitChanges;
                }
                else
                {
                    OptionsLeftBtn.ToolTip = "Ενεργοποίηση επεξεργασίας στοιχείων";
                    OptionsLeftBtn.Content = "Επεξεργασία";
                    OptionsLeftBtn.Click -= submitChanges;
                    OptionsLeftBtn.Click += toggleControls;
                }
            }
        }

        private void updateCustomerObject()
        {
            logger.Section("CustomerControl: updateCustomerObject");
            int customerID;
            string firstName;
            string lastName;
            string nickName;
            string phone;
            string email;
            Nullable<DateTime> dateTime = null;
            char gender;

            using (SqlConnection dbConn = new SqlConnection(connStr))
            {

                try
                {
                    logger.Log("Connecting to DB.");
                    dbConn.Open();
                }
                catch (SqlException ex)
                {
                    logger.Log("Error while connection to DB: " + ex.ToString());

                    MessageBox.Show("Παρουσιάστηκε πρόβλημα κατά στη σύνδεση. Παρακαλούμε επικοινωνήστε με το τεχνικό τμήμα.",
                        "Προέκυψε πρόβλημα",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.OK);

                    return;
                }

                // We know that the column CustomerID was set to IDENT so
                // every time the most new record will have the highest ID
                string query = "SELECT MAX(CustomerID) FROM dbo.Customers;";
                SqlCommand command = new SqlCommand(query, dbConn);

                SqlDataReader dataReader = command.ExecuteReader();

                dataReader.Read();

                customerID = dataReader.GetInt32(0);

                dataReader.Close();
                command.Dispose();

                string infoQuery = "SELECT * FROM dbo.Customers WHERE CustomerID = @ID";
                SqlCommand infoCommad = new SqlCommand(infoQuery, dbConn);

                infoCommad.Parameters.Add("@ID", System.Data.SqlDbType.Int);
                infoCommad.Parameters["@ID"].Value = customerID;

                logger.Log("Executing query");
                SqlDataReader infoReader = infoCommad.ExecuteReader();

                infoReader.Read();

                if (infoReader[1] != System.DBNull.Value) firstName = infoReader.GetString(1); else firstName = String.Empty;
                if (infoReader[2] != System.DBNull.Value) lastName = infoReader.GetString(2); else lastName = String.Empty;
                if (infoReader[3] != System.DBNull.Value) nickName = infoReader.GetString(3); else nickName = String.Empty;
                if (infoReader[4] != System.DBNull.Value) phone = infoReader.GetString(4); else phone = String.Empty;
                if (infoReader[5] != System.DBNull.Value) email = infoReader.GetString(5); else email = String.Empty;
                if (infoReader[6] != System.DBNull.Value) dateTime = infoReader.GetDateTime(6); else dateTime = null;
                if (infoReader[7] != System.DBNull.Value) gender = Char.Parse(infoReader.GetString(7).Substring(0, 1)); else gender = '\0';

                infoReader.Close();
                infoCommad.Dispose();
                dbConn.Close();
            }

            this.customer = new Customer(customerID, firstName, lastName, nickName, phone, email, dateTime, gender);
        }

        private void GetNotes()
        {
            logger.Section("CustomerControl");

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

                string query = "SELECT NoteID, Note, CreationDate FROM dbo.Notes WHERE CustomerID = @ID ORDER BY NoteID ASC";

                SqlCommand command = new SqlCommand(query, dbConn);
                command.Parameters.Add("@ID", System.Data.SqlDbType.Int);
                command.Parameters["@ID"].Value = this.customer.CustomerID;

                logger.Log("Executing query.");
                SqlDataReader dataReader = command.ExecuteReader();

                if (Notes == null)
                {
                    Notes = new ObservableCollection<TextBox>();
                    NotesListView.ItemsSource = Notes;
                }

                logger.Log("Generating notes");
                int i = 0;
                while (dataReader.Read())
                {
                    Style style = new Style(typeof(TextBox), (Style)this.FindResource("MaterialDesignOutlinedTextBox"));
                    style.Setters.Add(new Setter(MaterialDesignThemes.Wpf.HintAssist.HintProperty, dataReader.GetDateTime(2).ToString()));

                    TextBox noteTB = new TextBox();
                    noteTB.Text = dataReader.GetString(1);
                    noteTB.IsReadOnly = true;
                    noteTB.Style = style;
                    Notes.Insert(0, noteTB);
                    NotesID.Add(i, dataReader.GetInt32(0));
                    i++;
                }

                command.Dispose();
                dataReader.Close();
                dbConn.Close();
            }
        }

        private void loadProfileImage()
        {
            logger.Section("CustomerControl: loadProfileImage");

            using (SqlConnection dbConn = new SqlConnection(connStr))
            {
                try
                {
                    logger.Log("Connecting to DB.");
                    dbConn.Open();
                }
                catch (Exception ex)
                {
                    logger.Log("Error while connecting to DB: " + ex.ToString());

                    MessageBox.Show("Παρουσιάστηκε πρόβλημα κατά στη σύνδεση. Παρακαλούμε επικοινωνήστε με το τεχνικό τμήμα.",
                        "Προέκυψε πρόβλημα",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.OK);

                    return;
                }

                string query = "SELECT FileName FROM dbo.ProfileImages WHERE CustomerID = @ID";

                SqlCommand command = new SqlCommand(query, dbConn);
                command.Parameters.AddWithValue("@ID", System.Data.DbType.Int32);
                command.Parameters["@ID"].Value = this.customer.CustomerID;

                logger.Log("Executing query.");
                SqlDataReader dataReader = command.ExecuteReader();

                if (dataReader.Read())
                {
                    this.hadProfileImage = true;
                    string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    string[] pathStrs = { appDataPath, "Salon Management", "Resources", "Images", "ProfileImages", dataReader.GetString(0) };
                    this.imageFilePath = System.IO.Path.Combine(pathStrs);

                    openProfileImage(this.imageFilePath);
                }

                command.Dispose();
                dataReader.Close();
                dbConn.Close();
            }
        }

        private void openProfileImage(string imageFilePath)
        {
            logger.Section("CustomerControl: openProfileImage");

            if (File.Exists(imageFilePath))
            {
                BitmapImage customerBitMap = null;
                try
                {
                    logger.Log("Opening image: " + imageFilePath);
                    // Use stream to be able to delete image later
                    var stream = File.OpenRead(imageFilePath);

                    logger.Log("Creating bitmap");
                    customerBitMap = new BitmapImage();
                    customerBitMap.BeginInit();
                    customerBitMap.CacheOption = BitmapCacheOption.OnLoad;
                    customerBitMap.StreamSource = stream;
                    customerBitMap.EndInit();

                    stream.Close();
                    stream.Dispose();
                }
                catch (Exception ex)
                {
                    logger.Log("Error while opening image: " + imageFilePath + ex.ToString());
                }
                
                CustomerPicBorder.BorderThickness = new Thickness(3);
                CustomerPicture.Source = customerBitMap;

                CustomerIcon.Width = 0;
                CustomerIcon.Height = 0;
                CustomerIcon.Visibility = Visibility.Hidden;

                CustomerPicture.Width = 100;
                CustomerPicture.Height = 100;
                CustomerPicture.Visibility = Visibility.Visible;
            }
            else
            {
                logger.Log("Image does not exist: " + imageFilePath);
            }
        }

        private void SaveProfileImage()
        {
            logger.Section("CustomerControl: SaveProfileImage");

            string fileName = this.customer.CustomerID.ToString();
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string[] pathStrs = { appDataPath, "Salon Management", "Resources", "Images", "ProfileImages" };
            string destination = System.IO.Path.Combine(pathStrs);
            string imageFilePath = System.IO.Path.Combine(destination, fileName + System.IO.Path.GetExtension(this.imageFilePath));

            Directory.CreateDirectory(destination);

            try
            {
                logger.Log("Coping file: " + imageFilePath);  
                File.Copy(this.imageFilePath, imageFilePath, true);
            }
            catch (Exception ex) 
            {
                logger.Log("Error while coping file :" + imageFilePath + ex.ToString());
            }

            openProfileImage(imageFilePath);
            this.imageFilePath = imageFilePath;

            using (SqlConnection dbConn = new SqlConnection(connStr))
            {
                try
                {
                    logger.Log("Connecting to DB");
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
                }
                
                string query;
                if (hadProfileImage) query = "UPDATE dbo.ProfileImages SET FileName = @FName WHERE CustomerID = @ID";
                else query = "INSERT INTO dbo.ProfileImages (CustomerID, FileName) VALUES (@ID, @FName)";

                string imgSource = this.imageFilePath;
                int lastIndex = imgSource.LastIndexOf('.');
                string imgExtension = imgSource.Substring(lastIndex, imgSource.Length - lastIndex);

                SqlCommand command = new SqlCommand(query, dbConn);
                command.Parameters.AddWithValue("@ID", System.Data.DbType.Int32);
                command.Parameters.AddWithValue("@FName", System.Data.DbType.String);
                command.Parameters["@ID"].Value = this.customer.CustomerID;
                command.Parameters["@FName"].Value = customer.CustomerID + imgExtension;

                SqlDataAdapter adapter = new SqlDataAdapter();

                if (hadProfileImage)
                {
                    logger.Log("Executing update command.");

                    adapter.UpdateCommand = command;
                    adapter.UpdateCommand.ExecuteNonQuery();
                }
                else
                {
                    logger.Log("Executing insert command.");

                    adapter.InsertCommand = command;
                    adapter.InsertCommand.ExecuteNonQuery();

                    hadProfileImage = true;
                }

                command.Dispose();
                adapter.Dispose();
                dbConn.Close();
            }
            
        }

        private void NewNoteTB_KeyDown(object sender, KeyEventArgs e)
        {
            SaveNoteBtn.IsEnabled = true;
        }

        private void NewNoteTB_GotFocus(object sender, RoutedEventArgs e)
        {
            if (NewNoteTB.Text.Equals("Νέα σημείωση")) NewNoteTB.Text = string.Empty;
        }

        private void SaveNoteBtn_Click(object sender, RoutedEventArgs e)
        {
            logger.Section("CustomerControl: SaveNoteBtn");
            if (NewNoteTB.Text.Equals(string.Empty) || NewNoteTB.Text.Trim().Equals(string.Empty)) {
                logger.Log("Empty note.");

                MessageBox.Show(
                    "Έγινε προσπάθεια αποθήκευσης κενής σημείωσης. Η σημείωση αυτή δε θα αποθηκευτεί.",
                    "Κενή σημείωση",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation,
                    MessageBoxResult.OK);

                return;
            }

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

                string query = "INSERT INTO dbo.Notes (CustomerID, Note, CreationDate) VALUES (@ID,  @Note, @Date)";
                DateTime dateTime = DateTime.Now;

                SqlCommand command = new SqlCommand(query, dbConn);
                command.Parameters.Add("@ID", System.Data.SqlDbType.Int);
                command.Parameters.Add("@Note", System.Data.SqlDbType.NVarChar);
                command.Parameters.Add("@Date", System.Data.SqlDbType.DateTime);
                command.Parameters["@ID"].Value = this.customer.CustomerID;
                command.Parameters["@Note"].Value = NewNoteTB.Text;
                command.Parameters["@Date"].Value = dateTime;

                SqlDataAdapter adapter = new SqlDataAdapter();

                logger.Log("Executing query.");
                adapter.InsertCommand = command;
                adapter.InsertCommand.ExecuteNonQuery();

                string noteQuery = "SELECT MAX(NoteID) FROM dbo.Notes WHERE CustomerID = @ID";
                SqlCommand noteCommand = new SqlCommand(noteQuery, dbConn);
                noteCommand.Parameters.Add("@ID", System.Data.SqlDbType.Int);
                noteCommand.Parameters["@ID"].Value = this.customer.CustomerID;

                SqlDataReader dataReader = noteCommand.ExecuteReader();
                dataReader.Read();

                int lastIndex = NotesID.Count > 0 ? NotesID.Count : 0;
                NotesID.Add(lastIndex, dataReader.GetInt32(0));

                noteCommand.Dispose();
                dataReader.Close();
                command.Dispose();
                adapter.Dispose();
                dbConn.Close();

                if (Notes == null)
                {
                    Notes = new ObservableCollection<TextBox>();
                    NotesListView.ItemsSource = Notes;
                }

                Style style = new Style(typeof(TextBox), (Style)this.FindResource("MaterialDesignOutlinedTextBox"));
                style.Setters.Add(new Setter(MaterialDesignThemes.Wpf.HintAssist.HintProperty, dateTime.ToString()));
                

                TextBox noteTB = new TextBox();
                noteTB.Text = NewNoteTB.Text;
                noteTB.IsReadOnly = true;
                noteTB.Style = style;
                noteTB.MaxWidth = 540;

                Notes.Insert(0, noteTB);

                SaveNoteBtn.IsEnabled = false;
                NewNoteTB.Text = "Νέα σημείωση";
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            logger.Section("CustomerControl: DeleteBtn");

            using (SqlConnection dbConn = new SqlConnection(connStr))
            {

                try
                {
                    logger.Log("Connection to DB.");

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

                string query = "DELETE dbo.Notes WHERE NoteID = @ID";


                SqlCommand command = new SqlCommand(query, dbConn);

                command.Parameters.Add("@ID", System.Data.SqlDbType.Int);
                command.Parameters["@ID"].Value = NotesID[NotesID.Count() - NotesListView.SelectedIndex - 1];

                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                dataAdapter.DeleteCommand = command;
                logger.Log("Executing query.");
                dataAdapter.DeleteCommand.ExecuteNonQuery();


                command.Dispose();
                dataAdapter.Dispose();
                dbConn.Close();

                Notes.RemoveAt(NotesListView.SelectedIndex);
                NotesID.Remove(NotesID.Count() - NotesListView.SelectedIndex - 1);

            }
        }

        private void ChangePicBtn_Click(object sender, RoutedEventArgs e)
        {
            logger.Section("CustomerControl: ChangePicBtn");

            OpenFileDialog openFileDialog = new OpenFileDialog();
            
            openFileDialog.Title = "Επιλογή εικόνας πελάτη";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png)|*.jpg;*.jpeg;*.jpe;*.jfif;*.png";

            if (openFileDialog.ShowDialog() == true)
            {
                logger.Log("File choosed.");

                openProfileImage(openFileDialog.FileName);
                imageFilePath = openFileDialog.FileName;
            }
            
        }

        private void CustomerPicture_MouseEnter(object sender, MouseEventArgs e)
        {
            PopUpPicture.Source = CustomerPicture.Source;

            ImagePopUP.IsOpen = true;
        }

        private void CustomerPicture_MouseLeave(object sender, MouseEventArgs e)
        {
            ImagePopUP.IsOpen = false;
        }
    }
}
