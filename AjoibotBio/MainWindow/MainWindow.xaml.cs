using AjoibotBio.Js;
using AjoibotBio.Properties;
using AjoibotBio.Utils;
using log4net;
using System;
using System.Web;
using System.Windows;
using System.Windows.Interop;

namespace AjoibotBio.MainWindow
{
    public partial class MainWindow : Window
    {
        private event EventHandler MainWindowInitialized;

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public delegate void AppRestartedHandler();

        public static event AppRestartedHandler AppRestarted;

        public static IntPtr WindowHandle { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            CheckPrerequisits();

            Loaded += (s, e) =>
            {
                MainWindow.WindowHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
                HwndSource.FromHwnd(MainWindow.WindowHandle)?.AddHook(new HwndSourceHook(HandleMessages));
            };

            //MainWebView.Source = new Uri("https://zhiwj.coded.tj/main.php");
            //MainWebView.Source = new Uri("https://office.tajmedun.tj/api/password.php");

            MainWindowInitialized += InitFaceIdCamera;

            MainWindowInitialized += InitFingerprintScanner;

            MainWindowInitialized.Invoke(this, EventArgs.Empty);

            AppRestarted += ParseUri;
        }

        private void CheckPrerequisits()
        {
            if (WebView2Install.GetInfo().Type != InstallType.WebView2)
            {
                Log.Error("WebView2 environment is not installed on current machine");
            }
        }

        private void ParseUri()
        {
            Log.Debug("Parse uri: " + MainViewModel.Uri);

            if (MainViewModel.Uri.Contains("a_light"))
            {
                var url = new Uri(MainViewModel.Uri);
                var command = HttpUtility
                    .ParseQueryString(url.Query)
                    .Get("lamp");
                //SendCommandToLamp(command);
                //Commands.ShutdownApp();
                Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown());
            }
            else
                NavigateToUri();
        }

        private void NavigateToUri()
        {
            if (string.IsNullOrEmpty(MainViewModel.Uri))
            {
                this.printInMaivWebView("<h2 style='color:red'>Url was not provided<h2>");

            }
            else
            {
                MainWebView.Source = new Uri(MainViewModel.Uri);
            }
        }

        private void printInMaivWebView(string message)
        {
            var url = new Uri("data:text/html," + message);
            MainWebView.Source = url;
        }

        #region Accept System Message
        internal static void HandleParameter(string[] args)
        {
            if (Application.Current?.MainWindow is MainWindow mainWindow)
            {
                {
                    string line = string.Join("", args);
                    if (!string.IsNullOrEmpty(line))
                    {
                        MainViewModel.Uri = "http://" + line.Split(':')[1];
                        AppRestarted();
                    }
                    else
                    {
                        var settings = new Settings();
                        settings.Reload();
                        MainViewModel.Uri = settings.LastUri;
                    }
                }
            }
        }

        private static IntPtr HandleMessages(IntPtr handle, int message, IntPtr wParameter,
            IntPtr lParameter, ref Boolean handled)
        {
            if (handle != MainWindow.WindowHandle)
                return IntPtr.Zero;

            var data = UnsafeNative.GetMessage(message, lParameter);

            if (data != null)
            {
                if (Application.Current.MainWindow == null)
                    return IntPtr.Zero;

                var args = data.Split(' ');
                HandleParameter(args);
                handled = true;
            }

            return IntPtr.Zero;
        }

        public void BringToForeground()
        {
            if (this.Visibility == Visibility.Hidden)
                this.Show();
            this.Activate();
            this.Topmost = true;
            this.Topmost = false;
            this.Focus();
        }

        #endregion

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            MainViewModel.WindowsHeight = e.NewSize.Height;
            MainViewModel.WindowsWidth = e.NewSize.Width;
        }

        private void MainWebView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            MainWebView.CoreWebView2.AddHostObjectToScript("commands", new Commands());
            MainWebView.CoreWebView2.AddHostObjectToScript("faceId", new FaceId());
        }
    }
}
