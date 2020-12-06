using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Data.SqlClient;
using System.Collections.ObjectModel;
using System.Threading;
using System.IO;
using Microsoft.Win32;

namespace Salon_App_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private StackPanel currentBtn;
        private string connStr;
        private SqlConnection dbConn;
        private short keysPressed = 0;
        private UserControl openedControl = null;
        private static MainWindow mainWindow;
        private Logger logger;
        private ObservableCollection<Result> results { get; set; }

        private IDictionary<int, int> customerIDs = new Dictionary<int, int>();

        public static string ConnStr { get { return mainWindow.connStr; } } 

        public MainWindow()
        {
            InitializeComponent();

            logger = new Logger();
            logger.Section("MainWindow: Default Constructor");

            mainWindow = this;
            results = new ObservableCollection<Result>();

            connStr = Properties.DefaultSettings.Default.DBConnStr.Replace("Path", GetDBPath());

            try
            {
                logger.Log("Connecting to DB");
                dbConn = new SqlConnection(connStr);
                dbConn.Open();
            }
            catch (SqlException ex)
            {
                logger.Log("Error while connecting to DB.");
                logger.Log(ex.ToString());

                MessageBox.Show("Παρουσιάστηκε πρόβλημα κατά στη σύνδεση. Παρακαλούμε επικοινωνήστε με το τεχνικό τμήμα.",
                    "Προέκυψε πρόβλημα",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK);
            }

            // Create a backup at the start of the app
            BackUpManager BUManager = new BackUpManager();

            // Use a separate thread so GUI won't hang
            new Thread(BUManager.Initialize).Start();

            OpenUserControl(new HomeControl());

        }


        private string GetDBPath()
        {
            // The first time this property will be empty
            // because we don't know the username of current user
            // on this machine
            if (Properties.Settings.Default.DBPath.Equals(String.Empty))
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                Properties.Settings.Default.DBPath = Path.Combine(appData, "Salon Management", "Resources", "SalonDB.mdf");
            }

            return Properties.Settings.Default.DBPath;
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

        private void HomeBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (openedControl.GetType() != typeof(HomeControl))
            {
                ActivateButton(sender, new SolidColorBrush(Color.FromRgb(249, 46, 151)));

                OpenUserControl(new HomeControl());
            }
        }

        private void customersBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ActivateButton(sender, new SolidColorBrush(Color.FromRgb(53, 249, 26)));

            OpenUserControl(new CustomersBaseControl());
        }

        private void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox searchTextBox = (TextBox)sender;
            keysPressed++;

            string trimmedText = searchTextBox.Text.Trim();
            // If two keys were pressed search for the given text
            // and also there are containt in it
            if (keysPressed == 2 && !trimmedText.Equals(string.Empty))
            {
                SqlCommand command;
                string query = "SELECT CustomerID, FirstName, LastName, NickName FROM dbo.Customers WHERE";
                // If text containes two words, search for first name and last name
                if (trimmedText.Contains(" "))
                {
                    string[] strs = trimmedText.Split(' ');
                    query += " (FirstName LIKE @Name AND LastName LIKE @LName) OR (FirstName LIKE @LName AND LastName LIKE @Name)";

                    command = new SqlCommand(query, dbConn);

                    command.Parameters.AddWithValue("@Name", System.Data.SqlDbType.NVarChar);
                    command.Parameters.AddWithValue("@LName", System.Data.SqlDbType.NVarChar);
                    command.Parameters["@Name"].Value = strs[0] + "%";
                    command.Parameters["@LName"].Value = strs[1] + "%";
                }
                else
                {
                    query += " FirstName LIKE @Name OR LastName LIKE @Name";

                    command = new SqlCommand(query, dbConn);
                    command.Parameters.Add("@Name", System.Data.SqlDbType.NVarChar);
                    command.Parameters["@Name"].Value = searchTextBox.Text + "%";
                }

                // Prepare for next search
                keysPressed = 0;

                logger.Section("Customer search");
                logger.Log("Executing query");
                SqlDataReader dataReader = command.ExecuteReader();

                if (results.Count > 0) results.Clear();
                customerIDs.Clear();

                if (dataReader.HasRows) SearchResults.IsOpen = true;
                else SearchResults.IsOpen = false;

                logger.Log("Reading fetched data.");
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
                logger.Log("Reading complete.");

                SearchResultsGrid.ItemsSource = results;

                dataReader.Close();
                command.Dispose();
            }
            // To not show popup indefinitely
            else if (keysPressed == 4 || (trimmedText.Equals(string.Empty)))
            {
                keysPressed = 0;
                if (results.Count > 0) results.Clear();
                customerIDs.Clear();
                SearchResults.IsOpen = false;
            }

        }

        private void SearchResultsGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // To prevent program to hang if user right clicks in wrong area
            if (SearchResultsGrid.SelectedIndex > -1)
            {
                Result selection = (Result)SearchResultsGrid.SelectedItem;

                String query = "SELECT * FROM dbo.Customers WHERE CustomerID = @ID";

                SqlCommand sqlCmd = new SqlCommand(query, dbConn);
                sqlCmd.Parameters.Add("@ID", System.Data.SqlDbType.Int);
                sqlCmd.Parameters["@ID"].Value = selection.ID;

                logger.Section("Customer choose");
                logger.Log("Executing query");

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

                // Show detailes for selected customer
                OpenUserControl(new CustomerControl(new Customer(customerID, firstName, lastName, nickName, phone, email, dateTime, gender)));
            }
        }

        private void NewRecordLeft_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenUserControl(new CustomerControl());
        }

        public static void OpenUserControl(UserControl userControl)
        {
            mainWindow.logger.Section("MainWindow: OpenUserControl");
            mainWindow.logger.Log("Opening user control: " + userControl);

            // Close previous opened user control
            if (mainWindow.openedControl != null) CloseUserControl();

            mainWindow.openedControl = userControl;

            mainWindow.formsGrid.Children.Add(mainWindow.openedControl);

            mainWindow.logger.Log("User control opened.");
        }

        public static void CloseUserControl()
        {
            mainWindow.logger.Section("User Control");
            mainWindow.logger.Log("Closing user control.");

            mainWindow.formsGrid.Children.Remove(mainWindow.openedControl);
            mainWindow.openedControl = null;

            mainWindow.logger.Log("User control closed.");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            logger.Section("Window");
            logger.Log("Window closing");

            // Backup any new records added after the app has started
            BackUpManager BUManager = new BackUpManager();
            Thread t = new Thread(BUManager.Initialize);
            t.Start();
            t.Join();

            Properties.Settings.Default.Save();
            logger.Log("Window close");
        }

        private void WindowClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void ExportDBBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            logger.Section("DB export");

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

                logger.Log("Exporting DB.");
                new Thread(export.Export).Start();
            }
        }

        private void ConfigurationBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            logger.Section("MainWindow: Configuration");
            if (!ConfigurationPopup.IsOpen)
            {
                // Get the name of the opened userControl to show
                // the appropriate configurations
                string userControl = string.Empty;
                if (mainWindow.openedControl != null)
                {
                    userControl = mainWindow.openedControl.ToString();
                    int lastIndex = userControl.LastIndexOf('.') + 1;
                    userControl = userControl.Substring(lastIndex, userControl.Length - lastIndex);
                }

                logger.Log("Opening configuration control.");
                ConfigurationPopup.Child = new ConfigurationControl(userControl);
                ConfigurationPopup.IsOpen = true;
            }
            else
            {
                ConfigurationPopupClose();
            }

        }

        public static void ConfigurationPopupClose()
        {
            mainWindow.logger.Section("MainWindow: Configuration");
            mainWindow.logger.Log("Closing configuration control");

            mainWindow.ConfigurationPopup.Child = null;
            mainWindow.ConfigurationPopup.IsOpen = false;
        }

        private void WindowMinimize_MouseDown(object sender, MouseButtonEventArgs e)
        {
            logger.Section("MainWindow: Window Controls");
            logger.Log("Minimizing window");

            mainWindow.WindowState = WindowState.Minimized;
        }

        private void WindowMaximize_MouseDown(object sender, MouseButtonEventArgs e)
        {
            double left = SystemParameters.WorkArea.Left;
            double top = SystemParameters.WorkArea.Top;
            double height = SystemParameters.WorkArea.Height;
            double width = SystemParameters.WorkArea.Width;

            // Maximize window to working area
            if (this.Left != left || this.Top != top || this.Height != height || this.Width != width)
            {
                Properties.Settings.Default.WindowTop = this.Top.ToString();
                Properties.Settings.Default.WindowLeft = this.Left.ToString();
                Properties.Settings.Default.WindowHeight = this.Height.ToString();
                Properties.Settings.Default.WindowWidth = this.Width.ToString();

                this.Top = top;
                this.Left = left;
                this.Height = height;
                this.Width = width;
            }
            // Restore window to previous state
            else
            {
                this.Top = Double.Parse(Properties.Settings.Default.WindowTop);
                this.Left = Double.Parse(Properties.Settings.Default.WindowLeft);
                this.Height = Double.Parse(Properties.Settings.Default.WindowHeight);
                this.Width = Double.Parse(Properties.Settings.Default.WindowWidth);
            }
        }

        private void InfoBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (InfoPopup.IsOpen) InfoPopup.IsOpen = false;
            else InfoPopup.IsOpen = true;

            StringBuilder message = new StringBuilder();
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            message.Append("Διαδρομή αχείου βάσης δεδομένων: \"").Append(Path.Combine(appData, "Salon Management", "Resources", "SalonBB.mdf")).Append("\"").Append(Environment.NewLine);
            message.Append("Διαδρομή αντιγράφων ασφαλείας: \"").Append(Path.Combine(appData, "Salon Management", "Backup")).Append("\"").Append(Environment.NewLine);
            message.Append("Διαδρομή αρχείων καταγραφής (.log): \"").Append(Path.Combine(appData, "Salon Management", "Resources")).Append("\"").Append(Environment.NewLine);
            message.Append(Environment.NewLine).Append("Τα ημερήσια αντίγραφα έχουν ισχύς 7 ημερολογιακές ημέρες από την ημέρα δημιουργίας τους.").Append(Environment.NewLine);
            message.Append("Τα μηνιαία αντίγραφα ασφαλείας έχουν ισχύς 90 ημέρες από την ημέρα δημιουργίας τους.");

            InfoTextBox.Text = message.ToString();
        }

        private void InfoTextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            InfoPopup.IsOpen = false;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Save new window location and size
            Properties.Settings.Default.WindowTop = this.Top.ToString();
            Properties.Settings.Default.WindowLeft = this.Left.ToString();
            Properties.Settings.Default.WindowHeight = this.Height.ToString();
            Properties.Settings.Default.WindowWidth = this.Width.ToString();
        }
    }

    // Helping class for search results
    // Used as ItemSource for DataGrid
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
