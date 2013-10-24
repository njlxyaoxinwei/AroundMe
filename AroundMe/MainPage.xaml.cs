﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using AroundMe.Resources;
using System.Device.Location;
using Windows.Devices.Geolocation;
using System.Net.Http;
using Newtonsoft.Json;
using System.Windows.Media.Imaging;

namespace AroundMe
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            BuildLocalizedApplicationBar();

            Loaded += MainPage_Loaded;
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateMap();
        }

        private async void UpdateMap()
        {
            //AroundMeMap.SetView(new GeoCoordinate(41.8988D, -87.6231D), 17D);
            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;
            Geoposition position =
                await geolocator.GetGeopositionAsync(TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(30));

            var gpsCoorCenter =
                new GeoCoordinate(position.Coordinate.Latitude, position.Coordinate.Longitude);

            AroundMeMap.Center = gpsCoorCenter;
            AroundMeMap.ZoomLevel = 15;


        }

        // Sample code for building a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            // Create a new button and set the text value to the localized string from AppResources.
            ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/feature.search.png", UriKind.Relative));
            appBarButton.Text = "search";
            appBarButton.Click += SearchClick;
            ApplicationBar.Buttons.Add(appBarButton);

            // Create a new menu item with the localized string from AppResources.
            ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
            ApplicationBar.MenuItems.Add(appBarMenuItem);
        }

        private void SearchClick(object sender, EventArgs e)
        {
            string topic = HttpUtility.UrlEncode(SearchTopic.Text);
            string navTo = string.Format("/SearchResults.xaml?latitude={0}&longitude={1}&topic={2}&radius={3}",AroundMeMap.Center.Latitude, AroundMeMap.Center.Longitude, topic,5);
            //the radius is actually hard-coded to be 5km
            NavigationService.Navigate(new Uri(navTo, UriKind.RelativeOrAbsolute));
        }
    }

}