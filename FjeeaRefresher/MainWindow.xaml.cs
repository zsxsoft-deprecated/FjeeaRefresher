using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public MainWindow()
        {
            InitializeComponent();
            MainGrid.DataContext = Config.Data;
            NotifyIcon.Click += (object sender, EventArgs args) =>
            {
                WindowState = WindowState.Normal;
                Show();
            };
            
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
        }
    }
}
