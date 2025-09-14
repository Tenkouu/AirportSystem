using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
using static AirportSystemWindows.MainWindow;
using AirportSystemWindows.Services;
using AirportSystemWindows.Helpers;

namespace AirportSystemWindows
{
    /// <summary>
    /// Flight status management page for the airport system.
    /// Displays real-time flight information and allows status updates.
    /// </summary>
    public sealed partial class FlightStatusPage : Page
    {
        private ObservableCollection<FlightInfo> _flights;
        private readonly AirportApiService _apiService;
        private readonly SignalRService _signalRService;
        private Dictionary<string, int> _flightNumberToIdMap;

        /// <summary>
        /// Initializes a new instance of the FlightStatusPage class.
        /// </summary>
        public FlightStatusPage()
        {
            InitializeComponent();
            _apiService = new AirportApiService();
            _signalRService = new SignalRService();
            _flightNumberToIdMap = new Dictionary<string, int>();
            SetupUI();
            SetupSignalR();
        }

        /// <summary>
        /// Loads flight data from the API and populates the flights collection.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task LoadFlightsAsync()
        {
            try
            {
                ShowInfoBar("Loading flights...", InfoBarSeverity.Informational);
                
                var apiFlights = await _apiService.GetFlightsAsync();
                _flights.Clear();
                _flightNumberToIdMap.Clear();

                foreach (var apiFlight in apiFlights)
                {
                    var flightInfo = DataMapper.MapToFlightInfo(apiFlight);
                    _flights.Add(flightInfo);
                    _flightNumberToIdMap[apiFlight.FlightNumber] = apiFlight.FlightID;
                }

                ShowInfoBar("Flights loaded successfully!", InfoBarSeverity.Success);
            }
            catch (Exception ex)
            {
                ShowInfoBar($"Failed to load flights: {ex.Message}", InfoBarSeverity.Error);
            }
        }

        /// <summary>
        /// Sets up the user interface components and loads initial data.
        /// </summary>
        private async void SetupUI()
        {
            _flights = new ObservableCollection<FlightInfo>();
            FlightsListView.ItemsSource = _flights;
            await LoadFlightsAsync();
        }

        /// <summary>
        /// Establishes SignalR connection for real-time flight status updates.
        /// </summary>
        private async void SetupSignalR()
        {
            try
            {
                await _signalRService.ConnectAsync();
                _signalRService.FlightStatusUpdated += OnFlightStatusUpdated;
                ShowInfoBar("Real-time flight updates connected", InfoBarSeverity.Success);
            }
            catch (Exception ex)
            {
                ShowInfoBar($"Failed to connect to real-time updates: {ex.Message}", InfoBarSeverity.Warning);
            }
        }

        /// <summary>
        /// Handles real-time flight status update events from SignalR.
        /// </summary>
        /// <param name="flightStatus">The flight status update information.</param>
        private void OnFlightStatusUpdated(FlightStatusUpdate flightStatus)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                var flight = _flights.FirstOrDefault(f => f.FlightNumber == flightStatus.FlightNumber);
                if (flight != null)
                {
                    flight.Status = flightStatus.Status;
                    flight.StatusColor = GetStatusColor(flightStatus.Status);
                    ShowInfoBar($"Flight {flightStatus.FlightNumber} status updated to: {flightStatus.Status}", InfoBarSeverity.Informational);
                }
            });
        }

        /// <summary>
        /// Displays an information bar message with the specified severity level.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="severity">The severity level of the message.</param>
        private void ShowInfoBar(string message, InfoBarSeverity severity)
        {
            CheckInInfoBar.Title = "Message";
            CheckInInfoBar.Message = message;
            CheckInInfoBar.Severity = severity;
            CheckInInfoBar.IsOpen = true;
        }

        /// <summary>
        /// Handles the change status button click event.
        /// Opens a dialog to allow status changes for the selected flight.
        /// </summary>
        /// <param name="sender">The button that was clicked.</param>
        /// <param name="e">Event arguments.</param>
        private async void ChangeStatusButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string flightNumber = button.Tag.ToString();

            if (!_flightNumberToIdMap.TryGetValue(flightNumber, out int flightId))
            {
                ShowInfoBar($"Flight {flightNumber} not found", InfoBarSeverity.Error);
                return;
            }

            ContentDialog statusDialog = new ContentDialog
            {
                Title = $"Change Status for Flight {flightNumber}",
                Content = CreateStatusSelectionContent(),
                PrimaryButtonText = "Update",
                SecondaryButtonText = "Cancel",
                XamlRoot = this.Content.XamlRoot
            };

            var result = await statusDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var content = statusDialog.Content as StackPanel;
                var radioButtons = content.Children.OfType<RadioButton>().ToList();
                var selectedRadioButton = radioButtons.FirstOrDefault(rb => rb.IsChecked == true);

                if (selectedRadioButton != null)
                {
                    string newStatus = selectedRadioButton.Content.ToString();
                    await UpdateFlightStatusAsync(flightId, flightNumber, newStatus);
                }
            }
        }

        /// <summary>
        /// Creates the content for the status selection dialog.
        /// </summary>
        /// <returns>A StackPanel containing radio buttons for status selection.</returns>
        private StackPanel CreateStatusSelectionContent()
        {
            var panel = new StackPanel { Spacing = 10 };
            var statuses = new[] { "Checking In", "Delayed", "Boarding", "Cancelled", "Departed" };

            foreach (var status in statuses)
            {
                var radioButton = new RadioButton
                {
                    Content = status,
                    GroupName = "FlightStatus"
                };
                panel.Children.Add(radioButton);
            }

            return panel;
        }

        /// <summary>
        /// Updates the flight status via the API and updates the UI.
        /// </summary>
        /// <param name="flightId">The ID of the flight to update.</param>
        /// <param name="flightNumber">The flight number for display purposes.</param>
        /// <param name="newStatus">The new status to set.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task UpdateFlightStatusAsync(int flightId, string flightNumber, string newStatus)
        {
            try
            {
                ShowInfoBar("Updating flight status...", InfoBarSeverity.Informational);
                
                string apiStatus = DataMapper.MapStatusToApi(newStatus);
                bool success = await _apiService.UpdateFlightStatusAsync(flightId, apiStatus);
                
                if (success)
                {
                    var flight = _flights.FirstOrDefault(f => f.FlightNumber == flightNumber);
                    if (flight != null)
                    {
                        flight.Status = newStatus;
                        flight.StatusColor = GetStatusColor(newStatus);
                    }
                    ShowInfoBar($"Flight {flightNumber} status updated to: {newStatus}", InfoBarSeverity.Success);
                }
                else
                {
                    ShowInfoBar($"Failed to update flight {flightNumber} status", InfoBarSeverity.Error);
                }
            }
            catch (Exception ex)
            {
                ShowInfoBar($"Error updating flight status: {ex.Message}", InfoBarSeverity.Error);
            }
        }

        /// <summary>
        /// Gets the color associated with a specific flight status.
        /// </summary>
        /// <param name="status">The flight status.</param>
        /// <returns>A SolidColorBrush representing the status color.</returns>
        private SolidColorBrush GetStatusColor(string status)
        {
            return status switch
            {
                "Checking In" => new SolidColorBrush(Colors.Green),
                "Delayed" => new SolidColorBrush(Colors.Orange),
                "Boarding" => new SolidColorBrush(Colors.Blue),
                "Cancelled" => new SolidColorBrush(Colors.Red),
                "Departed" => new SolidColorBrush(Colors.Pink),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }

        /// <summary>
        /// Handles the page unloaded event to clean up SignalR connections.
        /// </summary>
        /// <param name="sender">The page that was unloaded.</param>
        /// <param name="e">Event arguments.</param>
        private async void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            await _signalRService.DisconnectAsync();
        }
    }
}
