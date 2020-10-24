using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
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
using System.Data.SqlClient;
using System.Windows.Controls.Primitives;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Data;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace Salon_App_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private StackPanel currentBtn;
        private string connStr = Properties.Settings.Default.DBConnStr;
        private SqlConnection dbConn;
        private short keysPressed = 0;
        private UserControl openedControl = null;
        private static MainWindow mainWindow;
        private ObservableCollection<Result> results{ get; set; }

        private IDictionary<int, int> customerIDs = new Dictionary<int, int>();


        public MainWindow()
        {
            InitializeComponent();
            mainWindow = this;
            results = new ObservableCollection<Result>();

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

            InsertRecords();
        }

        private void ActivateButton(object senderBtn, SolidColorBrush color)
        {
            if (senderBtn != null)
            {
                // Disable previous button before enable current
                DisableButton();

                // Get StackPanel
                currentBtn = (StackPanel)senderBtn;
                currentBtn.Background = new SolidColorBrush(Color.FromRgb(65, 66, 94));

                // Apply color to the left of the selected button
                Grid btnLeftBorder = (Grid)currentBtn.Children[0];
                btnLeftBorder.Background = color;

                //Change the color of the icon and the text of the button
                IconBlock btnIcon = (IconBlock)currentBtn.Children[1];
                btnIcon.Foreground = color;

                TextBlock btnText = (TextBlock)currentBtn.Children[2];
                btnText.Foreground = color;
            }
        }

        private void DisableButton()
        {
            if (currentBtn != null)
            {
                // Reset values to default
                currentBtn.Background = new SolidColorBrush(Color.FromRgb(48, 49, 69));

                Grid btnLeftBorder = (Grid)currentBtn.Children[0];
                btnLeftBorder.Background = new SolidColorBrush(Colors.Transparent);

                IconBlock btnIcon = (IconBlock)currentBtn.Children[1];
                btnIcon.Foreground = new SolidColorBrush(Colors.Gainsboro);

                TextBlock btnText = (TextBlock)currentBtn.Children[2];
                btnText.Foreground = new SolidColorBrush(Colors.Gainsboro);
            }
        }

        private void customerBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ActivateButton(sender, new SolidColorBrush(Color.FromRgb(249, 46, 151)));
        }

        private void customersBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ActivateButton(sender, new SolidColorBrush(Color.FromRgb(53, 249, 26)));

            OpenUserControl(new CustomersBaseControl());
        }

        private void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            keysPressed++;

            // If two keys were pressed search for the ginen text
            if (keysPressed >= 2)
            {
                keysPressed = 0;

                TextBox searchTextBox = (TextBox)sender;

                String query = "SELECT CustomerID, FirstName,  LastName, NickName FROM dbo.Customers WHERE FirstName LIKE @Name OR LastName LIKE @Name";
                
                SqlCommand selectCommand = new SqlCommand(query, dbConn);

                selectCommand.Parameters.Add("@Name", System.Data.SqlDbType.NVarChar);
                selectCommand.Parameters["@Name"].Value = searchTextBox.Text + "%";

                SqlDataReader dataReader = selectCommand.ExecuteReader();

                if (results.Count > 0) results.Clear();
                customerIDs.Clear();

                SearchResults.IsOpen = true;

                while (dataReader.Read())
                {
                    int id;
                    string firstName;
                    string lastName;
                    string nickName;

                    id = dataReader.GetInt32(0);
                    if (dataReader[1] != System.DBNull.Value) firstName = dataReader.GetString(1); else firstName = String.Empty;
                    if (dataReader[2] != System.DBNull.Value) lastName = dataReader.GetString(2); else lastName = String.Empty;
                    if (dataReader[3] != System.DBNull.Value) nickName = dataReader.GetString(3); else nickName = String.Empty;

                    results.Add(new Result(id, firstName, lastName, nickName));
                }

                SearchResultsGrid.ItemsSource = results;

                dataReader.Close();
                selectCommand.Dispose();
            }

        }

        private void SearchResultsGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Result selection = (Result)SearchResultsGrid.SelectedItem;

            String query = "SELECT * FROM dbo.Customers WHERE CustomerID = @ID";

            SqlCommand sqlCmd = new SqlCommand(query, dbConn);
            sqlCmd.Parameters.Add("@ID", System.Data.SqlDbType.Int);
            sqlCmd.Parameters["@ID"].Value = selection.ID;

            SqlDataReader dataReader = sqlCmd.ExecuteReader();

            dataReader.Read();

            int customerID;
            string firstName;
            string lastName;
            string nickName;
            string phone;
            string email;
            Nullable<DateTime> dateTime = null;
            char gender;

            customerID = dataReader.GetInt32(0);
            if (dataReader[1] != System.DBNull.Value) firstName = dataReader.GetString(1); else firstName = String.Empty;
            if (dataReader[2] != System.DBNull.Value) lastName = dataReader.GetString(2); else lastName = String.Empty;
            if (dataReader[3] != System.DBNull.Value) nickName = dataReader.GetString(3); else nickName = String.Empty;
            if (dataReader[4] != System.DBNull.Value) phone = dataReader.GetString(4); else phone = String.Empty;
            if (dataReader[5] != System.DBNull.Value) email = dataReader.GetString(5); else email = String.Empty;
            if (dataReader[6] != System.DBNull.Value) dateTime = dataReader.GetDateTime(6); else dateTime = null;
            if (dataReader[7] != System.DBNull.Value) gender = Char.Parse(dataReader.GetString(7).Substring(0, 1)); else gender = '\0';

            // Clear results and close popup
            SearchResults.IsOpen = false;
            results.Clear();
            dataReader.Close();

            OpenUserControl(new CustomerControl(new Customer(customerID, firstName, lastName, nickName, phone, email, dateTime, gender)));
        }

        private void InsertRecords()
        {
            string query = $"INSERT INTO dbo.Customers(FirstName, LastName, Nickname, Phone, Email, FirstVisit, Gender) VALUES ('Jim', 'Lk', NULL, NULL, NULL, NULL, NULL), ('maria', 'Lek', NULL, NULL, NULL, NULL, NULL), ('maria', 'Lk', NULL, NULL, NULL, NULL, NULL), ('John', 'Papas', NULL, NULL, NULL, NULL, NULL)";
            SqlCommand command = new SqlCommand(query, dbConn);

            command.ExecuteNonQuery();

            command.Dispose();

            string query2 = "INSERT INTO dbo.Notes(CustomerID, Note, CreationDate) VALUES (@ID, @Note, @Date), (@ID, @Note2, @Date)";
            SqlCommand com2 = new SqlCommand(query2, dbConn);
            com2.Parameters.AddWithValue("@ID", System.Data.SqlDbType.Int);
            com2.Parameters.AddWithValue("@Note", System.Data.SqlDbType.NVarChar);
            com2.Parameters.AddWithValue("@Note2", System.Data.SqlDbType.NVarChar);
            com2.Parameters.AddWithValue("@Date", System.Data.SqlDbType.DateTime);

            com2.Parameters["@ID"].Value = 4;
            com2.Parameters["@Note"].Value = "Some text";
            com2.Parameters["@Note2"].Value = "Some other text";
            com2.Parameters["@Date"].Value = DateTime.Now;

            com2.ExecuteNonQuery();
            com2.Dispose();

        }

        private void NewRecordLeft_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenUserControl(new CustomerControl());
        }

        private void OpenUserControl(UserControl userControl)
        {
            if (this.openedControl != null) CloseUserControl();

            openedControl = userControl;

            this.formsGrid.Children.Add(openedControl);
        }

        public static void CloseUserControl()
        {
            mainWindow.formsGrid.Children.Remove(mainWindow.openedControl);
        }

        private void ExportDBBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Title = "Επιλογή τοποθεσίας αποθήκευσης";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openFileDialog.ValidateNames = false;
            openFileDialog.CheckFileExists = false;
            openFileDialog.CheckPathExists = true;
            openFileDialog.FileName = "backup";

            if (openFileDialog.ShowDialog() == true)
            {
                string path = openFileDialog.FileName.Substring(0, openFileDialog.FileName.LastIndexOf('\\'));

                BackUpManager export = new BackUpManager(path);

                new Thread(export.Export).Start();
            }
        }

        private void ConfigurationBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string userControl = string.Empty;
            if (mainWindow.openedControl != null)
            {
                userControl = mainWindow.openedControl.ToString();
                int lastIndex = userControl.LastIndexOf('.') + 1;
                userControl = userControl.Substring(lastIndex, userControl.Length - lastIndex);
            }

            ConfigurationPopup.Child = new ConfigurationControl(userControl);
            ConfigurationPopup.IsOpen = true;
             
        }

        public static void ConfigurationPopupClose()
        {
            mainWindow.ConfigurationPopup.Child = null;
            mainWindow.ConfigurationPopup.IsOpen = false;
        }

        private void WindowMinimize_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mainWindow.WindowState = WindowState.Minimized;
        }
    }

    public class Result
    {
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }

        public Result(int ID, string firstName, string lastName, string nickName)
        {
            this.ID = ID;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.NickName = nickName;
        }
    }
}
