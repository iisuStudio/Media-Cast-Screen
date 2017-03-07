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

using Windows.Media.PlayTo;
using Windows.Media;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Connectivity;

// 空白頁項目範本已記錄在 http://go.microsoft.com/fwlink/?LinkId=234238

namespace App1
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //宣告區塊
        //MainPage rootPage = MainPage.Current;
        PlayToReceiver receiver = null;
        bool IsReceiverStarted = false;
        bool IsSeeking = false;
        double bufferedPlaybackRate = 0;
        bool justLoadedMedia = false;
        bool IsPlayReceivedPreMediaLoaded = false;
        enum MediaType { None, Image, AudioVideo };
        MediaType currentType = MediaType.None;
        BitmapImage imagerevd = null;
        private DispatcherTimer _timer;
        //開發者版面
        private bool dev_UI = false;
        private bool first_Start = true;
        //控制器顯示計數器
        private DispatcherTimer _timer_popup;
        int popup_sec = 5;
        private bool _sliderPressed = false;
        SystemMediaTransportControls systemControls;
        void InitializeTransportControls()
        {
            // Hook up app to system transport controls.
            systemControls = SystemMediaTransportControls.GetForCurrentView();
            //systemControls.ButtonPressed += SystemControls_ButtonPressed;

            // Register to handle the following system transpot control buttons.
            systemControls.IsPlayEnabled = true;
            systemControls.IsPauseEnabled = true;
            systemControls.IsStopEnabled = true;
            
        }

        public MainPage()
        {
            this.InitializeComponent();
            this.InitializeTransportControls();
            this.startPlayToReceiver();
            //dmrVideo.Source = new Uri("ms-appx:///Assets/Start_video.mp4");
            _timer = new DispatcherTimer();
            _timer_popup = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
            TimelineSlider.AddHandler(Slider.PointerPressedEvent, new PointerEventHandler(TimelineSlider_PointerPressed), true);
            TimelineSlider.AddHandler(Slider.PointerReleasedEvent, new PointerEventHandler(TimelineSlider_PointerReleased), true);
            if (dev_UI == false)
            {
                //dmrVideo.MediaOpened += First_Play;
                dmrImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/Ready.png"));
                
            }
        }

        private void First_Play(object sender, object e)
        {
            SetFullWindowMode(true);
            dmrVideo.MediaOpened -= First_Play;
        }
        //PlayTo區塊
        /// <summary>
        /// This is the click handler for the 'Default' button.  You would replace this with your own handler
        /// if you have a button or buttons on this page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async private void startPlayToReceiver()
        {
                try
                {
                    InitialisePlayToReceiver();
                    await receiver.StartAsync();
                    IsReceiverStarted = true;
                }
                catch (Exception ecp)
                {
                    IsReceiverStarted = false;
                    ErrorTextBlock.Text = ecp.Message;
                }
        }
        

        /// <summary>
        /// This is the click handler for the 'Other' button.  You would replace this with your own handler
        /// if you have a button or buttons on this page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async private void stopPlayToReceiver()
        {
            
                try
                {
                    await receiver.StopAsync();
                    IsReceiverStarted = false;
                }
                catch (Exception ecp)
                {
                    IsReceiverStarted = true;
                    ErrorTextBlock.Text = ecp.Message;
                }
        }
        
        private void InitialisePlayToReceiver()
        {
            try
            {
                if (receiver == null)
                {
                    // Get Computer Name
                    var hostNames = NetworkInformation.GetHostNames();
                    var localName = hostNames.FirstOrDefault(name => name.DisplayName.Contains(".local"));
                    string computerName = localName.DisplayName.Replace(".local", "");
                    
                    receiver = new PlayToReceiver() { FriendlyName = "Media play Surface " + computerName };
                    

                    receiver.PlayRequested += new TypedEventHandler<PlayToReceiver, object>(receiver_PlayRequested);
                    receiver.PauseRequested += new TypedEventHandler<PlayToReceiver, object>(receiver_PauseRequested);
                    receiver.StopRequested += new TypedEventHandler<PlayToReceiver, object>(receiver_StopRequested);
                    receiver.TimeUpdateRequested += new TypedEventHandler<PlayToReceiver, object>(receiver_TimeUpdateRequested);
                    receiver.CurrentTimeChangeRequested += new TypedEventHandler<PlayToReceiver, CurrentTimeChangeRequestedEventArgs>(receiver_CurrentTimeChangeRequested);
                    receiver.SourceChangeRequested += new TypedEventHandler<PlayToReceiver, SourceChangeRequestedEventArgs>(receiver_SourceChangeRequested);
                    receiver.MuteChangeRequested += new TypedEventHandler<PlayToReceiver, MuteChangeRequestedEventArgs>(receiver_MuteChangeRequested);
                    receiver.PlaybackRateChangeRequested += new TypedEventHandler<PlayToReceiver, PlaybackRateChangeRequestedEventArgs>(receiver_PlaybackRateChangeRequested);
                    receiver.VolumeChangeRequested += new TypedEventHandler<PlayToReceiver, VolumeChangeRequestedEventArgs>(receiver_VolumeChangeRequested);
                    

                    receiver.SupportsAudio = true;
                    receiver.SupportsVideo = true;
                    receiver.SupportsImage = true;

                    
                }
            }
            catch (Exception e)
            {
                
            }
        }

        private async void receiver_PlayRequested(PlayToReceiver recv, Object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                if (dmrVideo != null && currentType == MediaType.AudioVideo)
                {
                    IsPlayReceivedPreMediaLoaded = true;
                    dmrVideo.Play();
                }
                else if (currentType == MediaType.Image)
                {
                    dmrImage.Source = imagerevd;
                    receiver.NotifyPlaying();
                    
                }
            });
        }

        private async void receiver_PauseRequested(PlayToReceiver recv, Object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                if (dmrVideo != null && currentType == MediaType.AudioVideo)
                {
                    if (dmrVideo.CurrentState == MediaElementState.Stopped)
                    {
                        receiver.NotifyPaused();
                    }
                    else
                    {
                        dmrVideo.Pause();
                    }
                }
            });
        }

        private async void receiver_StopRequested(PlayToReceiver recv, Object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                if (dmrVideo != null && currentType == MediaType.AudioVideo)
                {
                    dmrVideo.Stop();
                    receiver.NotifyStopped();
                }
                else if (dmrImage != null && currentType == MediaType.Image)
                {
                    dmrImage.Source = null;
                    receiver.NotifyStopped();
                }
            });
        }

        private async void receiver_TimeUpdateRequested(PlayToReceiver recv, Object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                if (IsReceiverStarted)
                {
                    if (dmrVideo != null && currentType == MediaType.AudioVideo)
                    {
                        receiver.NotifyTimeUpdate(dmrVideo.Position);
                    }
                    else if (dmrImage != null && currentType == MediaType.Image)
                    {
                        receiver.NotifyTimeUpdate(new TimeSpan(0));
                    }
                }
            });
        }

        private async void receiver_CurrentTimeChangeRequested(PlayToReceiver recv, CurrentTimeChangeRequestedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                if (IsReceiverStarted)
                {
                    if (dmrVideo != null && currentType == MediaType.AudioVideo)
                    {
                        if (dmrVideo.CanSeek)
                        {
                            dmrVideo.Position = args.Time;
                            
                            receiver.NotifySeeking();
                            IsSeeking = true;
                        }
                    }
                    else if (currentType == MediaType.Image)
                    {
                        receiver.NotifySeeking();
                        receiver.NotifySeeked();
                    }
                }
            });
        }

        private async void receiver_SourceChangeRequested(PlayToReceiver recv, SourceChangeRequestedEventArgs args)
        {
            IsPlayReceivedPreMediaLoaded = false;
            if (args.Stream == null)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    if (currentType == MediaType.AudioVideo && dmrVideo != null)
                    {
                        dmrVideo.Stop();
                    }
                    else if (currentType == MediaType.Image && dmrImage != null)
                    {
                        dmrImage.Source = null;
                        dmrImage.Opacity = 0;
                        if (dev_UI == false)
                        {
                            //開啟影片播放器全螢幕模式
                            SetFullWindowMode(true);
                        }
                    }
                    currentType = MediaType.None;
                });
            }
            else if (args.Stream.ContentType.Contains("image"))
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    imagerevd = new BitmapImage();
                    imagerevd.ImageOpened += imagerevd_ImageOpened;
                    imagerevd.SetSource(args.Stream);
                    if (currentType != MediaType.Image)
                    {
                        if (currentType == MediaType.AudioVideo)
                        {
                            dmrVideo.Stop();
                        }
                        dmrImage.Opacity = 1;
                        dmrVideo.Opacity = 0;
                        if (dev_UI == false)
                        {
                            //關閉影片播放器全螢幕模式
                            SetFullWindowMode(false);
                        }
                    }
                    currentType = MediaType.Image;
                });
            }
            else
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    try
                    {
                        justLoadedMedia = true;
                        dmrVideo.SetSource(args.Stream, args.Stream.ContentType);

                    }
                    catch (Exception exp)
                    {
                        
                    }

                    if (currentType == MediaType.Image)
                    {
                        dmrImage.Opacity = 0;
                        dmrVideo.Opacity = 1;
                        dmrImage.Source = null;
                        if (dev_UI == false)
                        {
                            //開啟影片播放器全螢幕模式
                            SetFullWindowMode(true);
                        }
                    }
                    currentType = MediaType.AudioVideo;
                });
            }
        }

        void imagerevd_ImageOpened(object sender, RoutedEventArgs e)
        {
            receiver.NotifyLoadedMetadata();
        }

        private async void receiver_MuteChangeRequested(PlayToReceiver recv, MuteChangeRequestedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                if (dmrVideo != null && currentType == MediaType.AudioVideo)
                {
                    dmrVideo.IsMuted = args.Mute;
                }
                else if (dmrImage != null && currentType == MediaType.Image)
                {
                    receiver.NotifyVolumeChange(0, args.Mute);
                }
            });
        }

        private async void receiver_PlaybackRateChangeRequested(PlayToReceiver recv, PlaybackRateChangeRequestedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                if (dmrVideo != null && currentType == MediaType.AudioVideo)
                {
                    if (dmrVideo.CurrentState != MediaElementState.Opening && dmrVideo.CurrentState != MediaElementState.Closed)
                    {
                        dmrVideo.PlaybackRate = args.Rate;
                    }
                    else
                    {
                        bufferedPlaybackRate = args.Rate;
                    }
                }
            });
        }

        private async void receiver_VolumeChangeRequested(PlayToReceiver recv, VolumeChangeRequestedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                if (dmrVideo != null && currentType == MediaType.AudioVideo)
                {
                    dmrVideo.Volume = args.Volume;
                }
            });
        }

        private void dmrVideo_VolumeChanged(object sender, RoutedEventArgs e)
        {
            if (IsReceiverStarted)
            {
                receiver.NotifyVolumeChange(dmrVideo.Volume, dmrVideo.IsMuted);
            }
        }

        private void dmrVideo_RateChanged(object sender, Windows.UI.Xaml.Media.RateChangedRoutedEventArgs e)
        {
            if (IsReceiverStarted)
            {
                receiver.NotifyRateChange(dmrVideo.PlaybackRate);
            }

        }

        private void dmrVideo_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (IsReceiverStarted)
            {
                receiver.NotifyLoadedMetadata();
                receiver.NotifyDurationChange(dmrVideo.NaturalDuration.TimeSpan);
                if (IsPlayReceivedPreMediaLoaded == true)
                {
                    dmrVideo.Play();
                    
                }
                TimelineSlider.Maximum = Math.Round(dmrVideo.NaturalDuration.TimeSpan.TotalSeconds, MidpointRounding.AwayFromZero);
                TimelineSlider.StepFrequency = CalculateSliderFrequency(dmrVideo.NaturalDuration.TimeSpan);

                SetupTimer();
            }
        }

        private void dmrVideo_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            if (IsReceiverStarted)
            {
                switch (dmrVideo.CurrentState)
                {
                    case MediaElementState.Playing:
                        receiver.NotifyPlaying();
                        systemControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                        //設定時間
                        SetTextBlockTime();
                        //第一次播放介面BUG
                        dmrImage.Opacity = 0;
                        SetFullWindowMode(true);
                        
                        break;
                    case MediaElementState.Paused:
                        if (justLoadedMedia)
                        {
                            receiver.NotifyStopped();
                            justLoadedMedia = false;
                            systemControls.PlaybackStatus = MediaPlaybackStatus.Stopped;

                        }
                        else
                        {
                            receiver.NotifyPaused();
                            systemControls.PlaybackStatus = MediaPlaybackStatus.Paused;
                        }
                        break;
                    case MediaElementState.Stopped:
                        receiver.NotifyStopped();
                        systemControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
                        break;
                    default:
                        break;
                }

            }
        }

        private void dmrVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (IsReceiverStarted)
            {
                receiver.NotifyEnded();
                if (dmrVideo != null)
                {
                    dmrVideo.Stop();
                    
                }
                _timer.Stop();
                _timer.Tick -= Timer_Tick;

                TimelineSlider.Value = 0.0;    
            }
        }

        private void dmrVideo_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            if (IsReceiverStarted)
            {
                receiver.NotifyError();
            }
        }

        private void dmrVideo_SeekCompleted(object sender, RoutedEventArgs e)
        {
            if (IsReceiverStarted)
            {
                try
                {
                    if (!IsSeeking)
                    {
                        receiver.NotifySeeking();
                    }
                    receiver.NotifySeeked();
                    IsSeeking = false;
                }
                catch (InvalidOperationException exp)
                {

                }
            }
        }

        private void dmrVideo_DownloadProgressChanged_1(object sender, RoutedEventArgs e)
        {
            if (dmrVideo.DownloadProgress == 1 && bufferedPlaybackRate > 0)
            {
                dmrVideo.PlaybackRate = bufferedPlaybackRate;
                bufferedPlaybackRate = 0;
            }
        }

        private void dmrVideo_TappedFullScreen(object sender, RoutedEventArgs e)
        {
            /*
            dmrVideo.IsFullWindow = !dmrVideo.IsFullWindow;
            PlayerController.Width = ActualWidth;
            if (dmrVideo.IsFullWindow)
            {
                PlayerController.Margin = new Thickness
                {
                    Left = 0,
                    Bottom = 0
                };
                TimelineSlider.Width = PlayerController.Width -  415;
            }
            else
            {
                PlayerController.Margin = new Thickness
                {
                    Left = 35,
                    Bottom = 210
                };
                TimelineSlider.Width = dmrVideo.Width - 256;
            }*/
            //SetFullWindowMode(!dmrVideo.IsFullWindow);
            
        }

        private void SetFullWindowMode(bool isFullWindow)
        {
            dmrVideo.IsFullWindow = isFullWindow;
            
            if (isFullWindow)
            {
                // When displaying in full-window mode, center transport controls at the bottom of the window

                // Since the Popup is in the Output Grid, the offset must account for its parent
                var rootFrame = Window.Current.Content as Frame;
                var outputGridOffset = Output.TransformToVisual(rootFrame).TransformPoint(new Point(0, 0));

                PlayerController.HorizontalOffset = (rootFrame.ActualWidth - PlayerController.ActualWidth)/2 - outputGridOffset.X;
                PlayerController.VerticalOffset = rootFrame.ActualHeight - PlayerController.ActualHeight - outputGridOffset.Y;
            }
            else
            {
                // When displaying in embedded mode, center transport controls at the bottom of the MediaElement
                PlayerController.HorizontalOffset = (dmrVideo.Width - PlayerController.ActualWidth)/2;
                PlayerController.VerticalOffset = dmrVideo.Height - PlayerController.ActualHeight;
                
                dmrImage.Height = this.ActualHeight;
                dmrImage.Width = this.ActualWidth;
            }
        }

        private void dmrImage_ImageFailed_1(object sender, ExceptionRoutedEventArgs e)
        {
            receiver.NotifyError();
        }


        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            dmrVideo.Play();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            dmrVideo.Pause();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            dmrVideo.Stop();
        }


        

        private void dmrVideo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            popup_sec = 5;
            if (PlayerController.IsOpen == false)
            {
                PlayerController.IsOpen = true;
                _timer_popup.Tick += Timer_Popup_Tick;
                _timer_popup.Start();
            }
        }
        private void dmrVideo_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            popup_sec = 5;
            
        }
        private void PlayerController_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            popup_sec = 5;
        }
        private void Timer_Popup_Tick(object sender, object e) 
        {
            if (popup_sec > 0)
            {
                popup_sec--;
            }
            else
            {
                _timer_popup.Tick -= Timer_Popup_Tick;
                _timer_popup.Stop();
                PlayerController.IsOpen = false;
            }
        }
        private void dmrImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            
        }

        private void dmrVideo_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            
            
        }

        #region Timeline Slider interaction

        void TimelineSlider_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _sliderPressed = true;
        }

        void TimelineSlider_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (_sliderPressed)
            {
                dmrVideo.Position = TimeSpan.FromSeconds(TimelineSlider.Value);
                _sliderPressed = false;
            }
        }

        private double CalculateSliderFrequency(TimeSpan timevalue)
        {
            // Calculate the slider step frequency based on the timespan length
            double stepFrequency = 1;

            if (timevalue.TotalHours >= 1)
            {
                stepFrequency = 60;
            }
            else if (timevalue.TotalMinutes > 30)
            {
                stepFrequency = 30;
            }
            else if (timevalue.TotalMinutes > 10)
            {
                stepFrequency = 10;
            }
            else
            {
                stepFrequency = Math.Round(timevalue.TotalSeconds / 100, MidpointRounding.AwayFromZero);
            }

            return stepFrequency;
        }
        private void SetupTimer()
        {
            if (!_timer.IsEnabled)
            {
                _timer.Interval = TimeSpan.FromSeconds(TimelineSlider.StepFrequency);
                _timer.Tick += Timer_Tick;
                _timer.Start();
            }
        }
        private void Timer_Tick(object sender, object e)
        {
            // Don't update the Slider's position while the user is interacting with it
            if (!_sliderPressed)
            {
                TimelineSlider.Value = dmrVideo.Position.TotalSeconds;
                
            }
        }
        private void SetTextBlockTime()
        {
            DispatcherTimer _timer_clock = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
            _timer_clock.Tick += Timer_Clock_Tick;
            _timer_clock.Start();
        }
        private void Timer_Clock_Tick(object sender, object e)
        {
            double time_sec = dmrVideo.Position.TotalSeconds;
            int h, m, s;
            s = (int)time_sec;
            m = s / 60;
            h = m / 60;
            m -= h * 60;
            s -= h * 3600 + m * 60;
            TimeBlock.Text = h + ":" + m + ":" + s;
            
        }
        
        #endregion

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (dev_UI == false)
            {
                SetFullWindowMode(true);
                if (first_Start)
                {
                    SetFullWindowMode(false);
                    first_Start = !first_Start;
                }
            }
        }

        

        
    }
}
