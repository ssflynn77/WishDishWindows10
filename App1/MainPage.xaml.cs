using DisneyLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace WishDish
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ObservableCollection<int> partySizes = new ObservableCollection<int>();
        ObservableCollection<Restaurant> diningLocations = new ObservableCollection<Restaurant>();

        DateTimeOffset previouslySelectedDate = DateTimeOffset.Now.AddDays(1);

        public MainPage()
        {
            diningLocations = new ObservableCollection<Restaurant>(Restaurant.GetRestaurantsFromSite());
            diningLocations.Insert(0, new Restaurant(0, "All Locations"));
            
            this.InitializeComponent();

            // Create Party Sizes
            for (int x = 1; x < 50; x++)
            { partySizes.Add(x); }
            partySizePicker.SelectedItem = 4;

            // Set Min & Max Dates
            searchDatePicker.Opened += (sender, e) =>
            {
                searchDatePicker.MinDate = DateTimeOffset.Now;
                searchDatePicker.MaxDate = DateTimeOffset.Now.AddDays(180);
                searchDatePicker.SetDisplayDate ( previouslySelectedDate);
                searchDatePicker.Date = previouslySelectedDate;
            };

            searchDatePicker.Closed += (sender, e) =>
            {
                previouslySelectedDate = searchDatePicker.Date ?? DateTimeOffset.Now.AddDays(1);
            };

            closeOfferButton.Click += CloseOfferButton_Click;

            diningLocationPicker.Loaded += (e, e1) => { diningLocationPicker.SelectedIndex = 0; };
            SetTitleBarColors();
        }

        private void SetTitleBarColors()
        {
            // Example code for setting the title bar found
            //  https://docs.microsoft.com/en-us/windows/uwp/design/shell/title-bar

            var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;

            // Set active window colors
            titleBar.ForegroundColor = Windows.UI.Colors.White;
            titleBar.BackgroundColor = Windows.UI.Colors.Green;
            titleBar.ButtonForegroundColor = Windows.UI.Colors.White;
            titleBar.ButtonBackgroundColor = Windows.UI.Colors.SeaGreen;
            titleBar.ButtonHoverForegroundColor = Windows.UI.Colors.White;
            titleBar.ButtonHoverBackgroundColor = Windows.UI.Colors.DarkSeaGreen;
            titleBar.ButtonPressedForegroundColor = Windows.UI.Colors.Gray;
            titleBar.ButtonPressedBackgroundColor = Windows.UI.Colors.LightGreen;

            // Set inactive window colors
            titleBar.InactiveForegroundColor = Windows.UI.Colors.Gray;
            titleBar.InactiveBackgroundColor = Windows.UI.Colors.SeaGreen;
            titleBar.ButtonInactiveForegroundColor = Windows.UI.Colors.Gray;
            titleBar.ButtonInactiveBackgroundColor = Windows.UI.Colors.SeaGreen;
        }

        private void CloseOfferButton_Click(object sender, RoutedEventArgs e)
        {
            if (offersPivot.Items.Count == 0) return;
            offersPivot.Items.RemoveAt(offersPivot.SelectedIndex);
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            var searchDate = searchDatePicker.Date;
            var searchTime = searchTimePicker.Time;

            var seachDateAsString = searchDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var searchTimeAsString = $"{searchTime.Hours}:{searchTime.Minutes}";


            if (diningLocationPicker.SelectedIndex == 0)
            {
                var availability = await DisneyRequests.GetTimesAsync(seachDateAsString, searchTimeAsString, (int)partySizePicker.SelectedValue);
                var headerText = $"{searchDate.Value.ToString("M/d")} @ {new DateTime(searchTime.Ticks).ToString("h:mm t")}";
                addGridPivot(availability, headerText);
            }
            else
            {
                var selectedRestaurant = diningLocationPicker.SelectedValue as Restaurant;
                var restAvail = await DisneyRequests.GetRestaurantTimes(selectedRestaurant, seachDateAsString, searchTimeAsString, (int)partySizePicker.SelectedValue);

                if (restAvail.TimesAvailabile.Count > 0)
                {
                    var headerText = $"{selectedRestaurant.Name} - {searchDate.Value.ToString("M/d")} @ {new DateTime(searchTime.Ticks).ToString("h:mm t")}";
                    addRestaurantPivot(restAvail, headerText);
                }
                else
                {
                    ContentDialog noWifiDialog = new ContentDialog
                    {
                        Title = $"{selectedRestaurant.Name}: {restAvail.InfoTitle}",
                        Content = $"{searchDate.Value.ToString("M / d")} @ {new DateTime(searchTime.Ticks).ToString("h:mm t")}\r\n{restAvail.InfoText}",
                        CloseButtonText = "Ok"
                    };

                    ContentDialogResult result = await noWifiDialog.ShowAsync();
                }
            }

        }

        void addGridPivot(IEnumerable<Availability> availability, string header)
        {
            var grid = new resultsGrid(availability.Where(x => x.Offers.Count > 1).ToList());
            var newItem = new PivotItem() { Header = header, };
            newItem.Content = grid;
            offersPivot.Items.Add(newItem);
        }

        void addRestaurantPivot(RestaurantAvail availability, string header)
        {
            var grid = new restaurantResultsGrid(availability);
            var newItem = new PivotItem() { Header = header, };
            newItem.Content = grid;
            offersPivot.Items.Add(newItem);
        }

    }
}
