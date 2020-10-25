﻿using System;
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
using System.Windows.Threading;

namespace Salon_App_WPF
{
    /// <summary>
    /// Interaction logic for HomeControl.xaml
    /// </summary>
    public partial class HomeControl : UserControl
    {
        private Logger logger;
        public HomeControl()
        {
            InitializeComponent();

            logger = new Logger();

            logger.Section("HomeControl: Default Constructor");
            logger.Log("Initializing.");
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            logger.Section("HomeControl: UserControl_Loaded");
            
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += TimerTick;
            timer.Start();

            logger.Log("Timer start ticking");
        }

        private void TimerTick(object sender, EventArgs e)
        {
            logger.Section("HomeControl: TimerTick");

            TimeLabel.Content = DateTime.Now.ToLongTimeString();
            DayLabel.Content = DateTime.Now.ToLongDateString();

            logger.Log("Content updated");
        }
    }
}
