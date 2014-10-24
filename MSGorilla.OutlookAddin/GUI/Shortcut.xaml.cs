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
using System.Diagnostics;
using System.Threading;

using MSGorilla.WebAPI.Models;
using MSGorilla.WebAPI.Models.ViewModels;
using MSGorilla.WebAPI.Client;

namespace MSGorilla.OutlookAddin.GUI
{
    /// <summary>
    /// Customized treeviewitem to store message view info
    /// </summary>
    public class MyTreeViewItem : TreeViewItem
    {
        public MessageViewType MessageType { get; set; }
        public Dictionary<string, object> Argument { get; set; }
        public string Group { get; set; }
        public bool ShowMessageWindow { get; set; }
        public MyTreeViewItem() { }
        public MyTreeViewItem(string header,
            bool showMessageWindow = false,
            MessageViewType type = MessageViewType.Home,
            Dictionary<string, object> argument = null) : base()
        {
            this.Header = header;
            this.MessageType = type;
            this.Argument = argument;
            this.ShowMessageWindow = showMessageWindow;
        }
    }

    /// <summary>
    /// Interaction logic for Shortcut.xaml
    /// </summary>
    public partial class Shortcut : UserControl
    {
        GorillaWebAPI _client = Utils.GetGorillaClient();

        MyTreeViewItem _homeItem = new MyTreeViewItem("Home");
        MyTreeViewItem _ownItem = new MyTreeViewItem("Own", 
            true, 
            MessageViewType.Owner, 
            new Dictionary<string, object>() { {"UserID", Utils.GetCurrentUserID()}});
        MyTreeViewItem _mentionItem = new MyTreeViewItem("Mention", 
            true,
            MessageViewType.Mention,
            new Dictionary<string, object>() { { "UserID", Utils.GetCurrentUserID() } });
        MyTreeViewItem _topicItem = new MyTreeViewItem("Topic");
        MyTreeViewItem _followingItem = new MyTreeViewItem("Followings");

        public Shortcut()
        {
            InitializeComponent();

            tree.Items.Add(_ownItem);
            tree.Items.Add(_mentionItem);
            tree.Items.Add(_topicItem);
            tree.Items.Add(_followingItem);
            tree.Items.Add(_homeItem);

            Paragraph para = new Paragraph();
            Hyperlink link = Utils.CreateLink("Goto MSGorilla to see more.", "https://msgorilla.cloudapp.net/");
            para.Inlines.Add(link);
            Link2MSGorilla.Document.Blocks.Remove(Link2MSGorilla.Document.Blocks.FirstBlock);
            Link2MSGorilla.Document.Blocks.Add(para);

            Thread magnetThread = new Thread(new ThreadStart(Update));
            magnetThread.IsBackground = true;
            magnetThread.Start();
        }

        void Update()
        {
            
            while (true)
            {
                try
                {
                    if (!Globals.Ribbons.Ribbon.keepUpdateBtn.Checked)
                    {
                        continue;
                    }
                    //Fill home
                    List<DisplayMembership> members = _client.GetJoinedGroup();
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        _homeItem.Items.Clear();
                        foreach (var member in members)
                        {
                            MyTreeViewItem item = new MyTreeViewItem(member.DisplayName,
                                true,
                                MessageViewType.Home,
                                new Dictionary<string, object>() {
                                    {"GroupID", member.GroupID},
                                    {"UserID", Utils.GetCurrentUserID()}
                                });
                            _homeItem.Items.Add(item);
                        }
                    }));
                    //Fill topic
                    List<DisplayFavouriteTopic> topics = _client.GetMyFavouriteTopic();
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        _topicItem.Items.Clear();
                        foreach (var topic in topics)
                        {
                            MyTreeViewItem item = new MyTreeViewItem(
                                string.Format("{0}({1})", topic.topicName, topic.UnreadMsgCount),
                                true,
                                MessageViewType.Topic,
                                new Dictionary<string, object>(){
                                    {"GroupID", topic.GroupID},
                                    {"TopicName", topic.topicName}
                                });
                            _topicItem.Items.Add(item);
                        }
                    }));

                    //Fill Following
                    List<DisplayUserProfile> followings = _client.GetFollowings();
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        _followingItem.Items.Clear();
                        foreach (var user in followings)
                        {
                            MyTreeViewItem item = new MyTreeViewItem(
                                string.Format("{0}({1})", user.DisplayName, user.Userid),
                                true,
                                MessageViewType.User,
                                new Dictionary<string, object>()
                                {
                                    {"DisplayName", user.DisplayName},
                                    {"UserID", user.Userid}
                                });
                            _followingItem.Items.Add(item);
                        }
                    }));

                    //Update Notification count
                    NotificationCount notif = _client.GetNotificationCount();
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        _homeItem.Header = string.Format("Home({0})", notif.UnreadHomelineMsgCount);
                        _ownItem.Header = string.Format("Own({0})", notif.UnreadOwnerlineMsgCount);
                        _mentionItem.Header = string.Format("Mention({0})", notif.UnreadAtlineMsgCount);
                    }));
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message + "\r\n" + e.StackTrace, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Thread.Sleep(1000 * 120);
                }
            }
        }

        private void SearchMSGorilla(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.SearchTB.Text))
            {
                Process.Start("https://msgorilla.cloudapp.net/Search/index?keyword=" + this.SearchTB.Text);
            }
        }

        private void SelectAll(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void load(object sender, RoutedEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }
            e.Handled = true;

            MyTreeViewItem item = e.OriginalSource as MyTreeViewItem;
            if (item.ShowMessageWindow)
            {
                MessageViewWindow window = 
                    MessageViewWindow.CreateMessageViewWindow(item.MessageType, item.Argument);
                new Thread(new ThreadStart(() =>
                {
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        window.Show();
                    }));
                })).Start();
                window.Load();
            }
            item.IsSelected = false;
        }
    }
}
