
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Documents.DocumentStructures;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;

namespace Salon_App_WPF
{
    /// <summary>
    /// Interaction logic for ConfigurationControl.xaml
    /// </summary>
    public partial class ConfigurationControl : UserControl
    {
        private Dictionary<string, string> previousValues;
        private List<string> changedSettings;

        public ConfigurationControl(string activeUserContol)
        {
            InitializeComponent();

            previousValues = new Dictionary<string, string>();
            previousValues.Add("TopGrid", Properties.Settings.Default.TopGrid);
            previousValues.Add("SideMenu", Properties.Settings.Default.SideMenu);
            previousValues.Add("FormsGrid", Properties.Settings.Default.FormsGrid);
            previousValues.Add("MouseOver", Properties.Settings.Default.MouseOver);
            previousValues.Add("MainWindowIcon", Properties.Settings.Default.MainWindowIcon);
            previousValues.Add("OverIcon", Properties.Settings.Default.OverIcon);
            previousValues.Add("MainWindowText", Properties.Settings.Default.MainWindowText);
            previousValues.Add("Text", Properties.Settings.Default.Text);
            previousValues.Add("UserFormIcon", Properties.Settings.Default.UserFormIcon);
            previousValues.Add("GridBackground", Properties.Settings.Default.GridBackground);

            List<string> names = new List<string>();
            string[] labels = { "Επάνω μέρος", "Αριστερό μενού", "Παράθυρο φορμών", "Ποντίκι σε αντικείμενο", "Εικονίδια", "Ποντίκι σε εικονίδιο", "Κείμενο", "Κείμενο μενού", "Περίγραμμα εικόνας", "Εικονίδια φόρμας" };

            names.Add("TopGrid");
            names.Add("SideMenu");
            names.Add("FormsGrid");
            names.Add("MouseOver");
            names.Add("MainWindowIcon");
            names.Add("OverIcon");
            names.Add("MainWindowText");

            switch (activeUserContol)
            {
                case "CustomerControl":
                    {
                        names.Add("TextTB");
                        names.Add("ImageBorder");
                        names.Add("UserFormIcon");
                        break;
                    }
                case "CustomersBaseControl":
                    {

                        break;
                    }
            }

            Style style = new Style(typeof(TextBlock));
            style.Setters.Add(new Setter(TextBlock.FontSizeProperty, 15.0));
            style.Setters.Add(new Setter(TextBlock.BackgroundProperty, Brushes.Transparent));
            style.Setters.Add(new Setter(TextBlock.ForegroundProperty, Brushes.Gainsboro));

            for (int i = 0; i < names.Count(); i++)
            {
                TextBlock tb = new TextBlock();
                tb.Text = labels[i];
                tb.Style = style;

                ColorPicker cp = new ColorPicker();
                cp.Name = names[i] + "CP";
                cp.SelectedColor = (System.Windows.Media.Color)ColorConverter.ConvertFromString(previousValues[names[i]]);
                cp.SelectedColorChanged += SelectedColorChanged;
               

                Controls.Children.Add(tb);
                Controls.Children.Add(cp);
            }

            changedSettings = new List<string>();

        }

        private void SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            ColorPicker cp = (ColorPicker)sender;

            switch (cp.Name)
            {
                case ("TopGridCP"):
                    {
                        changedSettings.Add("TopGrid");
                        Properties.Settings.Default.TopGrid = cp.SelectedColor.ToString();
                        break;
                    }
                case ("SideMenuCP"):
                    {
                        changedSettings.Add("SideMenu");
                        Properties.Settings.Default.SideMenu = cp.SelectedColor.ToString();
                        break;
                    }
                case ("FormsGridCP"):
                    {
                        changedSettings.Add("FormsGrid");
                        Properties.Settings.Default.FormsGrid = cp.SelectedColor.ToString();
                        break;
                    }
                case ("MouseOverCP"):
                    {
                        changedSettings.Add("MouseOver");
                        Properties.Settings.Default.MouseOver = cp.SelectedColor.ToString();
                        break;
                    }
                case ("MainWindowIconCP"):
                    {
                        changedSettings.Add("MainWindowIcon");
                        Properties.Settings.Default.MainWindowIcon = cp.SelectedColor.ToString();
                        break;
                    }
                case ("OverIconCP"):
                    {
                        changedSettings.Add("OverIcon");
                        Properties.Settings.Default.OverIcon = cp.SelectedColor.ToString();
                        break;
                    }
                case ("MainWindowTextCP"):
                    {
                        changedSettings.Add("MainWindowText");
                        Properties.Settings.Default.MainWindowText = cp.SelectedColor.ToString();
                        break;
                    }
                case ("TextCP"):
                    {
                        changedSettings.Add("Text");
                        Properties.Settings.Default.Text = cp.SelectedColor.ToString();
                        break;
                    }
                case ("UserFormIconCP"):
                    {
                        changedSettings.Add("UserFormIcon");
                        Properties.Settings.Default.UserFormIcon = cp.SelectedColor.ToString();
                        break;
                    }
                case ("GridBackgroundCP"):
                    {
                        changedSettings.Add("GridBackground");
                        Properties.Settings.Default.GridBackground = cp.SelectedColor.ToString();
                        break;
                    }
            }
        }

        private void CanceBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (string propertie in changedSettings)
            {
                switch (propertie)
                {
                    case ("TopGrid"):
                        {
                            Properties.Settings.Default.TopGrid = previousValues[propertie];
                            break;
                        }
                    case ("SideMenu"):
                        {
                            Properties.Settings.Default.SideMenu = previousValues[propertie];
                            break;
                        }
                    case ("FormsGrid"):
                        {
                            Properties.Settings.Default.FormsGrid = previousValues[propertie];
                            break;
                        }
                    case ("MouseOver"):
                        {
                            Properties.Settings.Default.MouseOver = previousValues[propertie];
                            break;
                        }
                    case ("MainWindowIcon"):
                        {
                            Properties.Settings.Default.MainWindowIcon = previousValues[propertie];
                            break;
                        }
                    case ("OverIcon"):
                        {
                            Properties.Settings.Default.OverIcon = previousValues[propertie];
                            break;
                        }
                    case ("MainWindowText"):
                        {
                            Properties.Settings.Default.MainWindowText = previousValues[propertie];
                            break;
                        }
                    case ("Text"):
                        {
                            Properties.Settings.Default.Text = previousValues[propertie];
                            break;
                        }
                    case ("UserFormIcon"):
                        {
                            Properties.Settings.Default.UserFormIcon = previousValues[propertie];
                            break;
                        }
                    case ("GridBackground"):
                        {
                            Properties.Settings.Default.GridBackground = previousValues[propertie];
                            break;
                        }
                }
            }

            changedSettings.Clear();
        }

        private void DefaultBtn_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.TopGrid = Properties.DefaultSettings.Default.TopGrid;
            Properties.Settings.Default.SideMenu = Properties.DefaultSettings.Default.SideMenu;
            Properties.Settings.Default.FormsGrid = Properties.DefaultSettings.Default.FormsGrid;
            Properties.Settings.Default.MouseOver = Properties.DefaultSettings.Default.MouseOver;
            Properties.Settings.Default.MainWindowIcon = Properties.DefaultSettings.Default.MainWindowIcon;
            Properties.Settings.Default.OverIcon = Properties.DefaultSettings.Default.OverIcon;
            Properties.Settings.Default.MainWindowText = Properties.DefaultSettings.Default.MainWindowText;
            Properties.Settings.Default.Text = Properties.DefaultSettings.Default.Text;
            Properties.Settings.Default.UserFormIcon = Properties.DefaultSettings.Default.UserFormIcon;
            Properties.Settings.Default.GridBackground = Properties.DefaultSettings.Default.GridBackground;
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ConfigurationPopupClose();
        }
    }
}
