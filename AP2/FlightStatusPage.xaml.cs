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
using static AP2.MainWindow;
using AP2.Services;
using AP2.Helpers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AP2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FlightStatusPage : Page
    {
        private ObservableCollection<FlightInfo> _flights;
        private readonly AirportApiService _apiService;
        private readonly SignalRService _signalRService;
        private Dictionary<string, int> _flightNumberToIdMap;

        public FlightStatusPage()
        {
            InitializeComponent();
            _apiService = new AirportApiService();
            _signalRService = new SignalRService();
            _flightNumberToIdMap = new Dictionary<string, int>();
            SetupUI();
            SetupSignalR();
        }

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

        private async void SetupUI()
        {
            _flights = new ObservableCollection<FlightInfo>();
            FlightsListView.ItemsSource = _flights;
            await LoadFlightsAsync();
        }

        private async void SetupSignalR()
        {
            try
            {
                // Connect to SignalR hub
                await _signalRService.ConnectAsync();
                
                // Subscribe to flight status events
                _signalRService.FlightStatusUpdated += OnFlightStatusUpdated;
                
                ShowInfoBar("Real-time flight updates connected", InfoBarSeverity.Success);
            }
            catch (Exception ex)
            {
                ShowInfoBar($"Failed to connect to real-time updates: {ex.Message}", InfoBarSeverity.Warning);
            }
        }

        private void OnFlightStatusUpdated(FlightStatusUpdate flightStatus)
        {
            // Update UI on the UI thread
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

        private void ShowInfoBar(string message, InfoBarSeverity severity)
        {
            CheckInInfoBar.Title = "Message";
            CheckInInfoBar.Message = message;
            CheckInInfoBar.Severity = severity;
            CheckInInfoBar.IsOpen = true;
        }

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

        private StackPanel CreateStatusSelectionContent()
        {
            var panel = new StackPanel { Spacing = 10 };
            var statuses = new[] { "Checking In", "Delayed", "Boarding", "Cancelled" , "Departed"};

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

        // Clean up SignalR connection when page is unloaded
        private async void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            await _signalRService.DisconnectAsync();
        }
    }
}
