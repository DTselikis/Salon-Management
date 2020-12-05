
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
        private List<string> changedSettings;
        private Dictionary<string, ColorPicker> colorPickers;
        private Slider opacity;

        public ConfigurationControl(string activeUserContol)
        {
            InitializeComponent();
            previousValues = new Dictionary<string, string>();

            foreach (SettingsProperty currentProperty in Properties.Settings.Default.Properties)
            {
                // currentProperty.DefaultValue will always returns the default value
                // even if the value has changed
                previousValues.Add(currentProperty.Name, Properties.Settings.Default[currentProperty.Name].ToString());
            }
            previousValues["HomeOpacity"] = previousValues["HomeOpacity"].Replace('.', ',');

            List<string> clrPckrsNames = new List<string>();

            labels = new Dictionary<string, string>();
            labels.Add("TopGrid", "Επάνω μέρος");
            labels.Add("SideMenu", "Αριστερό μενού");
            labels.Add("FormsGrid", "Παράθυρο φορμών");
            labels.Add("MouseOver", "Ποντίκι σε αντικείμενο");
            labels.Add("MainWindowIcon", "Εικονίδια");
            labels.Add("OverIcon", "Ποντίκι σε εικονίδιο");
            labels.Add("MainWindowText", "Κείμενο");

            clrPckrsNames.Add("TopGrid");
            clrPckrsNames.Add("SideMenu");
            clrPckrsNames.Add("FormsGrid");
            clrPckrsNames.Add("MouseOver");
            clrPckrsNames.Add("MainWindowIcon");
            clrPckrsNames.Add("OverIcon");
            clrPckrsNames.Add("MainWindowText");

            switch (activeUserContol)
            {
                case "CustomerControl":
                    {
                        labels.Add("Text", "Κείμενο φόφμας");
                        labels.Add("ImageBorder", "Περίγραμμα εικόνας");
                        labels.Add("UserFormIcon", "Εικονίδια φόρμας");

                        clrPckrsNames.Add("Text");
                        clrPckrsNames.Add("ImageBorder");
                        clrPckrsNames.Add("UserFormIcon");
                        break;
                    }
                case "CustomersBaseControl":
                    {

                        break;
                    }
                case "HomeControl":
                    {
                        labels.Add("HomeOpacity", "Διαφάνεια λογότυπου");
                        labels.Add("HomeText", "Ώρα - Ημ/νία");

                        clrPckrsNames.Add("HomeText");
                        break;
                    }
            }

            Style style = new Style(typeof(TextBlock));
            style.Setters.Add(new Setter(TextBlock.FontSizeProperty, 15.0));
            style.Setters.Add(new Setter(TextBlock.BackgroundProperty, Brushes.Transparent));
            style.Setters.Add(new Setter(TextBlock.ForegroundProperty, Brushes.Gainsboro));

            colorPickers = new Dictionary<string, ColorPicker>();
            for (int i = 0; i < clrPckrsNames.Count(); i++)
            {
               
                TextBlock tb = new TextBlock();
                tb.Text = labels[clrPckrsNames[i]];
                tb.Style = style;
                Controls.Children.Add(tb);

                colorPickers.Add(clrPckrsNames[i], new ColorPicker());
                colorPickers[clrPckrsNames[i]].Name = clrPckrsNames[i];
                colorPickers[clrPckrsNames[i]].SelectedColor = (System.Windows.Media.Color)ColorConverter.ConvertFromString(previousValues[clrPckrsNames[i]]);
                colorPickers[clrPckrsNames[i]].SelectedColorChanged += SelectedColorChanged;

                Controls.Children.Add(colorPickers[clrPckrsNames[i]]);
            }

            if (labels.ContainsKey("HomeOpacity"))
            {
                opacity = new Slider();
                opacity.Minimum = Double.Parse("0,0");
                opacity.Maximum = Double.Parse("1,0"); ;
                opacity.TickFrequency = Double.Parse("0,1"); ;
                opacity.Value = Double.Parse(previousValues["HomeOpacity"]);
                opacity.ValueChanged += Slider_ValueChanged;

                TextBlock tb = new TextBlock();
                tb.Text = labels["HomeOpacity"];
                tb.Style = style;
                Controls.Children.Add(tb);

                Controls.Children.Add(opacity);
            }

            changedSettings = new List<string>();

        }

        private void SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            ColorPicker cp = (ColorPicker)sender;

            Properties.Settings.Default[cp.Name] = cp.SelectedColor.ToString();
            changedSettings.Add(cp.Name);
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (string propertie in changedSettings.ToList())
            {
                try
                {
                    colorPickers[propertie].SelectedColor = (Color)ColorConverter.ConvertFromString(previousValues[propertie]);
                    Properties.Settings.Default[propertie] = previousValues[propertie];
                }
                catch(KeyNotFoundException ex)
                {
                    opacity.Value = Double.Parse(previousValues[propertie]);
                }
            }

            changedSettings.Clear();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            string s = string.Format("{0:0.00}", opacity.Value).Replace(',', '.');

            Properties.Settings.Default.HomeOpacity = s;

            if (!changedSettings.Contains("HomeOpacity"))
            {
                changedSettings.Add("HomeOpacity");
            }
        }

        private void DefaultBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (string property in changedSettings.ToList())
            {
                try
                {
                    colorPickers[property].SelectedColor = (Color)ColorConverter.ConvertFromString(Properties.DefaultSettings.Default[property].ToString());
                    Properties.Settings.Default[property] = Properties.DefaultSettings.Default[property];
                }
                catch (KeyNotFoundException ex)
                {
                    opacity.Value = Double.Parse(Properties.Settings.Default.HomeOpacity);
                }
            }
        }

        private void DefaultAllBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (SettingsProperty property in Properties.DefaultSettings.Default.Properties)
            {
                try
                {
                    colorPickers[property.Name].SelectedColor = (Color)ColorConverter.ConvertFromString(property.DefaultValue.ToString());
                    Properties.Settings.Default[property.Name] = Properties.DefaultSettings.Default[property.Name];
                }
                catch (KeyNotFoundException ex)
                {
                    opacity.Value = Double.Parse(Properties.DefaultSettings.Default.HomeOpacity.Replace('.', ','));
                }
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
            MainWindow.ConfigurationPopupClose();
        }

       
    }
}
