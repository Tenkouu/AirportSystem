using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using AirportSystemWindows.Services;
using AirportSystemWindows.Helpers;
using ZXing;
using ZXing.Common;
using System.Drawing;

namespace AirportSystemWindows
{
    public class PassengerInfo
    {
        public string Name { get; set; }
        public string Passport { get; set; }
        public string Flight { get; set; }
        public string Seat { get; set; }
        public string Status { get; set; }
    }

    public sealed partial class CheckInPage : Page
    {
        private readonly AirportApiService _apiService;
        private readonly SignalRService _signalRService;
        private Dictionary<string, bool> _seatMap;
        private PassengerInfo _currentPassenger;
        private string _selectedSeat;
        private int _currentFlightId;

        public CheckInPage()
        {
            this.InitializeComponent();
            _apiService = new AirportApiService();
            _signalRService = new SignalRService();
            SetupUI();
            SetupSignalR();
        }

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
                var boardingPassImage = await CreateBoardingPassImageAsync(fullPassengerDetails, _currentPassenger.Seat);
                if (boardingPassImage != null)
                {
                    string tempFilePath = Path.Combine(Path.GetTempPath(), $"boarding_pass_{Guid.NewGuid()}.png");
                    boardingPassImage.Save(tempFilePath, System.Drawing.Imaging.ImageFormat.Png);
                    boardingPassImage.Dispose();
                    var process = new System.Diagnostics.Process();
                    process.StartInfo = new System.Diagnostics.ProcessStartInfo(tempFilePath) { UseShellExecute = true, Verb = "Print" };
                    process.Start();
                }
                else { ShowInfoBar("Failed to create boarding pass image.", InfoBarSeverity.Error); }
            }
            catch (Exception ex) { ShowInfoBar($"Could not print: {ex.Message}", InfoBarSeverity.Error); }
        }

        // --- THIS IS THE FINAL, REDESIGNED BOARDING PASS WITH CORRECTED FONT SIZES ---
        private async Task<System.Drawing.Bitmap> CreateBoardingPassImageAsync(PassengerApiResponse passengerDetails, string assignedSeat)
        {
            return await Task.Run(() =>
            {
                try
                {
                    int width = 300;
                    int height = 400;
                    var bitmap = new System.Drawing.Bitmap(width, height);
                    using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
                    {
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                        // --- Define Colors and the NEW Font Hierarchy ---
                        var accentColor = System.Drawing.Color.FromArgb(28, 97, 120);
                        var textColor = System.Drawing.Color.FromArgb(50, 50, 50);
                        var labelFont = new System.Drawing.Font("Finlandica", 8, System.Drawing.FontStyle.Bold);    // Extra small for labels
                        var mainDataFont = new System.Drawing.Font("Finlandica", 14, System.Drawing.FontStyle.Bold); // For Flight# and Name
                        var largeInfoFont = new System.Drawing.Font("Finlandica", 28, System.Drawing.FontStyle.Bold); // For big info like Seat/Gate
                        var smallDataFont = new System.Drawing.Font("Finlandica", 10, System.Drawing.FontStyle.Bold);  // For the date
                        var airportCodeFont = new System.Drawing.Font("Finlandica", 28, System.Drawing.FontStyle.Bold);// For airport codes
                        var textBrush = new System.Drawing.SolidBrush(textColor);
                        var accentBrush = new System.Drawing.SolidBrush(accentColor);

                        // --- Draw Background ---
                        using (var bgBrush = new System.Drawing.SolidBrush(accentColor)) { graphics.FillRectangle(bgBrush, 0, 0, width, height); }
                        graphics.FillRectangle(System.Drawing.Brushes.White, 10, 10, width - 20, height - 20);

                        // --- Header: Flight & Date ---
                        graphics.DrawString("FLIGHT", labelFont, accentBrush, 25, 30);
                        graphics.DrawString(passengerDetails.Flight.FlightNumber, mainDataFont, textBrush, 25, 42);

                        StringFormat rightAlign = new StringFormat { Alignment = StringAlignment.Far };
                        graphics.DrawString("DATE", labelFont, accentBrush, new System.Drawing.RectangleF(0, 30, width - 25, 20), rightAlign);
                        graphics.DrawString(passengerDetails.Flight.Time.ToString("dd MMM yyyy").ToUpper(), smallDataFont, textBrush, new System.Drawing.RectangleF(0, 45, width - 25, 20), rightAlign);

                        graphics.DrawLine(new System.Drawing.Pen(accentBrush, 1), 25, 80, width - 25, 80);

                        // --- From / To ---
                        string fromCode = GetAirportCode(passengerDetails.Flight.ArrivalAirport);
                        string toCode = GetAirportCode(passengerDetails.Flight.DestinationAirport);

                        graphics.DrawString("FROM", labelFont, accentBrush, 25, 100);
                        graphics.DrawString(fromCode, airportCodeFont, textBrush, 25, 112);

                        graphics.DrawString("✈", new System.Drawing.Font("Finlandica Symbol", 20), accentBrush, 133, 125);

                        graphics.DrawString("TO", labelFont, accentBrush, new System.Drawing.RectangleF(0, 100, width - 25, 20), rightAlign);
                        graphics.DrawString(toCode, airportCodeFont, textBrush, new System.Drawing.RectangleF(0, 112, width - 25, 50), rightAlign);

                        graphics.DrawLine(new System.Drawing.Pen(accentBrush, 1), 25, 180, width - 25, 180);

                        // --- Passenger ---
                        graphics.DrawString("PASSENGER", labelFont, accentBrush, 25, 200);
                        graphics.DrawString(passengerDetails.FullName.ToUpper(), mainDataFont, textBrush, 25, 212);

                        graphics.DrawLine(new System.Drawing.Pen(accentBrush, 1), 25, 250, width - 25, 250);

                        // --- Gate & Seat ---
                        graphics.DrawString("GATE", labelFont, accentBrush, 25, 270);
                        graphics.DrawString(passengerDetails.Flight.Gate, largeInfoFont, textBrush, 25, 282);

                        graphics.DrawString("SEAT", labelFont, accentBrush, new System.Drawing.RectangleF(0, 270, width - 25, 20), rightAlign);
                        graphics.DrawString(assignedSeat, largeInfoFont, textBrush, new System.Drawing.RectangleF(0, 282, width - 25, 50), rightAlign);

                        // --- Dashed Line & Barcode ---
                        using (var dashedPen = new System.Drawing.Pen(accentBrush, 2) { DashPattern = new float[] { 4, 4 } }) { graphics.DrawLine(dashedPen, 25, 350, width - 25, 350); }
                        using (var barcode = GenerateBarcode($"{passengerDetails.PassportNumber}{assignedSeat}")) { if (barcode != null) { float barcodeX = (width - barcode.Width) / 2f; graphics.DrawImage(barcode, barcodeX, 380); } }
                    }
                    return bitmap;
                }
                catch { return null; }
            });
        }

        // All other code remains exactly the same as the last working version.
        private string GetAirportCode(string fullAirportName) { var name = fullAirportName.Trim(); if (name.Length > 4 && name[name.Length - 4] == ' ') { return name.Substring(name.Length - 3); } return name.Length > 3 ? name.Substring(0, 3).ToUpper() : name.ToUpper(); }
        private System.Drawing.Bitmap GenerateBarcode(string data) { try { var writer = new BarcodeWriter<System.Drawing.Bitmap> { Format = BarcodeFormat.CODE_128, Options = new EncodingOptions { Height = 100, Width = 250, Margin = 10, PureBarcode = true } }; return writer.Write(data); } catch { return null; } }
        private async void SearchPassengerButton_Click(object sender, RoutedEventArgs e) { string p = PassportNumberTextBox.Text.Trim(); if (string.IsNullOrEmpty(p)) { ShowInfoBar("Please enter a passport number", InfoBarSeverity.Error); return; } try { ShowInfoBar("Searching...", InfoBarSeverity.Informational); var ap = await _apiService.GetPassengerByPassportAsync(p); if (ap != null) { _currentPassenger = DataMapper.MapToPassengerInfo(ap); _currentFlightId = ap.FlightID; if (ap.IsCheckedIn) { PassengerInfoCard.Visibility = Visibility.Collapsed; ShowInfoBar("Passenger is already checked in!", InfoBarSeverity.Warning); return; } PassengerNameText.Text = _currentPassenger.Name; PassengerPassportText.Text = _currentPassenger.Passport; PassengerFlightText.Text = _currentPassenger.Flight; CurrentSeatText.Text = _currentPassenger.Seat ?? "Not Assigned"; PassengerStatusText.Text = _currentPassenger.Status; await LoadSeatMapAsync(_currentFlightId); if (_signalRService.IsConnected) { await _signalRService.JoinFlightGroupAsync(_currentFlightId); } PassengerInfoCard.Visibility = Visibility.Visible; ShowInfoBar("Passenger found!", InfoBarSeverity.Success); } else { PassengerInfoCard.Visibility = Visibility.Collapsed; ShowInfoBar("Passenger not found.", InfoBarSeverity.Error); } } catch (Exception ex) { PassengerInfoCard.Visibility = Visibility.Collapsed; ShowInfoBar($"Error: {ex.Message}", InfoBarSeverity.Error); } }
        private async void AssignSeatButton_Click(object sender, RoutedEventArgs e) { if (_currentPassenger == null) return; SeatDialogPassengerInfo.Text = $"Flight {_currentPassenger.Flight} | Passenger: {_currentPassenger.Name}"; SelectedSeatText.Text = "Selected Seat: None"; _selectedSeat = null; await LoadSeatMapAsync(_currentFlightId); ResetSeatSelection(); await SeatSelectionDialog.ShowAsync(); }
        private async Task LoadSeatMapAsync(int flightId) { try { var s = await _apiService.GetSeatsByFlightAsync(flightId); _seatMap = DataMapper.MapToSeatMap(s); GenerateSeatMap(); } catch (Exception ex) { ShowInfoBar($"Seat map error: {ex.Message}", InfoBarSeverity.Error); } }
        private void SetupUI() { _seatMap = new Dictionary<string, bool>(); }
        private async void SetupSignalR() { try { await _signalRService.ConnectAsync(); _signalRService.SeatOccupied += OnSeatOccupied; _signalRService.SeatAvailable += OnSeatAvailable; } catch { } }
        private void OnSeatOccupied(string s) { DispatcherQueue.TryEnqueue(() => { if (_seatMap.ContainsKey(s)) { _seatMap[s] = true; UpdateSeatButton(s, true); } }); }
        private void OnSeatAvailable(string s) { DispatcherQueue.TryEnqueue(() => { if (_seatMap.ContainsKey(s)) { _seatMap[s] = false; UpdateSeatButton(s, false); } }); }
        private void UpdateSeatButton(string s, bool o) { foreach (var c in SeatMapGrid.Children.OfType<Button>()) { if (c.Tag?.ToString() == s) { c.Background = o ? new SolidColorBrush(Colors.LightCoral) : new SolidColorBrush(Colors.LightBlue); break; } } }
        private void ShowInfoBar(string m, InfoBarSeverity s) { CheckInInfoBar.Title = "Status"; CheckInInfoBar.Message = m; CheckInInfoBar.Severity = s; CheckInInfoBar.IsOpen = true; }
        private void GenerateSeatMap() { SeatMapGrid.Children.Clear(); var sortedSeats = _seatMap.Keys.OrderBy(seat => { if (seat.Length >= 2 && int.TryParse(seat.Substring(0, seat.Length - 1), out int row)) return row * 100 + (seat.Last() - 'A'); return 0; }); foreach (var seat in sortedSeats) { int row = 0, col = 0; if (seat.Length >= 2) { if (int.TryParse(seat.Substring(0, seat.Length - 1), out int rowNum)) row = Math.Max(0, rowNum - 1); col = seat.Last() switch { 'A' => 0, 'B' => 1, 'C' => 2, 'D' => 3, 'E' => 4, 'F' => 5, _ => 0 }; } Button b = new Button { Content = seat, Tag = seat, Margin = new Thickness(2), Width = 50, Height = 50, Background = _seatMap[seat] ? new SolidColorBrush(Colors.LightCoral) : new SolidColorBrush(Colors.LightBlue) }; b.Click += SeatButton_Click; Grid.SetRow(b, row); Grid.SetColumn(b, col); SeatMapGrid.Children.Add(b); } }
        private void SeatButton_Click(object sender, RoutedEventArgs e) { Button b = (Button)sender; string s = b.Tag.ToString(); if (_seatMap[s]) return; _selectedSeat = s; SelectedSeatText.Text = $"Selected Seat: {s}"; foreach (var c in SeatMapGrid.Children.OfType<Button>()) { if (!_seatMap[(string)c.Tag]) c.Background = new SolidColorBrush(Colors.LightBlue); } b.Background = new SolidColorBrush(Colors.SlateBlue); }
        private void ResetSeatSelection() { foreach (var c in SeatMapGrid.Children.OfType<Button>()) { string s = c.Tag.ToString(); c.Background = _seatMap[s] ? new SolidColorBrush(Colors.LightCoral) : new SolidColorBrush(Colors.LightBlue); } }
        private async void SeatSelectionDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) { if (_currentPassenger != null && !string.IsNullOrEmpty(_selectedSeat)) { try { var r = await _apiService.CheckInPassengerAsync(_currentPassenger.Passport, _selectedSeat); if (r.Success) { _seatMap[_selectedSeat] = true; _currentPassenger.Seat = _selectedSeat; _currentPassenger.Status = "Checked In"; CurrentSeatText.Text = _currentPassenger.Seat; PassengerStatusText.Text = _currentPassenger.Status; ShowInfoBar($"Seat assigned.", InfoBarSeverity.Success); } else { ShowInfoBar($"Failed: {r.Message}", InfoBarSeverity.Error); args.Cancel = true; } } catch (Exception ex) { ShowInfoBar($"Error: {ex.Message}", InfoBarSeverity.Error); args.Cancel = true; } } else { ShowInfoBar("Select a seat.", InfoBarSeverity.Error); args.Cancel = true; } }
        private void SeatSelectionDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) { _selectedSeat = null; }
        private async void Page_Unloaded(object sender, RoutedEventArgs e) { if (_signalRService.IsConnected && _currentFlightId > 0) await _signalRService.LeaveFlightGroupAsync(_currentFlightId); await _signalRService.DisconnectAsync(); }
    }
}