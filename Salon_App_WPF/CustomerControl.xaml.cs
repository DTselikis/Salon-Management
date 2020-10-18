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
using System.Windows.Shapes;

namespace Salon_App_WPF
{
    /// <summary>
    /// Interaction logic for CustomerControl.xaml
    /// </summary>
    public partial class CustomerControl : UserControl
    {

        private string connStr = Properties.Settings.Default.DBConnStr;
        private Customer customer;

        private ObservableCollection<TextBox> Notes { get; set; }
        private IDictionary<int, int> NotesID = new Dictionary<int, int>();

        public CustomerControl()
        {
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
            this.customer = customer;

            InitializeComponent();

            NameTextBox.Text = customer.FirstName;
            LastNameTextBox.Text = customer.LastName;
            PhoneTextBox.Text = customer.Phone;
            EmailTextBox.Text = customer.Email;
            firstVisitDatePicker.Text = customer.FirstVisit;
            if (customer.Gender == 'M') MaleRadioBtn.IsChecked = true;
            else if (customer.Gender == 'F') FemaleRadioBtn.IsChecked = true;

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
            // If phone has changed and it wasn't null
            if (!PhoneTextBox.Text.Equals(string.Empty) && !PhoneTextBox.Text.Equals(customer.Phone)) {
                bool hasRows;
                string queryPhone = "SELECT CustomerID FROM dbo.Customers WHERE Phone LIKE @Phone";

                hasRows = executeQuery(queryPhone, "@Phone", PhoneTextBox.Text);

                if (hasRows)
                {
                   MessageBoxResult result =  MessageBox.Show(
                        "Υπάρχει ήδη μία καταχώρηση με αυτό το τηλέφωνο.\nΘέλετε να συνεχίσετε με την αποθήκυεση των αλλαγών;",
                        "Διπλότυπο",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning,
                        MessageBoxResult.No);

                    if (result == MessageBoxResult.No) return;
                }
            }

            
            string query = "UPDATE dbo.Customers SET FirstName = @Name, LastName = @LName, Phone = @Phone, Email = @Email, FirstVisit = @FVisit, Gender = @Gender WHERE CustomerID = @ID";

            using (SqlConnection dbConn = new SqlConnection(connStr))
            {

                try
                {
                    dbConn.Open();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Παρουσιάστηκε πρόβλημα κατά στη σύνδεση. Παρακαλούμε επικοινωνήστε με το τεχνικό τμήμα.",
                        "ΠροέκυψεΠπρόβλημα",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.OK);

                    return;
                }

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
            using (SqlConnection dbConn = new SqlConnection(connStr))
            {

                try
                {
                    dbConn.Open();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Παρουσιάστηκε πρόβλημα κατά στη σύνδεση. Παρακαλούμε επικοινωνήστε με το τεχνικό τμήμα.",
                        "ΠροέκυψεΠπρόβλημα",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.OK);

                    return;
                }

                string notesQuery = "DELETE dbo.Notes WHERE CustomerID = @ID";

                SqlCommand notesCommand = new SqlCommand(notesQuery, dbConn);
                notesCommand.Parameters.Add("@ID", System.Data.SqlDbType.Int);
                notesCommand.Parameters["@ID"].Value = this.customer.CustomerID;

                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                dataAdapter.DeleteCommand = notesCommand;
                dataAdapter.DeleteCommand.ExecuteNonQuery();

                string query = "DELETE dbo.Customers WHERE CustomerID = @ID";

                SqlCommand command = new SqlCommand(query, dbConn);

                command.Parameters.Add("@ID", System.Data.SqlDbType.Int);
                command.Parameters["@ID"].Value = this.customer.CustomerID;

                
                dataAdapter.DeleteCommand = command;
                dataAdapter.DeleteCommand.ExecuteNonQuery();

                notesCommand.Dispose();
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

            MainWindow.CloseUserControl();
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
            bool hasRows;
            using (SqlConnection dbConn = new SqlConnection(connStr))
            {
                SqlCommand command = new SqlCommand(query, dbConn);
                SqlDataReader dataReader;

                command.Parameters.Add(field, System.Data.SqlDbType.NVarChar);
                command.Parameters[field].Value = value;

                dbConn.Open();

                dataReader = command.ExecuteReader();

                hasRows = dataReader.Read();

                command.Dispose();
                dataReader.Close();
            }

            return hasRows;
        }

        private bool executeQuery(string query, string firstField, string secondField, string firstValue, string secondValue)
        {
            bool hasRows;
            using (SqlConnection dbConn = new SqlConnection(connStr))
            {
                try
                {
                    dbConn.Open();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Παρουσιάστηκε πρόβλημα κατά στη σύνδεση. Παρακαλούμε επικοινωνήστε με το τεχνικό τμήμα.",
                        "ΠροέκυψεΠπρόβλημα",
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
            PhoneTextBox.IsReadOnly = !PhoneTextBox.IsReadOnly;
            EmailTextBox.IsReadOnly = !EmailTextBox.IsReadOnly;
            firstVisitDatePicker.IsEnabled = !firstVisitDatePicker.IsEnabled;
            MaleRadioBtn.IsEnabled = !MaleRadioBtn.IsEnabled;
            FemaleRadioBtn.IsEnabled = !FemaleRadioBtn.IsEnabled;

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
            int customerID;
            string firstName;
            string lastName;
            string phone;
            string email;
            Nullable<DateTime> dateTime = null;
            char gender;

            using (SqlConnection dbConn = new SqlConnection(connStr))
            {

                try
                {
                    dbConn.Open();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Παρουσιάστηκε πρόβλημα κατά στη σύνδεση. Παρακαλούμε επικοινωνήστε με το τεχνικό τμήμα.",
                        "ΠροέκυψεΠπρόβλημα",
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

                SqlDataReader infoReader = infoCommad.ExecuteReader();

                infoReader.Read();

                if (infoReader[1] != System.DBNull.Value) firstName = infoReader.GetString(1); else firstName = String.Empty;
                if (infoReader[2] != System.DBNull.Value) lastName = infoReader.GetString(2); else lastName = String.Empty;
                if (infoReader[3] != System.DBNull.Value) phone = infoReader.GetString(3); else phone = String.Empty;
                if (infoReader[4] != System.DBNull.Value) email = infoReader.GetString(4); else email = String.Empty;
                if (infoReader[5] != System.DBNull.Value) dateTime = infoReader.GetDateTime(5); else dateTime = null;
                if (infoReader[6] != System.DBNull.Value) gender = Char.Parse(infoReader.GetString(6).Substring(0, 1)); else gender = '\0';

                infoReader.Close();
                infoCommad.Dispose();
                dbConn.Close();
            }

            this.customer = new Customer(customerID, firstName, lastName, phone, email, dateTime, gender);
        }

        private void GetNotes()
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
                        "ΠροέκυψεΠπρόβλημα",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.OK);

                    return;
                }

                string query = "SELECT NoteID, Note FROM dbo.Notes WHERE CustomerID = @ID ORDER BY NoteID DESC";

                SqlCommand command = new SqlCommand(query, dbConn);
                command.Parameters.Add("@ID", System.Data.SqlDbType.Int);
                command.Parameters["@ID"].Value = this.customer.CustomerID;

                SqlDataReader dataReader = command.ExecuteReader();

                if (Notes == null)
                {
                    Notes = new ObservableCollection<TextBox>();
                    NotesListView.ItemsSource = Notes;
                }

                int i = 0;
                while (dataReader.Read())
                {
                    TextBox noteTB = new TextBox();
                    noteTB.Text = dataReader.GetString(1);
                    Notes.Add(noteTB);
                    NotesID.Add(i, dataReader.GetInt32(0));
                    i++;
                }

                command.Dispose();
                dataReader.Close();
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
            if (NewNoteTB.Text.Equals(string.Empty) || NewNoteTB.Text.Trim().Equals(string.Empty)) {
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

                string query = "INSERT INTO dbo.Notes (CustomerID, Note) VALUES (@ID,  @Note)";

                SqlCommand command = new SqlCommand(query, dbConn);
                command.Parameters.Add("@ID", System.Data.SqlDbType.Int);
                command.Parameters.Add("@Note", System.Data.SqlDbType.NVarChar);
                command.Parameters["@ID"].Value = this.customer.CustomerID;
                command.Parameters["@Note"].Value = NewNoteTB.Text;

                SqlDataAdapter adapter = new SqlDataAdapter();

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

                TextBox noteTB = new TextBox();
                noteTB.Text = NewNoteTB.Text;
                Notes.Add(noteTB);

                

                SaveNoteBtn.IsEnabled = false;
                NewNoteTB.Text = "Νέα σημείωση";
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            int s = NotesListView.SelectedIndex;
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

                string query = "DELETE dbo.Notes WHERE NoteID = @ID";

                SqlCommand command = new SqlCommand(query, dbConn);

                command.Parameters.Add("@ID", System.Data.SqlDbType.Int);
                command.Parameters["@ID"].Value = NotesID[NotesListView.SelectedIndex];

                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                dataAdapter.DeleteCommand = command;
                dataAdapter.DeleteCommand.ExecuteNonQuery();

                command.Dispose();
                dataAdapter.Dispose();
                dbConn.Close();

                Notes.RemoveAt(NotesListView.SelectedIndex);
                NotesID.Remove(NotesListView.SelectedIndex);

            }
        }
    }
}
