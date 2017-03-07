using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;
using Windows.ApplicationModel.Activation;
// 空白頁項目範本已記錄在 http://go.microsoft.com/fwlink/?LinkId=234238

namespace App1
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    /*public sealed partial class ExtendedSplash : Page
    {
        public ExtendedSplash()
        {
            this.InitializeComponent();
        }
    }*/
    partial class ExtendedSplash
    {
        internal Rect splashImageRect; // Rect to store splash screen image coordinates.
        internal bool dismissed = false; // Variable to track splash screen dismissal status.
        internal Frame rootFrame;

        private SplashScreen splash; // Variable to hold the splash screen object.

        private DispatcherTimer _timer_enter;
        private double _timer_enter_sec = 3;
        public ExtendedSplash(SplashScreen splashscreen, bool loadState)
        {
            InitializeComponent();

            //LearnMoreButton.Click += new RoutedEventHandler(LearnMoreButton_Click);
            // Listen for window resize events to reposition the extended splash screen image accordingly.
            // This is important to ensure that the extended splash screen is formatted properly in response to snapping, unsnapping, rotation, etc...
            Window.Current.SizeChanged += new WindowSizeChangedEventHandler(ExtendedSplash_OnResize);

            splash = splashscreen;

            if (splash != null)
            {
                // Register an event handler to be executed when the splash screen has been dismissed.
                splash.Dismissed += new TypedEventHandler<SplashScreen, Object>(DismissedEventHandler);

                // Retrieve the window coordinates of the splash screen image.
                splashImageRect = splash.ImageLocation;
                PositionImage();
            }

            // Create a Frame to act as the navigation context
            rootFrame = new Frame();

            // Restore the saved session state if necessary
            RestoreStateAsync(loadState);

            //3sec to enter main page
            _timer_enter = new DispatcherTimer();
            _timer_enter.Interval = TimeSpan.FromSeconds(0.1);
            _timer_enter.Tick += Timer_Enter_Tick;
            _timer_enter.Start();
        }

        private async void Timer_Enter_Tick(object sender, object e)
        {
            if (_timer_enter_sec > 0.0)
            {
                _timer_enter_sec -= 0.1;
                /*if (_timer_enter_sec < 2.0)
                {
                       
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            try
                            {
                                Starting_Ring.Height -= 5 / _timer_enter_sec;
                                Starting_Ring.Width -= 5 / _timer_enter_sec;
                            }
                            catch (Exception ex) { }
                        });
                    
                }*/
            }
            else
            {
                _timer_enter.Tick -= Timer_Enter_Tick;
                _timer_enter.Stop();
                // Navigate to mainpage
                rootFrame.Navigate(typeof(MainPage));

                // Set extended splash info on main page
                //((MainPage)rootFrame.Content).SetExtendedSplashInfo(splashImageRect, dismissed);

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }
        }

        async void RestoreStateAsync(bool loadState)
        {
            if (loadState)
                await SuspensionManager.RestoreAsync();

            // Normally you should start the time consuming task asynchronously here and
            // dismiss the extended splash screen in the completed handler of that task
            // This sample dismisses extended splash screen  in the handler for "Learn More" button for demonstration
        }
        
        // Position the extended splash screen image in the same location as the system splash screen image.
        void PositionImage()
        {
            extendedSplashImage.SetValue(Canvas.LeftProperty, splashImageRect.X);
            extendedSplashImage.SetValue(Canvas.TopProperty, splashImageRect.Y);
            extendedSplashImage.Height = splashImageRect.Height;
            extendedSplashImage.Width = splashImageRect.Width;
        }

        void ExtendedSplash_OnResize(Object sender, WindowSizeChangedEventArgs e)
        {
            // Safely update the extended splash screen image coordinates. This function will be fired in response to snapping, unsnapping, rotation, etc...
            if (splash != null)
            {
                // Update the coordinates of the splash screen image.
                splashImageRect = splash.ImageLocation;
                PositionImage();
                
                
            }
        }
        /*
        void LearnMoreButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to mainpage
            rootFrame.Navigate(typeof(MainPage));

            // Set extended splash info on main page
            ((MainPage)rootFrame.Content).SetExtendedSplashInfo(splashImageRect, dismissed);

            // Place the frame in the current Window
            Window.Current.Content = rootFrame;
        }
        */
        // Include code to be executed when the system has transitioned from the splash screen to the extended splash screen (application's first view).
        void DismissedEventHandler(SplashScreen sender, object e)
        {
            dismissed = true;

            // Navigate away from the app's extended splash screen after completing setup operations here...
            // This sample navigates away from the extended splash screen when the "Learn More" button is clicked.
        }

        private void PlayMeButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to mainpage
            rootFrame.Navigate(typeof(MainPage));
            
            // Set extended splash info on main page
            //((MainPage)rootFrame.Content).SetExtendedSplashInfo(splashImageRect, dismissed);

            // Place the frame in the current Window
            Window.Current.Content = rootFrame;
        }
    }
}
