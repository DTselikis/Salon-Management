
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
        private Dictionary<string, TimeAlignments> posAlignment;
        private Dictionary<TimeAlignments, string> alignmentPos;
        private List<string> changedSettings;
        private Dictionary<string, ColorPicker> colorPickers;
        private Slider opacity;
        private Slider timeSize;
        private ComboBox positionBox;
        private CheckBox timeCheckBox;

        private Logger logger;

        // Structure for keeping the horizontal and vertical alignment
        // of the time/date label. It's used as a key and value for dictionaries
        public struct TimeAlignments
        {
            public string hAlignment;
            public string vAlignment;
        }

        public ConfigurationControl(string activeUserContol)
        {
            logger = new Logger();

            logger.Section("ConfigurationControl: Constructor");

            InitializeComponent();
            previousValues = new Dictionary<string, string>();

            // Store previous values of all properties in case user change its mind
            // for the changes he made.
            foreach (SettingsProperty currentProperty in Properties.Settings.Default.Properties)
            {
                // currentProperty.DefaultValue will always returns the default value
                // even if the value has changed
                previousValues.Add(currentProperty.Name, Properties.Settings.Default[currentProperty.Name].ToString());
            }

            // Keep track of which and how many color pickers were generated
            List<string> clrPckrsNames = new List<string>();
            changedSettings = new List<string>();
            colorPickers = new Dictionary<string, ColorPicker>();

            // Generate label for each new control. Also helps
            // to deretmine which control will be generated
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

            // Additional controls based active user control
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
                        labels.Add("TimeDayEnabled", "Εμφάνιση Ώρας");
                        labels.Add("TimeHorizontalAlignment", "Θέση ώρας");
                        labels.Add("TimeTextSize", "Μέγεθος ώρας");

                        clrPckrsNames.Add("HomeText");
                        break;
                    }
            }

            Style style = new Style(typeof(TextBlock));
            style.Setters.Add(new Setter(TextBlock.FontSizeProperty, 15.0));
            style.Setters.Add(new Setter(TextBlock.BackgroundProperty, Brushes.Transparent));
            style.Setters.Add(new Setter(TextBlock.ForegroundProperty, Brushes.Gainsboro));
            
            // Generate each color picker
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

            // Generate additional controls based on active user control
            if (labels.ContainsKey("HomeOpacity"))
            {
                opacity = new Slider();
                opacity.Name = "HomeOpacity";
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

            if (labels.ContainsKey("TimeDayEnabled"))
            {
                timeCheckBox = new CheckBox();
                timeCheckBox.Name = "TimeDateChkBox";
                timeCheckBox.IsChecked = (previousValues["TimeDateEnabled"].Equals("Visible")) ? true : false;
                timeCheckBox.Click += TimeCheckBox_Clicked;

                TextBlock tb = new TextBlock();
                tb.Text = labels["TimeDayEnabled"];
                tb.Style = style;
                Controls.Children.Add(tb);

                Controls.Children.Add(timeCheckBox);
            }

            if (labels.ContainsKey("TimeHorizontalAlignment"))
            {
                TextBlock tb = new TextBlock();
                tb.Text = labels["TimeHorizontalAlignment"];
                tb.Style = style;
                Controls.Children.Add(tb);

                // Each position will be matched with a horizontal and a vertical alignment
                string[,] alignments = new string[9, 2]
                {
                    {"Left", "Top" },
                    {"Center", "Top" },
                    {"Right", "Top" },
                    {"Left", "Center" },
                    {"Center", "Center" },
                    {"Right", "Center" },
                    {"Left", "Bottom" },
                    {"Center", "Bottom" },
                    {"Right", "Bottom" }
                };

                List<string> positions = new List<string> { "Πάνω αριστερά", "Πάνω", "Πάνω δεξιά", "Μέση αριστερά", "Μέση", "Μέση δεξιά", "Κάτω αριστερά", "Κάτω", "Κάτω δεξιά" };

                // Provide a position name and get the horizontal and vertical alignment
                posAlignment = new Dictionary<string, TimeAlignments>();
                // Provide horizontal and vertical alignment and get position name
                alignmentPos = new Dictionary<TimeAlignments, string>();

                for (int i = 0; i < positions.Count; i++)
                {
                    TimeAlignments alignment = new TimeAlignments();
                    alignment.hAlignment = alignments[i, 0];
                    alignment.vAlignment = alignments[i, 1];

                    posAlignment.Add(positions.ElementAt(i), alignment);
                    alignmentPos.Add(alignment, positions.ElementAt(i));
                }

                positionBox = new ComboBox();
                positionBox.ItemsSource = positions;
                positionBox.IsEditable = true;
                positionBox.SelectionChanged += PosComboBox_SelectionChanged;
                positionBox.Text = alignmentPos[new TimeAlignments() { hAlignment = previousValues["TimeHorizontalAlignment"], vAlignment = previousValues["TimeVerticalAlignment"] }];

                Controls.Children.Add(positionBox);
            }

            if (labels.ContainsKey("TimeTextSize"))
            {
                TextBlock tb = new TextBlock();
                tb.Text = labels["TimeTextSize"];
                tb.Style = style;
                Controls.Children.Add(tb);

                timeSize = new Slider();
                timeSize.Name = "TimeTextSize";
                timeSize.Minimum = Double.Parse("0,0");
                timeSize.Maximum = Double.Parse("100,0"); ;
                timeSize.TickFrequency = Double.Parse("1,0"); ;
                timeSize.Value = Double.Parse(previousValues["TimeTextSize"]);
                timeSize.ValueChanged += Slider_ValueChanged;

                Controls.Children.Add(timeSize);
            }

        }

        private void SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            ColorPicker cp = (ColorPicker)sender;

            // Change property so it can be updated to the UI by binding
            Properties.Settings.Default[cp.Name] = cp.SelectedColor.ToString();
            changedSettings.Add(cp.Name);
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            logger.Section("ConfigurationControl: CancelBtn");

            // Restore the previous value for each change the user made
            foreach (string property in changedSettings.ToList())
            {
                try
                {
                    colorPickers[property].SelectedColor = (Color)ColorConverter.ConvertFromString(previousValues[property]);
                    Properties.Settings.Default[property] = previousValues[property];
                }
                catch(KeyNotFoundException ex)
                {
                    // Some controls requires special treatment
                    switch(property)
                    {
                        case "TimeTextSize":
                            {
                                // Slider_ValueChanged will be called
                                timeSize.Value = Double.Parse(previousValues[property]);
                                break;
                            }
                        case "HomeOpacity":
                            {
                                // Slider_ValueChanged will be called
                                opacity.Value = Double.Parse(previousValues[property]);
                                break;
                            }
                        case "TimeDateEnabled":
                            {
                                // Change property so it can be updated to the UI by binding
                                Properties.Settings.Default[property] = previousValues[property];
                                timeCheckBox.IsChecked = (previousValues[property].Equals("Visible")) ? true : false;
                                break;
                            }
                        case "TimeHorizontalAlignment":
                            {
                                // Change property so it can be updated to the UI by binding
                                Properties.Settings.Default.TimeHorizontalAlignment = previousValues["TimeHorizontalAlignment"];
                                Properties.Settings.Default.TimeVerticalAlignment = previousValues["TimeVerticalAlignment"];

                                string horizontal = previousValues["TimeHorizontalAlignment"];
                                string vertical = previousValues["TimeVerticalAlignment"];
                                // PosComboBox_SelectionChanged will be called
                                positionBox.Text = alignmentPos[new TimeAlignments() { hAlignment = horizontal, vAlignment = vertical }];
                                break;
                            }
                    }
                    
                }
            }

            changedSettings.Clear();

            logger.Log("All changes canceled.");
        }

        // Controls the behavior of both sliders
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = (Slider)sender;

            // Change property so it can be updated to the UI by binding
            Properties.Settings.Default[slider.Name] = slider.Value;

            // Because each slider change will trigger this event
            if (!changedSettings.Contains(slider.Name))
            {
                changedSettings.Add(slider.Name);
            }
        }

        private void DefaultBtn_Click(object sender, RoutedEventArgs e)
        {
            logger.Section("ConfigurationControl: DefaultBtn");

            // Restore default value for each change the user made
            foreach (string property in changedSettings.ToList())
            {
                try
                {
                    colorPickers[property].SelectedColor = (Color)ColorConverter.ConvertFromString(Properties.DefaultSettings.Default[property].ToString());
                    // Change property so it can be updated to the UI by binding
                    Properties.Settings.Default[property] = Properties.DefaultSettings.Default[property];
                }
                catch (KeyNotFoundException ex)
                {
                    // Some controls requires special treatment
                    switch(property)
                    {
                        case "TimeTextSize":
                            {
                                // Slider_ValueChanged will be called
                                timeSize.Value = Double.Parse(previousValues[property]);
                                break;
                            }
                        case "HomeOpacity":
                            {
                                // Slider_ValueChanged will be called
                                opacity.Value = Properties.Settings.Default.HomeOpacity;
                                break;
                            }
                        case "TimeDateEnabled":
                            {
                                // Change property so it can be updated to the UI by binding
                                Properties.Settings.Default[property] = Properties.DefaultSettings.Default[property];
                                break;
                            }
                        case "TimeHorizontalAlignment":
                            {
                                // Change property so it can be updated to the UI by binding
                                Properties.Settings.Default["TimeHorizontalAlignment"] = Properties.DefaultSettings.Default["TimeHorizontalAlignment"];
                                Properties.Settings.Default["TimeVerticalAlignment"] = Properties.DefaultSettings.Default["TimeVerticalAlignment"];

                                string horizontal = Properties.DefaultSettings.Default["TimeHorizontalAlignment"].ToString();
                                string vertical = Properties.DefaultSettings.Default["TimeVerticalAlignment"].ToString();
                                // PosComboBox_SelectionChanged will be called
                                positionBox.Text = alignmentPos[new TimeAlignments() { hAlignment = horizontal, vAlignment = vertical }];
                                break;
                            }
                    }
                    
                }
            }

            logger.Log("All changed values restored to default values.");
        }

        private void DefaultAllBtn_Click(object sender, RoutedEventArgs e)
        {
            logger.Section("ConfigurationControl: DefaultAllBtn");

            // Restore default values for all controls
            foreach (SettingsProperty property in Properties.DefaultSettings.Default.Properties)
            {
                try
                {
                    colorPickers[property.Name].SelectedColor = (Color)ColorConverter.ConvertFromString(property.DefaultValue.ToString());
                    // Change property so it can be updated to the UI by binding
                    Properties.Settings.Default[property.Name] = Properties.DefaultSettings.Default[property.Name];
                }
                catch (KeyNotFoundException ex)
                {
                    // Some controls requires special treatment
                    switch(property.Name)
                    {
                        case "TimeTextSize":
                            {
                                // Slider_ValueChanged will be called
                                timeSize.Value = Properties.DefaultSettings.Default.TimeTextSize;
                                break;
                            }
                        case "HomeOpacity":
                            {
                                // Slider_ValueChanged will be called
                                opacity.Value = Properties.DefaultSettings.Default.HomeOpacity;
                                break;
                            }
                        case "TimeDateEnabled":
                            {
                                // Change property so it can be updated to the UI by binding
                                Properties.Settings.Default[property.Name] = Properties.DefaultSettings.Default[property.Name];
                                break;
                            }
                    }
                    
                }
            }

            // Change property so it can be updated to the UI by binding
            Properties.Settings.Default["TimeHorizontalAlignment"] = Properties.DefaultSettings.Default["TimeHorizontalAlignment"];
            Properties.Settings.Default["TimeVerticalAlignment"] = Properties.DefaultSettings.Default["TimeVerticalAlignment"];

            string horizontal = Properties.DefaultSettings.Default["TimeHorizontalAlignment"].ToString();
            string vertical = Properties.DefaultSettings.Default["TimeVerticalAlignment"].ToString();
            // PosComboBox_SelectionChanged will be called
            positionBox.Text = alignmentPos[new TimeAlignments() { hAlignment = horizontal, vAlignment = vertical }];

            logger.Log("All values restored to default values");
        }

        // Save all changes and close pop up
        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
            MainWindow.ConfigurationPopupClose();
        }

        private void TimeCheckBox_Clicked(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.TimeDateEnabled.ToString().Equals("Visible"))
            {
                Properties.Settings.Default.TimeDateEnabled = "Hidden";
            }
            else
            {
                Properties.Settings.Default.TimeDateEnabled = "Visible";
            }

            if (!changedSettings.Contains("TimeDateEnabled"))
            {
                changedSettings.Add("TimeDateEnabled");
            }
        }

        private void PosComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string position = Convert.ToString(positionBox.SelectedItem);

            try
            {
                // Change property so it can be updated to the UI by binding
                Properties.Settings.Default.TimeHorizontalAlignment = posAlignment[position].hAlignment;
                Properties.Settings.Default.TimeVerticalAlignment = posAlignment[position].vAlignment;

                if (!changedSettings.Contains("TimeHorizontalAlignment"))
                {
                    changedSettings.Add("TimeHorizontalAlignment");
                }
            }
            catch
            {
                // When the comboBox.Text method is called by code, the position will be null
            }

            
        }
    }
}
