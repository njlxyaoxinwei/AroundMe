using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media.Animation;
using System.Diagnostics;

namespace AroundMe
{
    public partial class SearchResults : PhoneApplicationPage
    {
        private double _latitude;
        private double _longitude;
        private const string flickrApiKey = "8bc58aadfa85d0fb22f0e820f4df93fc";
        private string _topic;
        private double _radius;
        public SearchResults()
        {
            InitializeComponent();
            Loaded += SearchResults_Loaded;
            BuildLocalizedApplicationBar();
        }

        async void SearchResults_Loaded(object sender, RoutedEventArgs e)
        {
            Overlay.Visibility = Visibility.Visible;
            OverlayProgressBar.IsIndeterminate = true;

            var images = await FlickrImage.GetFlickrImages(flickrApiKey, _topic, _latitude, _longitude, _radius);
            DataContext = images;

            if (images.Count == 0)
                NoPhotosFound.Visibility = Visibility.Visible;
            else
                NoPhotosFound.Visibility = Visibility.Collapsed;
            Overlay.Visibility = Visibility.Collapsed;
            OverlayProgressBar.IsIndeterminate = false;

        }

        // Sample code for building a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {

            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsVisible = false;
            // Create a new button and set the text value to the localized string from AppResources.
            ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Check.png", UriKind.Relative));
            appBarButton.Text = "set";
            appBarButton.Click += appBarButton_Click;
            ApplicationBar.Buttons.Add(appBarButton);
        }

        private void appBarButton_Click(object sender, EventArgs e)
        {
            List<FlickrImage> imgs = new List<FlickrImage>();

            foreach (object item in PhotosForLockscreen.SelectedItems)
            {
                FlickrImage img = item as FlickrImage;
                if (img != null)
                    imgs.Add(img);

            }
            //remove all images in IsolatedStorage
            LockscreenHelpers.CleanStorage();
            //Save new list
            LockscreenHelpers.SaveSelectedBackgroundScreens(imgs);
            //Randomly select one item as lockscreen
            LockscreenHelpers.SetRandomImageFromLocalStorage();

            MessageBox.Show("You have a new background!", "Set!",MessageBoxButton.OK);

        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _latitude = Convert.ToDouble(NavigationContext.QueryString["latitude"]);
            _longitude = Convert.ToDouble(NavigationContext.QueryString["longitude"]);
            _topic = NavigationContext.QueryString["topic"];
            _radius = Convert.ToDouble(NavigationContext.QueryString["radius"]);


        }

        private void PhotosForLockscreen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PhotosForLockscreen.SelectedItems.Count == 0)
                ApplicationBar.IsVisible = false;
            else
                ApplicationBar.IsVisible = true;
        }

        private void Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            Image img = sender as Image;
            if (img == null)
                return;
            Storyboard s = new Storyboard();
            DoubleAnimation doubleAni = new DoubleAnimation();
            doubleAni.To = 1;
            doubleAni.Duration = new Duration(TimeSpan.FromMilliseconds(500));
            Storyboard.SetTarget(doubleAni, img);
            Storyboard.SetTargetProperty(doubleAni, new PropertyPath(OpacityProperty));
            s.Children.Add(doubleAni);
            s.Begin(); //Have to code this in C# because we cannot target elements in the List in XAML.

        }


    }
}