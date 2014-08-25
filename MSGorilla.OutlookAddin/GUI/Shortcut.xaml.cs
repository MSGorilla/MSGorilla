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

using MSGorilla.Library.Models;
using MSGorilla.Library.Models.ViewModels;
using MSGorilla.WebAPI.Client;

namespace MSGorilla.OutlookAddin.GUI
{
    /// <summary>
    /// Customized treeviewitem to store message view info
    /// </summary>
    public class MyTreeViewItem : TreeViewItem
    {
        public MessageView.TypeEnum MessageType { get; set; }
        public object Argument { get; set; }
        public string Group { get; set; }
        public bool ShowMessageWindow { get; set; }
        public MyTreeViewItem() { }
        public MyTreeViewItem(string header,
            bool showMessageWindow = false,
            MessageView.TypeEnum type = MessageView.TypeEnum.Home,
            string group = null,
            object argument = null) : base()
        {
            this.Header = header;
            this.MessageType = type;
            this.Argument = argument;
            this.ShowMessageWindow = showMessageWindow;
            this.Group = group;
        }
    }

    /// <summary>
    /// Interaction logic for Shortcut.xaml
    /// </summary>
    public partial class Shortcut : UserControl
    {
        GorillaWebAPI _client = Utils.GetGorillaClient();

        MyTreeViewItem _homeItem = new MyTreeViewItem("Home");
        MyTreeViewItem _ownItem = new MyTreeViewItem("Own", true, MessageView.TypeEnum.Owner);
        MyTreeViewItem _mentionItem = new MyTreeViewItem("Mention", true, MessageView.TypeEnum.Mention);
        MyTreeViewItem _topicItem = new MyTreeViewItem("Topic");

        public Shortcut()
        {
            InitializeComponent();

            tree.Items.Add(_ownItem);
            tree.Items.Add(_mentionItem);
            tree.Items.Add(_topicItem);
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
                    if (!Globals.ThisAddIn.KeepUpdatingInfo)
                    {
                        continue;
                    }
                    List<DisplayMembership> members = _client.GetJoinedGroup();
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        _homeItem.Items.Clear();
                        foreach (var member in members)
                        {
                            MyTreeViewItem item = new MyTreeViewItem(member.DisplayName,
                                true,
                                MessageView.TypeEnum.Home,
                                member.GroupID);
                            _homeItem.Items.Add(item);
                        }
                    }));

                    List<DisplayFavouriteTopic> topics = _client.GetMyFavouriteTopic();
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        _topicItem.Items.Clear();
                        foreach (var topic in topics)
                        {
                            MyTreeViewItem item = new MyTreeViewItem(
                                string.Format("{0}({1})", topic.topicName, topic.UnreadMsgCount),
                                true,
                                MessageView.TypeEnum.Topic,
                                topic.GroupID,
                                topic.topicName);
                            _topicItem.Items.Add(item);
                        }
                    }));

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
                    Thread.Sleep(1000 * 10);
                }
            }
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
                MessageView view = new MessageView();
                view.Type = item.MessageType;
                view.Argument = item.Argument;
                view.GroupID = item.Group;
                //Have to create a new thread to show message view
                //or the parent TreeViewItem will also be
                //selected. Still have no idea why this happen?
                new Thread(new ThreadStart(() =>{
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        view.Show();
                    }));
                })).Start();
                view.Load();
            }            
        }
    }
}
