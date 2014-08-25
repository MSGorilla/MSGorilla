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
    public partial class MessageView : Window
    {
        public enum TypeEnum{
            Home,
            Owner,
            Mention,
            Topic,
            Category
        };

        public TypeEnum Type;
        public object Argument;
        public string GroupID;

        string token = null;
        public MessageView()
        {
            InitializeComponent();

            //let this windows be the toppest for now
            this.Topmost = true;
            this.Topmost = false;
        }

        public void AppendMessage(DisplayMessage msg)
        {
            MessageItem item = new MessageItem();
            item.SetContent(msg);
            messageList.Items.Add(item);
        }

        public void ClearMessage()
        {
            messageList.Items.Clear();
        }

        private void GotoMSGorilla(object sender, RoutedEventArgs e)
        {
            Process.Start("https://msgorilla.cloudapp.net/");
            //wbSample.Navigate("https://msgorilla.cloudapp.net/");
        }

        private void SetTitle(string title)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                this.Title = title;
            }));
        }

        DisplayMessagePagination LoadMessage()
        {
            GorillaWebAPI client = Utils.GetGorillaClient();
            if (Type == TypeEnum.Home)
            {
                SetTitle(string.Format("Home(Group {0})", this.GroupID));
                return client.HomeLine(this.Argument as string, this.GroupID, 10, this.token);
            }
            else if (Type == TypeEnum.Owner)
            {
                SetTitle("My Own");
                return client.OwnerLine(this.Argument as string, 10, this.token);
            }
            else if (Type == TypeEnum.Mention)
            {
                SetTitle("Mention me");
                return client.AtLine(this.Argument as string, 10, this.token);
            }
            else if (Type == TypeEnum.Topic)
            {
                SetTitle("Topic: " + this.Argument as string);
                return client.TopicLine(this.Argument as string, 10, this.GroupID, this.token);
            }

            return new DisplayMessagePagination();
        }


        public async void Load()
        {
            ClearMessage();
            this.LoadMoreButton.Visibility = System.Windows.Visibility.Hidden;
            this.loadingBar.Visibility = System.Windows.Visibility.Visible;
            this.loadingBar.Value = 0;

            Task<DisplayMessagePagination> task = 
                new Task<DisplayMessagePagination>(() =>{
                    return LoadMessage();
                });
            task.Start();

            DisplayMessagePagination msgs = new DisplayMessagePagination();
            try
            {
                msgs = await task;
            }
            catch (Exception e)
            {
                this.Close();
                MessageBox.Show(e.Message + "\r\n\r\n" + e.StackTrace, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (msgs != null)
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
            else
            {
                this.token = null;
            }
            

            this.loadingBar.Visibility = System.Windows.Visibility.Hidden;
            if (!string.IsNullOrEmpty(token))
            {
                this.LoadMoreButton.Visibility = System.Windows.Visibility.Visible;
            }
        }

        public async void LoadMore(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(token))
            {
                return;
            }

            this.LoadMoreButton.Visibility = System.Windows.Visibility.Hidden;
            this.loadingBar.Visibility = System.Windows.Visibility.Visible;
            this.loadingBar.Value = 0;

            Task<DisplayMessagePagination> task = new Task<DisplayMessagePagination>(() =>
            {
                return LoadMessage();
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
                this.Close();
                MessageBox.Show(exception.Message + "\r\n\r\n" + exception.StackTrace, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            token = msgs.continuationToken;
            if (msgs.message != null)
            {
                foreach (var msg in msgs.message)
                {
                    AppendMessage(msg);
                }
            }

            this.loadingBar.Visibility = System.Windows.Visibility.Hidden;
            if (!string.IsNullOrEmpty(token))
            {
                this.LoadMoreButton.Visibility = System.Windows.Visibility.Visible;
            }
        }
    }
}