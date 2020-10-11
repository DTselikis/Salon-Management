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

namespace Salon_App_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private StackPanel currentBtn;

        public MainWindow()
        {
            InitializeComponent();
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

            CustomerControl customerControl = new CustomerControl();
            this.formsGrid.Children.Add(customerControl);
        }

        private void customersBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ActivateButton(sender, new SolidColorBrush(Color.FromRgb(53, 249, 26)));
        }
    }
}
