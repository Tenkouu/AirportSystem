using AirportSystemWindows.Helpers;
using AirportSystemWindows.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ZXing;
using ZXing.Common;

namespace AirportSystemWindows
{
    /// <summary>
    /// Represents passenger information for the check-in system.
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

    /// <summary>
    /// Check-in page for the airport management system.
    /// Handles passenger check-in, seat selection, and boarding pass generation.
    /// </summary>
    public sealed partial class CheckInPage : Page
    {
        private readonly AirportApiService _apiService;
        private readonly SignalRService _signalRService;
        private Dictionary<string, bool> _seatMap;
        private PassengerInfo _currentPassenger;
        private string _selectedSeat;
        private int _currentFlightId;

        /// <summary>
        /// Initializes a new instance of the CheckInPage class.
        /// </summary>
        public CheckInPage()
        {
            this.InitializeComponent();
            _apiService = new AirportApiService();
            _signalRService = new SignalRService();
            _seatMap = new Dictionary<string, bool>();
            SetupSignalR();
        }

        /// <summary>
        /// Sets up the SignalR connection and event handlers for real-time updates.
        /// </summary>
        private async void SetupSignalR()
        {
            try
            {
                await _signalRService.ConnectAsync();
                _signalRService.SeatOccupied += OnSeatOccupied;
                _signalRService.SeatAvailable += OnSeatAvailable;
                _signalRService.SeatSelected += OnSeatSelected;
                _signalRService.SeatDeselected += OnSeatDeselected;
            }
            catch (Exception ex)
            {
                ShowInfoBar($"Could not connect to real-time service: {ex.Message}", InfoBarSeverity.Error);
            }
        }

        /// <summary>
        /// Handles real-time seat occupied events from SignalR.
        /// </summary>
        /// <param name="seatNumber">The seat number that became occupied.</param>
        private void OnSeatOccupied(string seatNumber)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if (_seatMap.ContainsKey(seatNumber))
                {
                    _seatMap[seatNumber] = true;
                    UpdateSeatButton(seatNumber, isOccupied: true, isSelectedByOther: false);
                }
            });
        }

        /// <summary>
        /// Handles real-time seat available events from SignalR.
        /// </summary>
        /// <param name="seatNumber">The seat number that became available.</param>
        private void OnSeatAvailable(string seatNumber)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if (_seatMap.ContainsKey(seatNumber))
                {
                    _seatMap[seatNumber] = false;
                    UpdateSeatButton(seatNumber, isOccupied: false, isSelectedByOther: false);
                }
            });
        }

        /// <summary>
        /// Handles real-time seat selected events from SignalR.
        /// </summary>
        /// <param name="seatNumber">The seat number that was selected by another user.</param>
        private void OnSeatSelected(string seatNumber)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                UpdateSeatButton(seatNumber, isOccupied: false, isSelectedByOther: true);
            });
        }

        /// <summary>
        /// Handles real-time seat deselected events from SignalR.
        /// </summary>
        /// <param name="seatNumber">The seat number that was deselected by another user.</param>
        private void OnSeatDeselected(string seatNumber)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                UpdateSeatButton(seatNumber, isOccupied: false, isSelectedByOther: false);
            });
        }

        /// <summary>
        /// Updates the visual state of a seat button based on its occupancy and selection status.
        /// </summary>
        /// <param name="seatNumber">The seat number to update.</param>
        /// <param name="isOccupied">Whether the seat is occupied.</param>
        /// <param name="isSelectedByOther">Whether the seat is selected by another user.</param>
        private void UpdateSeatButton(string seatNumber, bool isOccupied, bool isSelectedByOther)
        {
            var button = SeatMapGrid.Children.OfType<Button>().FirstOrDefault(b => b.Tag?.ToString() == seatNumber);
            if (button != null)
            {
                if (isOccupied)
                {
                    button.Background = new SolidColorBrush(Colors.LightCoral); // Red: Occupied
                    button.IsEnabled = false;
                }
                else if (isSelectedByOther)
                {
                    // --- THE FIX IS HERE: Changed from Orange to a grey color ---
                    button.Background = new SolidColorBrush(Colors.DarkGray); // Grey: Locked by another agent
                    button.IsEnabled = false;
                }
                else
                {
                    button.Background = new SolidColorBrush(Colors.LightBlue); // Blue: Available
                    button.IsEnabled = true;
                }
            }
        }

        /// <summary>
        /// Handles seat button click events for seat selection.
        /// </summary>
        private async void SeatButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            string newSeatSelection = b.Tag.ToString();

            if (!string.IsNullOrEmpty(_selectedSeat) && _selectedSeat != newSeatSelection)
            {
                await _signalRService.DeselectSeatAsync(_currentFlightId, _selectedSeat);
            }

            await _signalRService.SelectSeatAsync(_currentFlightId, newSeatSelection);

            _selectedSeat = newSeatSelection;
            SelectedSeatText.Text = $"Selected Seat: {_selectedSeat}";

            foreach (var child in SeatMapGrid.Children.OfType<Button>())
            {
                string currentSeatTag = (string)child.Tag;
                if (!_seatMap[currentSeatTag])
                {
                    child.Background = new SolidColorBrush(Colors.LightBlue);
                }
            }
            b.Background = new SolidColorBrush(Colors.SlateBlue);
        }

        /// <summary>
        /// Handles the assign seat button click event.
        /// Opens the seat selection dialog for the current passenger.
        /// </summary>
        private async void AssignSeatButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPassenger == null) return;
            SeatDialogPassengerInfo.Text = $"Flight {_currentPassenger.Flight} | Passenger: {_currentPassenger.Name}";
            SelectedSeatText.Text = "Selected Seat: None";
            _selectedSeat = null;
            await LoadSeatMapAsync(_currentFlightId);
            await SeatSelectionDialog.ShowAsync();
        }

        /// <summary>
        /// Handles the secondary button click event for the seat selection dialog.
        /// Cancels seat selection and deselects the current seat.
        /// </summary>
        private async void SeatSelectionDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (!string.IsNullOrEmpty(_selectedSeat))
            {
                await _signalRService.DeselectSeatAsync(_currentFlightId, _selectedSeat);
            }
            _selectedSeat = null;
        }

        /// <summary>
        /// Handles the search passenger button click event.
        /// Searches for a passenger by passport number and displays their information.
        /// </summary>
        private async void SearchPassengerButton_Click(object sender, RoutedEventArgs e)
        {
            string passport = PassportNumberTextBox.Text.Trim();
            
            if (string.IsNullOrEmpty(passport))
            {
                ShowInfoBar("Please enter a passport number", InfoBarSeverity.Error);
                return;
            }

            try
            {
                ShowInfoBar("Searching...", InfoBarSeverity.Informational);
                var apiPassenger = await _apiService.GetPassengerByPassportAsync(passport);
                
                if (apiPassenger != null)
                {
                    if (_currentFlightId > 0 && _currentFlightId != apiPassenger.FlightID)
                    {
                        await _signalRService.LeaveFlightGroupAsync(_currentFlightId);
                    }

                    _currentPassenger = DataMapper.MapToPassengerInfo(apiPassenger);
                    _currentFlightId = apiPassenger.FlightID;

                    if (apiPassenger.IsCheckedIn)
                    {
                        PassengerInfoCard.Visibility = Visibility.Collapsed;
                        ShowInfoBar("Passenger is already checked in!", InfoBarSeverity.Warning);
                        return;
                    }

                    PassengerNameText.Text = _currentPassenger.Name;
                    PassengerPassportText.Text = _currentPassenger.Passport;
                    PassengerFlightText.Text = _currentPassenger.Flight;
                    CurrentSeatText.Text = _currentPassenger.Seat ?? "Not Assigned";
                    PassengerStatusText.Text = _currentPassenger.Status;

                    await LoadSeatMapAsync(_currentFlightId);

                    if (_signalRService.IsConnected)
                    {
                        await _signalRService.JoinFlightGroupAsync(_currentFlightId);
                    }

                    PassengerInfoCard.Visibility = Visibility.Visible;
                    ShowInfoBar("Passenger found!", InfoBarSeverity.Success);
                }
                else
                {
                    PassengerInfoCard.Visibility = Visibility.Collapsed;
                    ShowInfoBar("Passenger not found.", InfoBarSeverity.Error);
                }
            }
            catch (Exception ex)
            {
                PassengerInfoCard.Visibility = Visibility.Collapsed;
                ShowInfoBar($"Error: {ex.Message}", InfoBarSeverity.Error);
            }
        }
        /// <summary>
        /// Loads the seat map for the specified flight.
        /// </summary>
        /// <param name="flightId">The ID of the flight to load seats for.</param>
        private async Task LoadSeatMapAsync(int flightId)
        {
            try
            {
                var seats = await _apiService.GetSeatsByFlightAsync(flightId);
                _seatMap = DataMapper.MapToSeatMap(seats);
                GenerateSeatMapUI();
            }
            catch (Exception ex)
            {
                ShowInfoBar($"Could not load seat map: {ex.Message}", InfoBarSeverity.Error);
            }
        }
        /// <summary>
        /// Generates the seat map UI by creating buttons for each seat.
        /// </summary>
        private void GenerateSeatMapUI()
        {
            SeatMapGrid.Children.Clear();
            
            var sortedSeats = _seatMap.Keys.OrderBy(seat =>
            {
                if (seat.Length >= 2 && int.TryParse(seat.AsSpan(0, seat.Length - 1), out int row))
                    return row * 100 + (seat.Last() - 'A');
                return 0;
            });

            foreach (var seatNumber in sortedSeats)
            {
                int row = 0, col = 0;
                
                if (seatNumber.Length >= 2)
                {
                    if (int.TryParse(seatNumber.AsSpan(0, seatNumber.Length - 1), out int rowNum))
                        row = Math.Max(0, rowNum - 1);
                    
                    col = seatNumber.Last() switch
                    {
                        'A' => 0,
                        'B' => 1,
                        'C' => 2,
                        'D' => 3,
                        'E' => 4,
                        'F' => 5,
                        _ => 0
                    };
                }

                Button b = new Button
                {
                    Content = seatNumber,
                    Tag = seatNumber,
                    Margin = new Thickness(2),
                    Width = 50,
                    Height = 50,
                };
                
                b.Click += SeatButton_Click;
                Grid.SetRow(b, row);
                Grid.SetColumn(b, col);
                SeatMapGrid.Children.Add(b);
                UpdateSeatButton(seatNumber, _seatMap[seatNumber], isSelectedByOther: false);
            }
        }
        /// <summary>
        /// Handles the primary button click event for the seat selection dialog.
        /// Processes the seat assignment and check-in.
        /// </summary>
        private async void SeatSelectionDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (_currentPassenger != null && !string.IsNullOrEmpty(_selectedSeat))
            {
                try
                {
                    var result = await _apiService.CheckInPassengerAsync(_currentPassenger.Passport, _selectedSeat);
                    
                    if (result.Success)
                    {
                        _seatMap[_selectedSeat] = true;
                        _currentPassenger.Seat = _selectedSeat;
                        _currentPassenger.Status = "Checked In";
                        CurrentSeatText.Text = _currentPassenger.Seat;
                        PassengerStatusText.Text = _currentPassenger.Status;
                        ShowInfoBar($"Seat assigned successfully.", InfoBarSeverity.Success);
                    }
                    else
                    {
                        ShowInfoBar($"Failed to assign seat: {result.Message}", InfoBarSeverity.Error);
                        args.Cancel = true;
                    }
                }
                catch (Exception ex)
                {
                    ShowInfoBar($"Error assigning seat: {ex.Message}", InfoBarSeverity.Error);
                    args.Cancel = true;
                }
            }
            else
            {
                ShowInfoBar("You must select an available seat first.", InfoBarSeverity.Error);
                args.Cancel = true;
            }
        }
        /// <summary>
        /// Handles the print boarding pass button click event.
        /// Generates and prints a boarding pass for the current passenger.
        /// </summary>
        private async void PrintBoardingPassButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPassenger == null || string.IsNullOrEmpty(_currentPassenger.Seat))
            {
                ShowInfoBar("Please assign a seat first.", InfoBarSeverity.Error);
                return;
            }

            try
            {
                ShowInfoBar("Generating boarding pass...", InfoBarSeverity.Informational);
                var fullPassengerDetails = await _apiService.GetPassengerByPassportAsync(_currentPassenger.Passport);
                
                if (fullPassengerDetails == null)
                {
                    ShowInfoBar("Could not get full passenger details for printing.", InfoBarSeverity.Error);
                    return;
                }

                using (var boardingPassImage = await CreateBoardingPassImageAsync(fullPassengerDetails, _currentPassenger.Seat))
                {
                    if (boardingPassImage != null)
                    {
                        string tempFilePath = Path.Combine(Path.GetTempPath(), $"boarding_pass_{Guid.NewGuid()}.png");
                        boardingPassImage.Save(tempFilePath, System.Drawing.Imaging.ImageFormat.Png);
                        
                        var process = new System.Diagnostics.Process();
                        process.StartInfo = new System.Diagnostics.ProcessStartInfo(tempFilePath)
                        {
                            UseShellExecute = true,
                            Verb = "Print"
                        };
                        process.Start();
                    }
                    else
                    {
                        ShowInfoBar("Failed to create boarding pass image.", InfoBarSeverity.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowInfoBar($"Could not print: {ex.Message}", InfoBarSeverity.Error);
            }
        }
        /// <summary>
        /// Creates a boarding pass image for the specified passenger and seat.
        /// </summary>
        /// <param name="passengerDetails">The passenger details from the API.</param>
        /// <param name="assignedSeat">The assigned seat number.</param>
        /// <returns>A bitmap image of the boarding pass.</returns>
        private async Task<Bitmap> CreateBoardingPassImageAsync(PassengerApiResponse passengerDetails, string assignedSeat)
        {
            return await Task.Run(() =>
            {
                try
                {
                    int width = 300;
                    int height = 400;
                    var bitmap = new Bitmap(width, height);
                    
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                        
                        var accentColor = System.Drawing.Color.FromArgb(28, 97, 120);
                        var textColor = System.Drawing.Color.FromArgb(50, 50, 50);
                        var labelFont = new Font("Segoe UI", 8, System.Drawing.FontStyle.Bold);
                        var mainDataFont = new Font("Segoe UI", 14, System.Drawing.FontStyle.Bold);
                        var largeInfoFont = new Font("Segoe UI", 28, System.Drawing.FontStyle.Bold);
                        var smallDataFont = new Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
                        var airportCodeFont = new Font("Segoe UI", 28, System.Drawing.FontStyle.Bold);
                        var textBrush = new System.Drawing.SolidBrush(textColor);
                        var accentBrush = new System.Drawing.SolidBrush(accentColor);
                        
                        using (var bgBrush = new System.Drawing.SolidBrush(accentColor))
                        {
                            graphics.FillRectangle(bgBrush, 0, 0, width, height);
                        }
                        
                        graphics.FillRectangle(System.Drawing.Brushes.White, 10, 10, width - 20, height - 20);
                        
                        graphics.DrawString("FLIGHT", labelFont, accentBrush, 25, 30);
                        graphics.DrawString(passengerDetails.Flight.FlightNumber, mainDataFont, textBrush, 25, 42);
                        
                        StringFormat rightAlign = new StringFormat { Alignment = StringAlignment.Far };
                        graphics.DrawString("DATE", labelFont, accentBrush, new RectangleF(0, 30, width - 25, 20), rightAlign);
                        graphics.DrawString(passengerDetails.Flight.Time.ToString("dd MMM yyyy").ToUpper(), 
                            smallDataFont, textBrush, new RectangleF(0, 45, width - 25, 20), rightAlign);
                        
                        graphics.DrawLine(new System.Drawing.Pen(accentBrush, 1), 25, 80, width - 25, 80);
                        
                        string fromCode = GetAirportCode(passengerDetails.Flight.ArrivalAirport);
                        string toCode = GetAirportCode(passengerDetails.Flight.DestinationAirport);
                        
                        graphics.DrawString("FROM", labelFont, accentBrush, 25, 100);
                        graphics.DrawString(fromCode, airportCodeFont, textBrush, 25, 112);
                        graphics.DrawString("✈", new Font("Segoe UI Symbol", 20), accentBrush, 133, 125);
                        graphics.DrawString("TO", labelFont, accentBrush, new RectangleF(0, 100, width - 25, 20), rightAlign);
                        graphics.DrawString(toCode, airportCodeFont, textBrush, new RectangleF(0, 112, width - 25, 50), rightAlign);
                        
                        graphics.DrawLine(new System.Drawing.Pen(accentBrush, 1), 25, 180, width - 25, 180);
                        
                        graphics.DrawString("PASSENGER", labelFont, accentBrush, 25, 200);
                        graphics.DrawString(passengerDetails.FullName.ToUpper(), mainDataFont, textBrush, 25, 212);
                        
                        graphics.DrawLine(new System.Drawing.Pen(accentBrush, 1), 25, 250, width - 25, 250);
                        
                        graphics.DrawString("GATE", labelFont, accentBrush, 25, 270);
                        graphics.DrawString(passengerDetails.Flight.Gate, largeInfoFont, textBrush, 25, 282);
                        graphics.DrawString("SEAT", labelFont, accentBrush, new RectangleF(0, 270, width - 25, 20), rightAlign);
                        graphics.DrawString(assignedSeat, largeInfoFont, textBrush, new RectangleF(0, 282, width - 25, 50), rightAlign);
                        
                        using (var dashedPen = new System.Drawing.Pen(accentBrush, 2) { DashPattern = new float[] { 4, 4 } })
                        {
                            graphics.DrawLine(dashedPen, 25, 350, width - 25, 350);
                        }
                        
                        using (var barcode = GenerateBarcode($"{passengerDetails.PassportNumber}{assignedSeat}"))
                        {
                            if (barcode != null)
                            {
                                float barcodeX = (width - barcode.Width) / 2f;
                                graphics.DrawImage(barcode, barcodeX, 380);
                            }
                        }
                    }
                    
                    return bitmap;
                }
                catch
                {
                    return null;
                }
            });
        }
        /// <summary>
        /// Extracts airport code from full airport name.
        /// </summary>
        /// <param name="fullAirportName">The full airport name.</param>
        /// <returns>The extracted airport code.</returns>
        private string GetAirportCode(string fullAirportName)
        {
            var name = fullAirportName.Trim();
            return name.Length > 4 && name[name.Length - 4] == ' ' 
                ? name.Substring(name.Length - 3) 
                : (name.Length > 3 ? name.Substring(0, 3).ToUpper() : name.ToUpper());
        }

        /// <summary>
        /// Generates a barcode for the specified data.
        /// </summary>
        /// <param name="data">The data to encode in the barcode.</param>
        /// <returns>A bitmap containing the barcode image.</returns>
        private Bitmap GenerateBarcode(string data)
        {
            try
            {
                var writer = new BarcodeWriter<Bitmap>
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = new EncodingOptions
                    {
                        Height = 100,
                        Width = 250,
                        Margin = 10,
                        PureBarcode = true
                    }
                };
                return writer.Write(data);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Shows an information bar with the specified message and severity.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="severity">The severity level of the message.</param>
        private void ShowInfoBar(string message, InfoBarSeverity severity)
        {
            CheckInInfoBar.Title = "Status";
            CheckInInfoBar.Message = message;
            CheckInInfoBar.Severity = severity;
            CheckInInfoBar.IsOpen = true;
        }

        /// <summary>
        /// Handles the page unloaded event to clean up SignalR connections.
        /// </summary>
        private async void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_signalRService.IsConnected && _currentFlightId > 0)
            {
                await _signalRService.LeaveFlightGroupAsync(_currentFlightId);
            }
            await _signalRService.DisconnectAsync();
        }
    }
}