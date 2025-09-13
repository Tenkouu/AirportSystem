using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AP2
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            // default page нээх
            ContentFrame.Navigate(typeof(CheckInPage));
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(AppTitleBar);
        }

        private void MainNavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item)
            {
                switch (item.Tag.ToString())
                {
                    case "CheckInPage":
                        ContentFrame.Navigate(typeof(CheckInPage));
                        break;
                    case "FlightStatusPage":
                        ContentFrame.Navigate(typeof(FlightStatusPage));
                        break;
                }
            }
        }

        public class FlightInfo : INotifyPropertyChanged
        {
            private string _flightNumber = string.Empty;
            private string _departureAirport = string.Empty;
            private string _arrivalAirport = string.Empty;
            private string _scheduledTime = string.Empty;
            private string _status = string.Empty;
            private string _gate = string.Empty;
            private SolidColorBrush _statusColor = new SolidColorBrush(Colors.Gray);

            public string FlightNumber 
            { 
                get => _flightNumber; 
                set { _flightNumber = value; OnPropertyChanged(); }
            }
            public string DepartureAirport 
            { 
                get => _departureAirport; 
                set { _departureAirport = value; OnPropertyChanged(); }
            }
            public string ArrivalAirport 
            { 
                get => _arrivalAirport; 
                set { _arrivalAirport = value; OnPropertyChanged(); }
            }
            public string ScheduledTime 
            { 
                get => _scheduledTime; 
                set { _scheduledTime = value; OnPropertyChanged(); }
            }
            public string Status 
            { 
                get => _status; 
                set { _status = value; OnPropertyChanged(); }
            }
            public string Gate 
            { 
                get => _gate; 
                set { _gate = value; OnPropertyChanged(); }
            }
            public SolidColorBrush StatusColor 
            { 
                get => _statusColor; 
                set { _statusColor = value; OnPropertyChanged(); }
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class PassengerInfo
        {
            public string Name { get; set; }
            public string Passport { get; set; }
            public string Flight { get; set; }
            public string Seat { get; set; }
            public string Status { get; set; }
        }
    }
}
