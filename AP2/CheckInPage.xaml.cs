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
using Microsoft.UI.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using static AP2.MainWindow;
using AP2.Services;
using AP2.Helpers;
using ZXing;
using ZXing.Common;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AP2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
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
            InitializeComponent();
            _apiService = new AirportApiService();
            _signalRService = new SignalRService();
            SetupUI();
            SetupSignalR();
        }

        private async Task LoadSeatMapAsync(int flightId)
        {
            try
            {
                ShowInfoBar("Loading seat map...", InfoBarSeverity.Informational);
                var seats = await _apiService.GetSeatsByFlightAsync(flightId);
                _seatMap = DataMapper.MapToSeatMap(seats);
                
                // Debug: Show seat map contents
                var debugInfo = string.Join(", ", _seatMap.Select(kvp => $"{kvp.Key}:{(kvp.Value ? "O" : "F")}"));
                ShowInfoBar($"Loaded {_seatMap.Count} seats: {debugInfo}", InfoBarSeverity.Success);
                GenerateSeatMap();
            }
            catch (Exception ex)
            {
                ShowInfoBar($"Failed to load seat map: {ex.Message}", InfoBarSeverity.Error);
            }
        }

        private void SetupUI()
        {
            // Initialize empty seat map - will be loaded when passenger is found
            _seatMap = new Dictionary<string, bool>();
        }

        private async void SetupSignalR()
        {
            try
            {
                // Connect to SignalR hub
                await _signalRService.ConnectAsync();
                
                // Subscribe to seat events
                _signalRService.SeatOccupied += OnSeatOccupied;
                _signalRService.SeatAvailable += OnSeatAvailable;
                
                ShowInfoBar("Real-time updates connected", InfoBarSeverity.Success);
            }
            catch (Exception ex)
            {
                ShowInfoBar($"Failed to connect to real-time updates: {ex.Message}", InfoBarSeverity.Warning);
            }
        }

        private void OnSeatOccupied(string seatNumber)
        {
            // Update UI on the UI thread
            DispatcherQueue.TryEnqueue(() =>
            {
                if (_seatMap.ContainsKey(seatNumber))
                {
                    _seatMap[seatNumber] = true;
                    UpdateSeatButton(seatNumber, true);
                    ShowInfoBar($"Seat {seatNumber} is now occupied", InfoBarSeverity.Informational);
                }
            });
        }

        private void OnSeatAvailable(string seatNumber)
        {
            // Update UI on the UI thread
            DispatcherQueue.TryEnqueue(() =>
            {
                if (_seatMap.ContainsKey(seatNumber))
                {
                    _seatMap[seatNumber] = false;
                    UpdateSeatButton(seatNumber, false);
                    ShowInfoBar($"Seat {seatNumber} is now available", InfoBarSeverity.Informational);
                }
            });
        }

        private void UpdateSeatButton(string seatNumber, bool isOccupied)
        {
            foreach (var child in SeatMapGrid.Children.OfType<Button>())
            {
                if (child.Tag.ToString() == seatNumber)
                {
                    child.Background = isOccupied ? 
                        new SolidColorBrush(Colors.LightCoral) : 
                        new SolidColorBrush(Colors.LightBlue);
                    break;
                }
            }
        }

        private void ShowInfoBar(string message, InfoBarSeverity severity)
        {
            CheckInInfoBar.Title = "Message";
            CheckInInfoBar.Message = message;
            CheckInInfoBar.Severity = severity;
            CheckInInfoBar.IsOpen = true;
        }

        private async void SearchPassengerButton_Click(object sender, RoutedEventArgs e)
        {
            string passportNumber = PassportNumberTextBox.Text.Trim();

            if (string.IsNullOrEmpty(passportNumber))
            {
                ShowInfoBar("Please enter a passport number", InfoBarSeverity.Error);
                return;
            }

            try
            {
                ShowInfoBar("Searching for passenger...", InfoBarSeverity.Informational);
                
                var apiPassenger = await _apiService.GetPassengerByPassportAsync(passportNumber);
                
                if (apiPassenger != null)
                {
                    _currentPassenger = DataMapper.MapToPassengerInfo(apiPassenger);
                    _currentFlightId = apiPassenger.FlightID;

                    // Check if passenger is already checked in
                    if (apiPassenger.IsCheckedIn)
                    {
                        PassengerInfoCard.Visibility = Visibility.Collapsed;
                        ShowInfoBar("⚠️ This passenger is already checked in! Cannot perform check-in again.", InfoBarSeverity.Warning);
                        return;
                    }

                    PassengerNameText.Text = _currentPassenger.Name;
                    PassengerPassportText.Text = _currentPassenger.Passport;
                    PassengerFlightText.Text = _currentPassenger.Flight;
                    CurrentSeatText.Text = _currentPassenger.Seat ?? "Not Assigned";
                    PassengerStatusText.Text = _currentPassenger.Status;

                    // Load seat map for this flight
                    await LoadSeatMapAsync(_currentFlightId);

                    // Join flight group for real-time updates
                    if (_signalRService.IsConnected)
                    {
                        await _signalRService.JoinFlightGroupAsync(_currentFlightId);
                    }

                    PassengerInfoCard.Visibility = Visibility.Visible;
                    ShowInfoBar("Passenger found successfully!", InfoBarSeverity.Success);
                }
                else
                {
                    PassengerInfoCard.Visibility = Visibility.Collapsed;
                    ShowInfoBar("Passenger not found. Please check the passport number.", InfoBarSeverity.Error);
                }
            }
            catch (Exception ex)
            {
                PassengerInfoCard.Visibility = Visibility.Collapsed;
                ShowInfoBar($"Error searching for passenger: {ex.Message}", InfoBarSeverity.Error);
            }
        }

        private async void AssignSeatButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPassenger == null) return;

            SeatDialogPassengerInfo.Text = $"Flight {_currentPassenger.Flight} | Passenger: {_currentPassenger.Name}";
            SelectedPassengerText.Text = $"Passenger: {_currentPassenger.Name}";
            SelectedSeatText.Text = "Selected Seat: None";
            _selectedSeat = null;

            // Reload seat map to get current seat status
            await LoadSeatMapAsync(_currentFlightId);
            ResetSeatSelection();
            await SeatSelectionDialog.ShowAsync();
        }

        private async void PrintBoardingPassButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPassenger == null || string.IsNullOrEmpty(_currentPassenger.Seat))
            {
                ShowInfoBar("Please assign a seat first before printing boarding pass.", InfoBarSeverity.Error);
                return;
            }

            // Create boarding pass dialog with print options
            var boardingPassContent = CreateBoardingPassContent();

            // Show boarding pass dialog
            ContentDialog boardingPassDialog = new ContentDialog
            {
                Title = "🎫 Boarding Pass",
                Content = boardingPassContent,
                CloseButtonText = "Print",
                SecondaryButtonText = "Preview",
                PrimaryButtonText = "Close",
                XamlRoot = this.Content.XamlRoot
            };

            var result = await boardingPassDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                // Print boarding pass
                await PrintBoardingPassAsync();
            }
            else if (result == ContentDialogResult.Secondary)
            {
                // Show preview
                await ShowBoardingPassPreviewAsync();
            }
        }

        private StackPanel CreateBoardingPassContent()
        {
            var boardingPassContent = new StackPanel
            {
                Spacing = 12
            };

            // Header
            var headerText = new TextBlock
            {
                Text = "🔥 CUSTOM AIRPORT SYSTEM",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red)
            };
            boardingPassContent.Children.Add(headerText);

            // Passenger info
            var passengerInfo = new StackPanel
            {
                Background = new SolidColorBrush(Microsoft.UI.Colors.LightBlue),
                Padding = new Thickness(15)
            };

            var passengerName = new TextBlock
            {
                Text = $"Passenger: {_currentPassenger.Name}",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold
            };
            passengerInfo.Children.Add(passengerName);

            var flightInfo = new TextBlock
            {
                Text = $"Flight: {_currentPassenger.Flight}",
                FontSize = 14,
                Margin = new Thickness(0, 5, 0, 0)
            };
            passengerInfo.Children.Add(flightInfo);

            var seatInfo = new TextBlock
            {
                Text = $"Seat: {_currentPassenger.Seat}",
                FontSize = 14,
                Margin = new Thickness(0, 5, 0, 0)
            };
            passengerInfo.Children.Add(seatInfo);

            var statusInfo = new TextBlock
            {
                Text = $"Status: {_currentPassenger.Status}",
                FontSize = 14,
                Margin = new Thickness(0, 5, 0, 0)
            };
            passengerInfo.Children.Add(statusInfo);

            boardingPassContent.Children.Add(passengerInfo);

            // Instructions
            var instructions = new TextBlock
            {
                Text = "Please proceed to gate for boarding.",
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gray)
            };
            boardingPassContent.Children.Add(instructions);

            return boardingPassContent;
        }

        private async Task PrintBoardingPassAsync()
        {
            try
            {
                ShowInfoBar("Preparing to print boarding pass...", InfoBarSeverity.Informational);

                // Create boarding pass as image and save to file
                var boardingPassImage = await CreateBoardingPassImageAsync();
                if (boardingPassImage != null)
                {
                    // Save image to temporary file
                    var tempPath = Path.Combine(Path.GetTempPath(), $"boarding_pass_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                    boardingPassImage.Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);

                    // Show print options dialog
                    var printOptionsDialog = new ContentDialog
                    {
                        Title = "🖨️ Print Options",
                        Content = CreatePrintOptionsContent(tempPath),
                CloseButtonText = "Print",
                        SecondaryButtonText = "Save as Image",
                        PrimaryButtonText = "Cancel",
                XamlRoot = this.Content.XamlRoot
            };

                    var result = await printOptionsDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
                    {
                        // Print using Windows default printer
                        await PrintImageAsync(tempPath);
                        ShowInfoBar("Boarding pass printed successfully!", InfoBarSeverity.Success);
                    }
                    else if (result == ContentDialogResult.Secondary)
                    {
                        // Save as image
                        var saveDialog = new ContentDialog
                        {
                            Title = "💾 Save Boarding Pass",
                            Content = $"Boarding pass saved to: {tempPath}",
                            CloseButtonText = "OK",
                            XamlRoot = this.Content.XamlRoot
                        };
                        await saveDialog.ShowAsync();
                        ShowInfoBar("Boarding pass saved as image!", InfoBarSeverity.Success);
                    }
                    else
                    {
                        ShowInfoBar("Print cancelled.", InfoBarSeverity.Informational);
                    }

                    // Clean up
                    boardingPassImage.Dispose();
                }
            }
            catch (Exception ex)
            {
                ShowInfoBar($"Print error: {ex.Message}", InfoBarSeverity.Error);
            }
        }

        private async Task<System.Drawing.Bitmap> CreateBoardingPassImageAsync()
        {
            try
            {
                // Create bitmap for boarding pass
                var bitmap = new System.Drawing.Bitmap(600, 400);
                var graphics = System.Drawing.Graphics.FromImage(bitmap);
                
                // Set background
                graphics.Clear(System.Drawing.Color.White);

                var font = new System.Drawing.Font("Arial", 10);
                var boldFont = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold);
                var titleFont = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold);
                var smallFont = new System.Drawing.Font("Arial", 8);
                var brush = System.Drawing.Brushes.Black;

                float yPosition = 30;
                float leftMargin = 30;
                float pageWidth = 540;

                // Header with airline logo area
                var headerRect = new System.Drawing.RectangleF(leftMargin, yPosition, pageWidth, 40);
                graphics.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(0, 102, 204)), headerRect);
                graphics.DrawRectangle(System.Drawing.Pens.DarkBlue, headerRect);

                graphics.DrawString("✈️ CUSTOM AIRPORT SYSTEM", titleFont, System.Drawing.Brushes.White, 
                    new System.Drawing.RectangleF(leftMargin + 10, yPosition + 10, pageWidth - 20, 20), 
                    new System.Drawing.StringFormat { Alignment = System.Drawing.StringAlignment.Center });

                yPosition += 50;

                // Generate Barcode only
                var barcodeData = $"{_currentPassenger.Passport}{_currentPassenger.Seat}";
                var barcode = GenerateBarcode(barcodeData);
                
                // Main content area
                var contentRect = new System.Drawing.RectangleF(leftMargin, yPosition, pageWidth, 200);
                graphics.DrawRectangle(System.Drawing.Pens.Black, contentRect);

                // Left side - Passenger info
                var leftRect = new System.Drawing.RectangleF(leftMargin + 5, yPosition + 5, pageWidth * 0.6f, 190);
                graphics.FillRectangle(System.Drawing.Brushes.LightBlue, leftRect);
                graphics.DrawRectangle(System.Drawing.Pens.Black, leftRect);

                float leftY = yPosition + 15;
                graphics.DrawString("PASSENGER INFORMATION", boldFont, brush, leftMargin + 10, leftY);
                leftY += 25;

                graphics.DrawString($"Name: {_currentPassenger.Name}", font, brush, leftMargin + 10, leftY);
                leftY += 20;
                graphics.DrawString($"Passport: {_currentPassenger.Passport}", font, brush, leftMargin + 10, leftY);
                leftY += 20;
                graphics.DrawString($"Flight: {_currentPassenger.Flight}", font, brush, leftMargin + 10, leftY);
                leftY += 20;
                graphics.DrawString($"Seat: {_currentPassenger.Seat}", font, brush, leftMargin + 10, leftY);
                leftY += 20;
                graphics.DrawString($"Status: {_currentPassenger.Status}", font, brush, leftMargin + 10, leftY);
                leftY += 20;
                graphics.DrawString($"Date: {DateTime.Now:yyyy-MM-dd}", font, brush, leftMargin + 10, leftY);
                leftY += 20;
                graphics.DrawString($"Time: {DateTime.Now:HH:mm:ss}", font, brush, leftMargin + 10, leftY);

                // Right side - Barcode only
                var rightRect = new System.Drawing.RectangleF(leftMargin + pageWidth * 0.6f + 5, yPosition + 5, pageWidth * 0.35f, 190);
                graphics.FillRectangle(System.Drawing.Brushes.White, rightRect);
                graphics.DrawRectangle(System.Drawing.Pens.Black, rightRect);

                // Barcode (horizontal, centered)
                if (barcode != null)
                {
                    // Calculate center position for barcode
                    float barcodeWidth = rightRect.Width - 20;
                    float barcodeHeight = 60;
                    float barcodeX = rightRect.X + 10;
                    float barcodeY = rightRect.Y + (rightRect.Height - barcodeHeight) / 2;
                    
                    var barcodeRect = new System.Drawing.RectangleF(barcodeX, barcodeY, barcodeWidth, barcodeHeight);
                    graphics.DrawImage(barcode, barcodeRect);
                    
                    // Add barcode text below
                    graphics.DrawString(barcodeData, smallFont, brush, 
                        new System.Drawing.RectangleF(barcodeX, barcodeY + barcodeHeight + 5, barcodeWidth, 15), 
                        new System.Drawing.StringFormat { Alignment = System.Drawing.StringAlignment.Center });
                }

                graphics.DrawString("BOARDING PASS", smallFont, brush, rightRect.X + 10, rightRect.Y + rightRect.Height - 15);

                yPosition += 220;

                // Footer
                var footerRect = new System.Drawing.RectangleF(leftMargin, yPosition, pageWidth, 30);
                graphics.FillRectangle(System.Drawing.Brushes.LightGray, footerRect);
                graphics.DrawRectangle(System.Drawing.Pens.Black, footerRect);

                graphics.DrawString("Please proceed to gate for boarding. Keep this pass with you.", 
                    font, brush, new System.Drawing.RectangleF(leftMargin + 10, yPosition + 5, pageWidth - 20, 20), 
                    new System.Drawing.StringFormat { Alignment = System.Drawing.StringAlignment.Center });

                yPosition += 40;

                // Security notice
                graphics.DrawString("SECURITY NOTICE: This boarding pass contains sensitive information.", 
                    smallFont, System.Drawing.Brushes.Red, leftMargin, yPosition);
                yPosition += 15;
                graphics.DrawString("Do not share or photograph this document.", 
                    smallFont, System.Drawing.Brushes.Red, leftMargin, yPosition);

                // Dispose resources
                graphics.Dispose();
                barcode?.Dispose();

                return bitmap;
            }
            catch (Exception ex)
            {
                ShowInfoBar($"Error creating boarding pass image: {ex.Message}", InfoBarSeverity.Error);
                return null;
            }
        }


        private System.Drawing.Bitmap GenerateBarcode(string data)
        {
            try
            {
                var writer = new BarcodeWriter<System.Drawing.Bitmap>
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = new EncodingOptions
                    {
                        Height = 80,
                        Width = 300,
                        Margin = 5
                    }
                };
                return writer.Write(data);
            }
            catch
            {
                return null;
            }
        }

        private StackPanel CreatePrintOptionsContent(string imagePath)
        {
            var panel = new StackPanel { Spacing = 10 };
            
            var infoText = new TextBlock
            {
                Text = "Boarding pass is ready to print. Choose an option:",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 10)
            };
            panel.Children.Add(infoText);

            var pathText = new TextBlock
            {
                Text = $"File: {imagePath}",
                FontSize = 12,
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gray),
                TextWrapping = TextWrapping.Wrap
            };
            panel.Children.Add(pathText);

            return panel;
        }

        private async Task PrintImageAsync(string imagePath)
        {
            try
            {
                // Use Windows default image viewer to print
                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "mspaint.exe";
                process.StartInfo.Arguments = $"/p \"{imagePath}\"";
                process.Start();
                
                await Task.Delay(1000); // Give time for the process to start
            }
            catch (Exception ex)
            {
                ShowInfoBar($"Print error: {ex.Message}", InfoBarSeverity.Error);
            }
        }

        private async Task ShowBoardingPassPreviewAsync()
        {
            try
            {
                ShowInfoBar("Generating preview...", InfoBarSeverity.Informational);

                // Create actual boarding pass image
                var boardingPassImage = await CreateBoardingPassImageAsync();
                if (boardingPassImage != null)
                {
                    // Save image to temporary file for preview
                    var tempPath = Path.Combine(Path.GetTempPath(), $"boarding_pass_preview_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                    boardingPassImage.Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);

                    // Create preview content
                    var previewContent = new StackPanel { Spacing = 15 };

                    // Title
                    var titleText = new TextBlock
                    {
                        Text = "📄 Boarding Pass Preview",
                        FontSize = 18,
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    previewContent.Children.Add(titleText);

                    // Preview info
                    var infoText = new TextBlock
                    {
                        Text = $"This is exactly how your boarding pass will look when printed.\n\nFile saved to: {tempPath}",
                        FontSize = 14,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gray),
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 0, 0, 15)
                    };
                    previewContent.Children.Add(infoText);

                    // Boarding pass details
                    var detailsPanel = new StackPanel
                    {
                        Background = new SolidColorBrush(Microsoft.UI.Colors.LightBlue),
                        Padding = new Thickness(15),
                        CornerRadius = new CornerRadius(8)
                    };

                    var passengerName = new TextBlock
                    {
                        Text = $"Passenger: {_currentPassenger.Name}",
                        FontSize = 16,
                        FontWeight = FontWeights.SemiBold,
                        Margin = new Thickness(0, 0, 0, 8)
                    };
                    detailsPanel.Children.Add(passengerName);

                    var flightInfo = new TextBlock
                    {
                        Text = $"Flight: {_currentPassenger.Flight}",
                        FontSize = 14,
                        Margin = new Thickness(0, 0, 0, 5)
                    };
                    detailsPanel.Children.Add(flightInfo);

                    var seatInfo = new TextBlock
                    {
                        Text = $"Seat: {_currentPassenger.Seat}",
                        FontSize = 14,
                        Margin = new Thickness(0, 0, 0, 5)
                    };
                    detailsPanel.Children.Add(seatInfo);

                    var statusInfo = new TextBlock
                    {
                        Text = $"Status: {_currentPassenger.Status}",
                        FontSize = 14,
                        Margin = new Thickness(0, 0, 0, 5)
                    };
                    detailsPanel.Children.Add(statusInfo);

                    var barcodeInfo = new TextBlock
                    {
                        Text = "✓ Bar Code included",
                        FontSize = 12,
                        Foreground = new SolidColorBrush(Microsoft.UI.Colors.Green),
                        Margin = new Thickness(0, 5, 0, 0)
                    };
                    detailsPanel.Children.Add(barcodeInfo);

                    previewContent.Children.Add(detailsPanel);

                    // Show preview dialog
                    ContentDialog previewDialog = new ContentDialog
                    {
                        Title = "🎫 Boarding Pass Preview",
                        Content = previewContent,
                        CloseButtonText = "Print",
                        SecondaryButtonText = "Close",
                        PrimaryButtonText = "Open Image",
                        XamlRoot = this.Content.XamlRoot
                    };

                    var result = await previewDialog.ShowAsync();
                    if (result == ContentDialogResult.Primary)
                    {
                        // Print the boarding pass
                        await PrintBoardingPassAsync();
                    }
                    else if (result == ContentDialogResult.Secondary)
                    {
                        // Open image in default viewer
                        var process = new System.Diagnostics.Process();
                        process.StartInfo.FileName = tempPath;
                        process.Start();
                        ShowInfoBar("Boarding pass image opened!", InfoBarSeverity.Success);
                    }

                    // Clean up
                    boardingPassImage.Dispose();
                }
                else
                {
                    ShowInfoBar("Failed to generate preview", InfoBarSeverity.Error);
                }
            }
            catch (Exception ex)
            {
                ShowInfoBar($"Preview error: {ex.Message}", InfoBarSeverity.Error);
            }
        }

        private void GenerateSeatMap()
        {
            SeatMapGrid.Children.Clear();
            ShowInfoBar($"Generating seat map with {_seatMap.Count} seats", InfoBarSeverity.Informational);

            // Sort seats by row number first, then by column letter
            var sortedSeats = _seatMap.Keys.OrderBy(seat => 
            {
                if (seat.Length >= 2 && int.TryParse(seat.Substring(0, seat.Length - 1), out int row))
                {
                    return row * 100 + (seat.Last() - 'A'); // Sort by row, then by column
                }
                return 0;
            });

            foreach (var seat in sortedSeats)
            {
                // Parse seat number (e.g., "1A", "2B", etc.)
                string seatNumber = seat;
                int row = 0;
                int col = 0;
                
                if (seatNumber.Length >= 2)
                {
                    // Extract row number (e.g., "1" from "1A")
                    string rowStr = seatNumber.Substring(0, seatNumber.Length - 1);
                    if (int.TryParse(rowStr, out int rowNum))
                    {
                        row = Math.Max(0, rowNum - 1); // Convert to 0-based index
                    }
                    
                    // Extract column letter (e.g., "A" from "1A")
                    char colChar = seatNumber.Last();
                    col = colChar switch
                    {
                        'A' => 0,
                        'B' => 1,
                        'C' => 2,
                        'D' => 3,
                        'E' => 4,
                        'F' => 5,
                        'G' => 6,
                        'H' => 7,
                        'J' => 8,
                        _ => 0
                    };
                }

                Button seatButton = new Button
                {
                    Content = seat,
                    Tag = seat,
                    Margin = new Thickness(2),
                    Width = 50,
                    Height = 50,
                    Background = _seatMap[seat] ? new SolidColorBrush(Colors.LightCoral) : new SolidColorBrush(Colors.LightBlue)
                };

                seatButton.Click += SeatButton_Click;
                Grid.SetRow(seatButton, row);
                Grid.SetColumn(seatButton, col);
                SeatMapGrid.Children.Add(seatButton);
            }
        }

        private void SeatButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedSeat = sender as Button;
            string seatNumber = clickedSeat.Tag.ToString();

            if (_seatMap[seatNumber]) return; // already occupied

            _selectedSeat = seatNumber;
            SelectedSeatText.Text = $"Selected Seat: {seatNumber}";

            foreach (var child in SeatMapGrid.Children.OfType<Button>())
            {
                if (!_seatMap[(string)child.Tag])
                {
                    child.Background = new SolidColorBrush(Colors.LightBlue);
                }
            }

            clickedSeat.Background = new SolidColorBrush(Colors.SlateBlue);
        }

        private void ResetSeatSelection()
        {
            foreach (var child in SeatMapGrid.Children.OfType<Button>())
            {
                string seatNumber = child.Tag.ToString();
                child.Background = _seatMap[seatNumber] ? new SolidColorBrush(Colors.LightCoral) : new SolidColorBrush(Colors.LightBlue);
            }
        }

        private async void SeatSelectionDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (_currentPassenger != null && !string.IsNullOrEmpty(_selectedSeat))
            {
                try
                {
                    // ShowInfoBar("Assigning seat...", InfoBarSeverity.Informational);
                    
                    var checkInResponse = await _apiService.CheckInPassengerAsync(_currentPassenger.Passport, _selectedSeat);
                    
                    if (checkInResponse.Success)
                    {
                        _seatMap[_selectedSeat] = true;
                        _currentPassenger.Seat = _selectedSeat;
                        _currentPassenger.Status = "Checked In";

                        CurrentSeatText.Text = _currentPassenger.Seat;
                        PassengerStatusText.Text = _currentPassenger.Status;
                        // ShowInfoBar($"Seat {_selectedSeat} assigned successfully!", InfoBarSeverity.Success);
                    }
                    else
                    {
                        ShowInfoBar($"Failed to assign seat: {checkInResponse.Message}", InfoBarSeverity.Error);
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
                ShowInfoBar("Please select a seat first.", InfoBarSeverity.Error);
                args.Cancel = true;
            }
        }

        private void SeatSelectionDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _selectedSeat = null;
        }

        // Clean up SignalR connection when page is unloaded
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
