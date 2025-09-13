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
using static AirportSystemWindows.MainWindow;

namespace AirportSystemWindows
{
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
                ShowInfoBar("Generating boarding pass image...", InfoBarSeverity.Informational);

                var boardingPassImage = await CreateBoardingPassImageAsync();

                if (boardingPassImage != null)
                {
                    string tempFilePath = Path.Combine(Path.GetTempPath(), $"boarding_pass_{Guid.NewGuid()}.png");
                    boardingPassImage.Save(tempFilePath, System.Drawing.Imaging.ImageFormat.Png);
                    boardingPassImage.Dispose();

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
            catch (Exception ex)
            {
                ShowInfoBar($"Could not print: {ex.Message}", InfoBarSeverity.Error);
            }
        }

        // --- THIS IS THE NEW, REDESIGNED BOARDING PASS METHOD ---
        private async Task<System.Drawing.Bitmap> CreateBoardingPassImageAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    int width = 400;
                    int height = 600;
                    var bitmap = new System.Drawing.Bitmap(width, height);

                    using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
                    {
                        // Set high quality rendering
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                        // Define colors and fonts
                        var bgColor = System.Drawing.Color.FromArgb(255, 255, 255); // White background
                        var accentColor = System.Drawing.Color.FromArgb(28, 97, 120); // A nice teal/green
                        var textColor = System.Drawing.Color.FromArgb(50, 50, 50); // Dark text
                        var headerFont = new System.Drawing.Font("Segoe UI", 8, System.Drawing.FontStyle.Bold);
                        var largeFont = new System.Drawing.Font("Segoe UI", 28, System.Drawing.FontStyle.Bold);
                        var regularFont = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold);
                        var textBrush = new System.Drawing.SolidBrush(textColor);
                        var accentBrush = new System.Drawing.SolidBrush(accentColor);

                        // Fill background with main border
                        using (var bgBrush = new System.Drawing.SolidBrush(accentColor))
                        {
                            graphics.FillRectangle(bgBrush, 0, 0, width, height);
                        }
                        // Draw white inner area
                        graphics.FillRectangle(System.Drawing.Brushes.White, 15, 15, width - 30, height - 30);


                        // --- Header Section ---
                        graphics.DrawString("FLIGHT", headerFont, accentBrush, 30, 30);
                        graphics.DrawString(_currentPassenger.Flight.Split('-')[0].Trim(), largeFont, textBrush, 30, 45);

                        StringFormat rightAlign = new StringFormat { Alignment = StringAlignment.Far };
                        graphics.DrawString("DATE", headerFont, accentBrush, new System.Drawing.RectangleF(0, 30, width - 30, 20), rightAlign);
                        graphics.DrawString(DateTime.Now.ToString("dd MMM yyyy").ToUpper(), regularFont, textBrush, new System.Drawing.RectangleF(0, 50, width - 30, 20), rightAlign);

                        graphics.DrawLine(new System.Drawing.Pen(accentBrush, 1), 30, 90, width - 30, 90);

                        // --- From / To Section ---
                        graphics.DrawString("FROM", headerFont, accentBrush, 30, 110);
                        graphics.DrawString(_currentPassenger.Flight.Split('→')[0].Split('-')[1].Trim(), largeFont, textBrush, 30, 125);

                        // Simple airplane icon
                        graphics.DrawString("✈", new System.Drawing.Font("Segoe UI Symbol", 20), accentBrush, 175, 130);

                        graphics.DrawString("TO", headerFont, accentBrush, new System.Drawing.RectangleF(0, 110, width - 30, 20), rightAlign);
                        graphics.DrawString(_currentPassenger.Flight.Split('→')[1].Trim(), largeFont, textBrush, new System.Drawing.RectangleF(0, 125, width - 30, 40), rightAlign);

                        graphics.DrawLine(new System.Drawing.Pen(accentBrush, 1), 30, 180, width - 30, 180);

                        // --- Passenger Section ---
                        graphics.DrawString("PASSENGER", headerFont, accentBrush, 30, 200);
                        graphics.DrawString(_currentPassenger.Name.ToUpper(), regularFont, textBrush, 30, 215);

                        graphics.DrawLine(new System.Drawing.Pen(accentBrush, 1), 30, 250, width - 30, 250);

                        // --- Gate / Class / Seat Section ---
                        float columnWidth = (width - 60) / 3f;
                        graphics.DrawString("GATE", headerFont, accentBrush, 30, 270);
                        graphics.DrawString("A10", largeFont, textBrush, 30, 285); // Dummy Gate

                        graphics.DrawString("CLASS", headerFont, accentBrush, 30 + columnWidth, 270);
                        graphics.DrawString("ECO", largeFont, textBrush, 30 + columnWidth, 285); // Dummy Class

                        graphics.DrawString("SEAT", headerFont, accentBrush, 30 + (columnWidth * 2), 270);
                        graphics.DrawString(_currentPassenger.Seat, largeFont, textBrush, 30 + (columnWidth * 2), 285);

                        // --- Dotted Line Separator ---
                        float startX = 20;
                        float endX = width - 20;
                        float y = 350;
                        using (var dashedPen = new System.Drawing.Pen(accentBrush, 2) { DashPattern = new float[] { 5, 5 } })
                        {
                            graphics.DrawLine(dashedPen, startX, y, endX, y);
                        }

                        // --- Barcode Section ---
                        using (var barcode = GenerateBarcode($"{_currentPassenger.Passport}{_currentPassenger.Seat}"))
                        {
                            if (barcode != null)
                            {
                                float barcodeX = (width - barcode.Width) / 2f; // Center the barcode
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

        private System.Drawing.Bitmap GenerateBarcode(string data)
        {
            try
            {
                var writer = new BarcodeWriter<System.Drawing.Bitmap>
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = new EncodingOptions { Height = 80, Width = 300, Margin = 10, PureBarcode = true }
                };
                return writer.Write(data);
            }
            catch { return null; }
        }

        // All other methods below this line are for the UI and are unchanged.
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