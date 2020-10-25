
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
using MaterialDesignThemes;
using Xceed.Wpf.Toolkit;

namespace Salon_App_WPF
{
    /// <summary>
    /// Interaction logic for ConfigurationControl.xaml
    /// </summary>
    public partial class ConfigurationControl : UserControl
    {
        private Dictionary<string, string> previousValues;
        private Dictionary<string, string> labels;
        private Dictionary<string, string> defaultValues;
        private List<string> changedSettings;
        private Dictionary<string, ColorPicker> colorPickers;
        private Slider opacity;

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
            previousValues.Add("ImageBorder", Properties.Settings.Default.ImageBorder);
            previousValues.Add("UserFormIcon", Properties.Settings.Default.UserFormIcon);
            previousValues.Add("GridBackground", Properties.Settings.Default.GridBackground);
            previousValues.Add("HomeOpacity", Properties.Settings.Default.HomeOpacity.Replace('.', ','));
            previousValues.Add("HomeText", Properties.Settings.Default.HomeText);

            defaultValues = new Dictionary<string, string>();
            defaultValues.Add("TopGrid", Properties.DefaultSettings.Default.TopGrid);
            defaultValues.Add("SideMenu", Properties.DefaultSettings.Default.SideMenu);
            defaultValues.Add("FormsGrid", Properties.DefaultSettings.Default.FormsGrid);
            defaultValues.Add("MouseOver", Properties.DefaultSettings.Default.MouseOver);
            defaultValues.Add("MainWindowIcon", Properties.DefaultSettings.Default.MainWindowIcon);
            defaultValues.Add("OverIcon", Properties.DefaultSettings.Default.OverIcon);
            defaultValues.Add("MainWindowText", Properties.DefaultSettings.Default.MainWindowText);
            defaultValues.Add("Text", Properties.DefaultSettings.Default.Text);
            defaultValues.Add("ImageBorder", Properties.DefaultSettings.Default.ImageBorder);
            defaultValues.Add("UserFormIcon", Properties.DefaultSettings.Default.UserFormIcon);
            defaultValues.Add("GridBackground", Properties.DefaultSettings.Default.GridBackground);
            defaultValues.Add("HomeOpacity", Properties.DefaultSettings.Default.HomeOpacity.Replace('.', ','));
            defaultValues.Add("HomeText", Properties.DefaultSettings.Default.HomeText);


            List<string> names = new List<string>();

            labels = new Dictionary<string, string>();
            labels.Add("TopGrid", "Επάνω μέρος");
            labels.Add("SideMenu", "Αριστερό μενού");
            labels.Add("FormsGrid", "Παράθυρο φορμών");
            labels.Add("MouseOver", "Ποντίκι σε αντικείμενο");
            labels.Add("MainWindowIcon", "Εικονίδια");
            labels.Add("OverIcon", "Ποντίκι σε εικονίδιο");
            labels.Add("MainWindowText", "Κείμενο");

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
                        labels.Add("Text", "Κείμενο φόφμας");
                        labels.Add("ImageBorder", "Περίγραμμα εικόνας");
                        labels.Add("UserFormIcon", "Εικονίδια φόρμας");

                        names.Add("Text");
                        names.Add("ImageBorder");
                        names.Add("UserFormIcon");
                        break;
                    }
                case "CustomersBaseControl":
                    {

                        break;
                    }
                case "HomeControl":
                    {
                        labels.Add("HomeOpacity", "Διαφάνεια λογότυπου");
                        labels.Add("HomeText", "Ώρα = Ημ/νια");

                        names.Add("HomeOpacity");
                        names.Add("HomeText");
                        break;
                    }
            }

            Style style = new Style(typeof(TextBlock));
            style.Setters.Add(new Setter(TextBlock.FontSizeProperty, 15.0));
            style.Setters.Add(new Setter(TextBlock.BackgroundProperty, Brushes.Transparent));
            style.Setters.Add(new Setter(TextBlock.ForegroundProperty, Brushes.Gainsboro));

            colorPickers = new Dictionary<string, ColorPicker>();
            for (int i = 0; i < names.Count(); i++)
            {
               
                TextBlock tb = new TextBlock();
                tb.Text = labels[names[i]];
                tb.Style = style;
                Controls.Children.Add(tb);

                if (!names[i].Equals("HomeOpacity"))
                {
                    colorPickers.Add(names[i], new ColorPicker());
                    colorPickers[names[i]].Name = names[i] + "CP";
                    colorPickers[names[i]].SelectedColor = (System.Windows.Media.Color)ColorConverter.ConvertFromString(previousValues[names[i]]);
                    colorPickers[names[i]].SelectedColorChanged += SelectedColorChanged;

                    Controls.Children.Add(colorPickers[names[i]]);
                }
                else
                {
                    opacity = new Slider();
                    opacity.Minimum = Double.Parse("0,0");
                    opacity.Maximum = Double.Parse("1,0"); ;
                    opacity.TickFrequency = Double.Parse("0,1"); ;
                    opacity.Value = Double.Parse(Properties.Settings.Default.HomeOpacity.Replace('.', ','));
                    opacity.ValueChanged += Slider_ValueChanged;

                    Controls.Children.Add(opacity);

                }
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
                case ("HomeTextCP"):
                    {
                        changedSettings.Add("HomeText");
                        Properties.Settings.Default.HomeText = cp.SelectedColor.ToString();
                        break;
                    }
            }
        }

        private void CanceBtn_Click(object sender, RoutedEventArgs e)
        {

            if (opacity != null)
            {
                Properties.Settings.Default.HomeOpacity = previousValues["HomeOpacity"];
                opacity.Value = Double.Parse(Properties.Settings.Default.HomeOpacity);
            }

            foreach (string propertie in changedSettings.ToList())
            {
                colorPickers[propertie].SelectedColor = (Color)ColorConverter.ConvertFromString(previousValues[propertie]);

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
                    case ("HomeOpacity"):
                        {
                            Properties.Settings.Default.HomeOpacity = previousValues[propertie];
                            break;
                        }
                    case ("HomeText"):
                        {
                            Properties.Settings.Default.HomeText = previousValues[propertie];
                            break;
                        }
                }
            }

            changedSettings.Clear();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider opacity = (Slider)sender;

            string s = string.Format("{0:0.00}", opacity.Value).Replace(',', '.');

            Properties.Settings.Default.HomeOpacity = s;
        }

        private void DefaultBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (string propertie in changedSettings.ToList())
            {
                colorPickers[propertie].SelectedColor = (Color)ColorConverter.ConvertFromString(defaultValues[propertie]);
            }

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
            Properties.Settings.Default.HomeOpacity = Properties.DefaultSettings.Default.HomeOpacity;
            Properties.Settings.Default.HomeText = Properties.DefaultSettings.Default.HomeText;
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ConfigurationPopupClose();
        }
    }
}
