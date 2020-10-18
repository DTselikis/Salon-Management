using System;
using System.Collections.Generic;
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

                string query = "SELECT FirstName, LastName, Phone, Email, FirstVisit, Gender FROM dbo.Customers;";

                DataTable data = new DataTable("Employee");
                SqlCommand command = new SqlCommand(query, dbConn);
                SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                dataAdapter.Fill(data);
                CustomersBase.ItemsSource = data.DefaultView;

                dataAdapter.Dispose();
                dbConn.Close();
            }
        }
    }
}
