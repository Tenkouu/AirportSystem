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

namespace AirportSystemWindows
{
    /// <summary>
    /// Main window for the airport management system.
    /// Provides navigation between different pages and contains shared data models.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
            ContentFrame.Navigate(typeof(CheckInPage));
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(AppTitleBar);
        }

        /// <summary>
        /// Handles navigation view selection changes to navigate between pages.
        /// </summary>
        /// <param name="sender">The navigation view that triggered the event.</param>
        /// <param name="args">Event arguments containing selection information.</param>
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

        /// <summary>
        /// Represents flight information with property change notifications.
        /// </summary>
        public class FlightInfo : INotifyPropertyChanged
        {
            private string _flightNumber = string.Empty;
            private string _departureAirport = string.Empty;
            private string _arrivalAirport = string.Empty;
            private string _scheduledTime = string.Empty;
            private string _status = string.Empty;
            private string _gate = string.Empty;
            private SolidColorBrush _statusColor = new SolidColorBrush(Colors.Gray);

            /// <summary>
            /// Gets or sets the flight number.
            /// </summary>
            public string FlightNumber 
            { 
                get => _flightNumber; 
                set { _flightNumber = value; OnPropertyChanged(); }
            }

            /// <summary>
            /// Gets or sets the departure airport.
            /// </summary>
            public string DepartureAirport 
            { 
                get => _departureAirport; 
                set { _departureAirport = value; OnPropertyChanged(); }
            }

            /// <summary>
            /// Gets or sets the arrival airport.
            /// </summary>
            public string ArrivalAirport 
            { 
                get => _arrivalAirport; 
                set { _arrivalAirport = value; OnPropertyChanged(); }
            }

            /// <summary>
            /// Gets or sets the scheduled departure time.
            /// </summary>
            public string ScheduledTime 
            { 
                get => _scheduledTime; 
                set { _scheduledTime = value; OnPropertyChanged(); }
            }

            /// <summary>
            /// Gets or sets the flight status.
            /// </summary>
            public string Status 
            { 
                get => _status; 
                set { _status = value; OnPropertyChanged(); }
            }

            /// <summary>
            /// Gets or sets the gate number.
            /// </summary>
            public string Gate 
            { 
                get => _gate; 
                set { _gate = value; OnPropertyChanged(); }
            }

            /// <summary>
            /// Gets or sets the color associated with the flight status.
            /// </summary>
            public SolidColorBrush StatusColor 
            { 
                get => _statusColor; 
                set { _statusColor = value; OnPropertyChanged(); }
            }

            /// <summary>
            /// Occurs when a property value changes.
            /// </summary>
            public event PropertyChangedEventHandler? PropertyChanged;

            /// <summary>
            /// Raises the PropertyChanged event for the specified property.
            /// </summary>
            /// <param name="propertyName">The name of the property that changed.</param>
            protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Represents passenger information for the airport system.
        /// </summary>
        public class PassengerInfo
        {
            /// <summary>
            /// Gets or sets the passenger's full name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the passenger's passport number.
            /// </summary>
            public string Passport { get; set; }

            /// <summary>
            /// Gets or sets the flight number.
            /// </summary>
            public string Flight { get; set; }

            /// <summary>
            /// Gets or sets the assigned seat number.
            /// </summary>
            public string Seat { get; set; }

            /// <summary>
            /// Gets or sets the passenger's check-in status.
            /// </summary>
            public string Status { get; set; }
        }
    }
}
