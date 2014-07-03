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
using System.Threading;
using System.Diagnostics;

using MSGorilla.WebAPI.Client;
using MSGorilla.Library.Models.SqlModels;

namespace MSGorilla.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string GetCurrentUserID()
        {
            string[] array = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\');
            if (array.Length > 1)
            {
                return array[1];
            }
            return array[0];
        }

        GorillaStatusHelper _helper;

        public MainWindow()
        {
            InitializeComponent();
            this.Left = SystemParameters.PrimaryScreenWidth - this.Width;

            _helper = new GorillaStatusHelper(GetCurrentUserID());
            TBStatus.Text = "Welcome to use MSGorilla, " + _helper.Userid;

            ThreadStart entry = new ThreadStart(Update);
            Thread workThread = new Thread(entry);
            workThread.IsBackground = true;
            workThread.Start();
        }

        private void Drag_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    this.DragMove();
                }
            }
            catch { }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://msgorilla.cloudapp.net");
        }

        private void BtnOwn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://msgorilla.cloudapp.net/Notification/index?category=ownerline");
        }

        private void BtnAt_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://msgorilla.cloudapp.net/Notification/index?category=atline");
        }

        private void BtnReply_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://msgorilla.cloudapp.net/Notification/Replies");
        }

        private void Update()
        {
            UpdateCount();
            while (true)
            {
                Thread.Sleep(30000);
                UpdateCount();
            }
        }
        private void UpdateCount()
        {
            try
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    TBStatus.Text = "Updating ...";
                }));
                NotificationCount notif = _helper.UpdateStatus();

                this.Dispatcher.Invoke((Action)(() =>
                {
                    BtnHome.Content = string.Format("Home({0})", notif.UnreadHomelineMsgCount);
                    BtnAt.Content = string.Format("Mention({0})", notif.UnreadAtlineMsgCount);
                    BtnOwn.Content = string.Format("My Own({0})", notif.UnreadOwnerlineMsgCount);
                    BtnReply.Content = string.Format("Reply Me({0})", notif.UnreadReplyCount);

                    TBStatus.Text = "Welcome to use MSGorilla, " + notif.Userid;
                }));
            }
            catch (Exception e)
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    TBStatus.Text = e.ToString();
                }));
            }
        }
    }

    class GorillaStatusHelper
    {
        private GorillaWebAPI _client;
        public string Userid { get; set; }

        public GorillaStatusHelper(string userid)
        {
            _client = new GorillaWebAPI("ShareAccount", "!QAZxsw2#EDCvfr4");
            this.Userid = userid;
        }

        public NotificationCount UpdateStatus()
        {
            return _client.GetNotificationCount(Userid);
        }
    }
}
