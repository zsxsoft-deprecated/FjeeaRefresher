using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FjeeaRefresher
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private string LastCachedText = "";

        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }

        public System.Windows.Threading.DispatcherTimer Timer = new System.Windows.Threading.DispatcherTimer
        {
            IsEnabled = true, 
            Interval = new TimeSpan(0, 0, 0, 60)
        };
        public System.Windows.Forms.NotifyIcon NotifyIcon = new System.Windows.Forms.NotifyIcon
        {
            Icon = FjeeaResource.Properties.Resources.Icon, 
            Visible = true,
            Text = "FjeeaRefresher"
        };


        public async void GetScoreInThread()
        {
            Console.WriteLine("Getting score");
            var TryLoginText = await NetworkOperator.PostToLQCX();
            if (ValidationCodeParser.CalcSimilarDegree(TryLoginText, LastCachedText) > 20 || LastCachedText == "")
            {
                LastCachedText = TryLoginText;
                if (TryLoginText == "") return;
                Dispatcher.Invoke(() =>
                {
                    WebBrowser.NavigateToString(NetworkOperator.FormatUrlInHtml(TryLoginText));
                });
                NotifyIcon.ShowBalloonTip(30000, "FjeeaRefresher", TryLoginText, System.Windows.Forms.ToolTipIcon.Info);
            } 
            

        }

        public void GetScore() =>Task.Factory.StartNew(GetScoreInThread);

        public MainWindow()
        {
            InitializeComponent();
            MainGrid.DataContext = Config.Data;
            Timer.Interval = new TimeSpan(0, 0, 0, Config.Data.Interval);
            Timer.Tick += (object sender, EventArgs args) =>
            {
                GetScore();
            };
            NotifyIcon.Click += (object sender, EventArgs args) =>
            {
                WindowState = WindowState.Normal;
                Show();
            };/*
            WebBrowser.Navigating += new NavigatingCancelEventHandler((object sender, NavigatingCancelEventArgs e) =>
            {
                HtmlDocument = WebBrowser.Document
            });*/
            WebBrowser.Navigated += new NavigatedEventHandler((object sender, NavigationEventArgs e) =>
            {
                ///<see cref="http://stackoverflow.com/questions/6138199/wpf-webbrowser-control-how-to-supress-script-errors"/>
                // get an IWebBrowser2 from the document
                IOleServiceProvider sp = WebBrowser.Document as IOleServiceProvider;
                if (sp != null)
                {
                    Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                    Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                    object webBrowser;
                    sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                    webBrowser?.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { true });
                }
            }
            );

        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                Hide();

            base.OnStateChanged(e);
        }

        private void ButtonSubmit_Click(object sender, RoutedEventArgs e)
        {
            Timer.Interval = new TimeSpan(0, 0, 0, Config.Data.Interval);
            Config.Save();
            GetScore();
        }

        private void ButtonTest_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(async () =>
            {
                var TryLoginText = await NetworkOperator.TryLogin();
                if (ValidationCodeParser.CalcSimilarDegree(TryLoginText, LastCachedText) > 20 || LastCachedText == "")
                {
                    LastCachedText = TryLoginText;
                    Dispatcher.Invoke(() =>
                    {
                        WebBrowser.NavigateToString(NetworkOperator.FormatUrlInHtml(TryLoginText));
                    });
                }
            });
        }
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            NotifyIcon.Visible = false;
        }
    }
}
