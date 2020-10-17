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

        private IDictionary<int, int> customerIDs = new Dictionary<int, int>();


        public MainWindow()
        {
            InitializeComponent();

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
        }

        private void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            keysPressed++;

            // If two keys were pressed search for the ginen text
            if (keysPressed >= 2)
            {
                keysPressed = 0;

                Popup popup = this.SearchResults;
                TextBox searchTextBox = (TextBox)sender;

                String query = "SELECT CustomerID, FirstName, LastName FROM dbo.Customers WHERE FirstName LIKE @Name OR LastName LIKE @Name";
                
                SqlCommand selectCommand = new SqlCommand(query, dbConn);

                selectCommand.Parameters.Add("@Name", System.Data.SqlDbType.VarChar);
                selectCommand.Parameters["@Name"].Value = searchTextBox.Text + "%";

                SqlDataReader dataReader = selectCommand.ExecuteReader();

                // Clears results from previous search
                if (ResultsListBox.HasItems)
                {
                    ResultsListBox.Items.Clear();
                    customerIDs.Clear();
                }

                SearchResults.IsOpen = true;

                int i = 0;
                StringBuilder resultStr = new StringBuilder();
                while (dataReader.Read())
                {
                    customerIDs.Add(i, dataReader.GetInt32(0));
                    i++;

                    resultStr.Append(dataReader[1]).Append(" ").Append(dataReader[2]);
                    ResultsListBox.Items.Add(resultStr.ToString());
                    resultStr.Clear();
                }

                dataReader.Close();
                selectCommand.Dispose();
            }

        }

        private void ResultsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox resultLB = (ListBox)sender;
            MessageBox.Show(resultLB.SelectedIndex.ToString());
            String query = "SELECT * FROM dbo.Customers WHERE CustomerID = @ID";

            SqlCommand sqlCmd = new SqlCommand(query, dbConn);

            sqlCmd.Parameters.Add("@ID", System.Data.SqlDbType.Int);
            if (resultLB.SelectedIndex != -1)
            {


                sqlCmd.Parameters["@ID"].Value = customerIDs[resultLB.SelectedIndex];

                SqlDataReader dataReader = sqlCmd.ExecuteReader();

                dataReader.Read();

                string firstName;
                string lastName;
                string phone;
                string email;
                Nullable<DateTime> dateTime = null;
                char gender;

                if (dataReader[1] != System.DBNull.Value) firstName = dataReader.GetString(1); else firstName = String.Empty;
                if (dataReader[2] != System.DBNull.Value) lastName = dataReader.GetString(1); else lastName = String.Empty;
                if (dataReader[3] != System.DBNull.Value) phone = dataReader.GetString(1); else phone = String.Empty;
                if (dataReader[4] != System.DBNull.Value) email = dataReader.GetString(1); else email = String.Empty;
                if (dataReader[5] != System.DBNull.Value) dateTime = DateTime.Parse(dataReader.GetString(5));
                if (dataReader[6] != System.DBNull.Value) gender = Char.Parse(dataReader.GetString(6).Substring(0, 1)); else gender = '\0';

                CustomerControl customerControl = new CustomerControl(new Customer(firstName, lastName, phone, email, dateTime, gender));

                this.formsGrid.Children.Add(customerControl);

                // Clear results and close popup
                SearchResults.IsOpen = false;
                resultLB.Items.Clear();
            }

        }

        private void customersBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ActivateButton(sender, new SolidColorBrush(Color.FromRgb(53, 249, 26)));
        }
    }
}
