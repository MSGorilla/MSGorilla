using System;
using System.Collections.Generic;
using System.Linq;
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

using System.Collections.ObjectModel;
using MSGorilla.Library.Models.ViewModels;
using MSGorilla.WebAPI.Client;
using System.Globalization;
using System.Diagnostics;

namespace MSGorilla.OutlookAddin.GUI
{
    /// <summary>
    /// Interaction logic for MessageView.xaml
    /// </summary>
    public partial class MessageView : UserControl
    {
        public MessageViewType Type;

        public Dictionary<string, object> Argument;

        string token = null;
        public MessageView()
        {
            InitializeComponent();

            //let this windows be the toppest for now
            //this.Topmost = true;
            //this.Topmost = false;
        }

        public void AppendMessage(DisplayMessage msg)
        {
            MessageItem item = new MessageItem();
            item.SetContent(msg);
            item.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            messageList.Items.Add(item);
        }

        public void ClearMessage()
        {
            messageList.Items.Clear();
        }

        DisplayMessagePagination LoadMessageFromGorilla()
        {
            GorillaWebAPI client = Utils.GetGorillaClient();
            if (Type == MessageViewType.Home)
            {
                return client.HomeLine(this.Argument["UserID"] as string, 
                    this.Argument["GroupID"] as string, 
                    10, 
                    this.token);
            }
            else if (Type == MessageViewType.Owner)
            {
                return client.OwnerLine(this.Argument["UserID"] as string, 10, this.token);
            }
            else if (Type == MessageViewType.Mention)
            {
                return client.AtLine(this.Argument["UserID"] as string, 10, this.token);
            }
            else if (Type == MessageViewType.Topic)
            {
                return client.TopicLine(this.Argument["TopicName"] as string, 
                    10, 
                    this.Argument["GroupID"] as string, 
                    this.token);
            }
            else if (Type == MessageViewType.User)
            {
                return client.UserLine(this.Argument["UserID"] as string,
                    "",
                    10,
                    this.token);
            }

            return new DisplayMessagePagination();
        }

        public async void LoadMessage(bool initialLoad = false)
        {
            if (initialLoad || !string.IsNullOrEmpty(token))
            {
                this.LoadMoreButton.Visibility = System.Windows.Visibility.Hidden;
                this.loadingBar.Visibility = System.Windows.Visibility.Visible;
                this.loadingBar.Value = 0;

                Task<DisplayMessagePagination> task = new Task<DisplayMessagePagination>(() =>
                {
                    return LoadMessageFromGorilla();
                }
                );
                task.Start();

                DisplayMessagePagination msgs = new DisplayMessagePagination();
                try
                {
                    msgs = await task;
                }
                catch (Exception exception)
                {
                    //this.Close();
                    MessageBox.Show(exception.Message + "\r\n\r\n" + exception.StackTrace, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }


                if (msgs.message != null)
                {
                    token = msgs.continuationToken;
                    if (msgs.message != null)
                    {
                        foreach (var msg in msgs.message)
                        {
                            AppendMessage(msg);
                        }
                    }
                }
            }

            this.loadingBar.Visibility = System.Windows.Visibility.Hidden;
            if (!string.IsNullOrEmpty(token))
            {
                this.LoadMoreButton.Visibility = System.Windows.Visibility.Visible;
            }
        }

        public void Load()
        {
            ClearMessage();
            LoadMessage(true);
        }

        public void LoadMore(object sender = null, RoutedEventArgs e = null)
        {
            LoadMessage(false);
        }
    }
}