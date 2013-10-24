using System;
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
            //BuildLocalizedApplicationBar();

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
            //AroundMeMap.SetView(gpsCoorCenter, 17D);
            AroundMeMap.Center = gpsCoorCenter;
            AroundMeMap.ZoomLevel = 15;

            ResultTextBlock.Text = string.Format("{0} - {1}", AroundMeMap.Center.Latitude, AroundMeMap.Center.Longitude);

            HttpClient client = new HttpClient();
            //Licenses:
            //http://www.flickr.com/services/api/flickr.photos.licenses.getinfo.html
            /*
                    <license id="4" name="Attribution License" url="http://creativecommons.org/licenses/by/2.0/" />
                    <license id="5" name="Attribution-ShareAlike License" url="http://creativecommons.org/licenses/by-sa/2.0/" />
                    <license id="6" name="Attribution-NoDerivs License" url="http://creativecommons.org/licenses/by-nd/2.0/" />
                    <license id="7" name="No known copyright restrictions" url="http://flickr.com/commons/usage/" />

            */

            string license = "4,5,6,7";
            license.Replace(",", "%2C"); //How comma is represented in a url

            //Your API Key
            string flickrApiKey = "8bc58aadfa85d0fb22f0e820f4df93fc";

            string url = "http://api.flickr.com/services/rest/" +
            "?method=flickr.photos.search" +
            "&license={0}"+
            "&api_key={1}" +
            "&lat={2}" +
            "&lon={3}" +
            "&radius=2" +
            "&format=json" +
            "&nojsoncallback=1";
            var baseurl = string.Format(url,license,flickrApiKey, gpsCoorCenter.Latitude, gpsCoorCenter.Longitude);

            string flickrResult = await client.GetStringAsync(baseurl);
            //ResultTextBlock.Text = flickrResult;
            FlickrData apidata = JsonConvert.DeserializeObject<FlickrData>(flickrResult);

            if (apidata.stat == "ok")
            {
                foreach (Photo data in apidata.photos.photo)
                {
                    //To retrieve one photo, use this format
                    //http://farm{farmid}.staticflickr.com/{server-id}/{id}_{secret}{size}.jpg
                    string photoUrl = "http://farm{0}.staticflickr.com/{1}/{2}_{3}_n.jpg";
                    string baseFlickrUrl = string.Format(photoUrl, data.farm, data.server, data.id, data.secret);
                    FlickrImage.Source = new BitmapImage(new Uri(baseFlickrUrl));
                    //just trying to grab one photo, so break
                    break;
                }
            }

        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
    public class Photo
    {
        public string id { get; set; }
        public string owner { get; set; }
        public string secret { get; set; }
        public string server { get; set; }
        public int farm { get; set; }
        public string title { get; set; }
        public int ispublic { get; set; }
        public int isfriend { get; set; }
        public int isfamily { get; set; }
    }

    public class Photos
    {
        public int page { get; set; }
        public int pages { get; set; }
        public int perpage { get; set; }
        public string total { get; set; }
        public List<Photo> photo { get; set; }
    }

    public class FlickrData
    {
        public Photos photos { get; set; }
        public string stat { get; set; }
    }
}