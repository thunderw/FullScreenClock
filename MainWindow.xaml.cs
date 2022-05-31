using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Threading;

namespace FullScreenClock
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string[] DOW = new string[] {"日","一", "二", "三", "四", "五"};
        private string crtFileName = "";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    WindowState = WindowState.Normal;
                    WindowStyle = WindowStyle.SingleBorderWindow;
                    ResizeMode = ResizeMode.CanResize;
                    break;
                case Key.Enter:
                    ResizeMode = ResizeMode.NoResize;
                    WindowStyle = WindowStyle.None;
                    WindowState = WindowState.Maximized;
                    break;
                case Key.Right:
                case Key.PageDown:
                    bool isFound = false;
                    foreach (string fn in Directory.GetFiles("images", "*.jp*g"))
                    {
                        if(isFound)
                        {
                            imgBack.Source = new BitmapImage(new Uri(new FileInfo(fn).FullName));
                            crtFileName = fn;
                            break;
                        }
                        isFound = string.Equals(fn, crtFileName);
                    }
                    break;
                case Key.Left:
                case Key.PageUp:
                    string prevFile = "";
                    foreach (string fn in Directory.GetFiles("images", "*.jp*g"))
                    {
                        if (string.Equals(fn, crtFileName) && !string.IsNullOrEmpty(prevFile))
                        {
                            imgBack.Source = new BitmapImage(new Uri(new FileInfo(prevFile).FullName));
                            crtFileName = prevFile;
                        }
                        prevFile = fn;
                    }
                    break;
                case Key.Space:
                    var status = sbMain.GetCurrentState();
                    if (status == System.Windows.Media.Animation.ClockState.Active)
                    {
                        sbMain.Stop();
                    } else
                    {
                        sbMain.Begin();
                    }
                    break;
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState==WindowState.Maximized)
            {
                ResizeMode = ResizeMode.NoResize;
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(.1);
            timer.Tick += Timer_Tick;
            timer.Start();

            foreach(string fn in Directory.GetFiles("images", "*.jp*g"))
            {
                imgBack.Source = new BitmapImage(new Uri(new FileInfo(fn).FullName));
                crtFileName = fn;
                break;
            }
            sbMain.Begin();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            tbDate.Text = now.ToString("yyyy年M月d日");
            tbTime.Text = now.ToString("HH:mm:ss");
            tbDayOfWeek.Text = DOW[(int)now.DayOfWeek];
        }
    }
}
