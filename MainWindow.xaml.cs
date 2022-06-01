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
                    string dn = string.IsNullOrEmpty(crtFileName) ? "images" : 
                        System.IO.Path.GetDirectoryName(crtFileName);
                    foreach (string fn in Directory.GetFiles(dn, "*.jp*g"))
                    {
                        if(isFound)
                        {
                            imgBack.Source = new BitmapImage(new Uri(new FileInfo(fn).FullName));
                            crtFileName = fn;
                            Properties.Settings.Default.LastFileName = crtFileName;
                            Properties.Settings.Default.Save();
                            break;
                        }
                        isFound = string.Equals(fn, crtFileName);
                    }
                    break;
                case Key.Left:
                case Key.PageUp:
                    dn = string.IsNullOrEmpty(crtFileName) ? "images" :
                        System.IO.Path.GetDirectoryName(crtFileName);
                    string prevFile = "";
                    foreach (string fn in Directory.GetFiles(dn, "*.jp*g"))
                    {
                        if (string.Equals(fn, crtFileName) && !string.IsNullOrEmpty(prevFile))
                        {
                            imgBack.Source = new BitmapImage(new Uri(new FileInfo(prevFile).FullName));
                            crtFileName = prevFile;
                            Properties.Settings.Default.LastFileName = crtFileName;
                            Properties.Settings.Default.Save();
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

            if (!string.IsNullOrEmpty(Properties.Settings.Default.LastFileName))
            {
                var fi = new FileInfo(Properties.Settings.Default.LastFileName);
                if(fi.Exists)
                {
                    imgBack.Source = new BitmapImage(new Uri(fi.FullName));
                    crtFileName = fi.FullName;
                }
            }
            if (string.IsNullOrEmpty(crtFileName))
            {
                LoadFirstFile("images");
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

        private void Window_Drop(object sender, DragEventArgs e)
        {
            var ar = (System.Array)e.Data.GetData(DataFormats.FileDrop);
            if (ar.Length > 0) {
                var fn = ar.GetValue(0).ToString();
                if (Directory.Exists(fn))
                {
                    foreach (string f in Directory.GetFiles(fn, "*.jp*g"))
                    {
                        imgBack.Source = new BitmapImage(new Uri(new FileInfo(f).FullName));
                        crtFileName = f;
                        Properties.Settings.Default.LastFileName = crtFileName;
                        Properties.Settings.Default.Save();
                        break;
                    }
                } else
                {
                    var fi = new FileInfo(fn);
                    var ext = System.IO.Path.GetExtension(fn).ToLower();
                    if (fi.Exists && (".jpg".Equals(ext) || ".jpeg".Equals(ext)))
                    {
                        imgBack.Source = new BitmapImage(new Uri(new FileInfo(fn).FullName));
                        crtFileName = fn;
                        Properties.Settings.Default.LastFileName = crtFileName;
                        Properties.Settings.Default.Save();
                    }
                }
            }
        }

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Link;
            } else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dn">Directory Name</param>
        private void LoadFirstFile(string dn)
        {
            foreach (string fn in Directory.GetFiles(dn, "*.jp*g"))
            {
                imgBack.Source = new BitmapImage(new Uri(new FileInfo(fn).FullName));
                crtFileName = fn;
                break;
            }
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var d = e.Delta >= 0 ? e.Delta / 200.0 : 1 / e.Delta - 1;
            stClock.ScaleX += d;
            stClock.ScaleY += d;
            if (stClock.ScaleX < 0.1)
            {
                stClock.ScaleX = 0.1;
                stClock.ScaleY = 0.1;
            }
            if (stClock.ScaleX > 8)
            {
                stClock.ScaleX = 8;
                stClock.ScaleY = 8;
            }
        }

        private Point mouseStartPos;
        private Vector startTtOffset;

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            startTtOffset = new Vector(ttClock.X, ttClock.Y);
            mouseStartPos = e.GetPosition(this);
            // Do not move CaptureMouse before get ttClock.X, or you'll get wrong value
            grdClock.CaptureMouse();
        }

        private void grdClock_MouseUp(object sender, MouseButtonEventArgs e)
        {
            grdClock.ReleaseMouseCapture();
        }

        private void grdClock_MouseMove(object sender, MouseEventArgs e)
        {
            if (grdClock.IsMouseCaptured)
            {
                var offset = Point.Subtract(e.GetPosition(this), mouseStartPos);
                ttClock.X = startTtOffset.X + offset.X / stClock.ScaleX;
                ttClock.Y = startTtOffset.Y + offset.Y / stClock.ScaleY;
            }
        }
    }
}
