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

using MSGorilla.WebAPI.Client;

namespace MSGorilla.OutlookAddin.GUI
{
    /// <summary>
    /// Interaction logic for MessageViewWindow.xaml
    /// </summary>
    public partial class MessageViewWindow : Window
    {
        public static MessageViewWindow CreateMessageViewWindow(
            MessageViewType type,
            Dictionary<string, object> argument)
        {
            MessageViewWindow viewWindow = new MessageViewWindow();
            viewWindow.messageView.Type = type;
            viewWindow.messageView.Argument = argument;
            return viewWindow;
        }

        public MessageViewWindow()
        {
            InitializeComponent();
            this.Title = "MSGorilla Outlook Addin";
        }

        private void LikeTopic(object sender, RoutedEventArgs e)
        {
            
        }

        void LoadTopicActionBtn()
        {
            GorillaWebAPI client = Utils.GetGorillaClient();
            string topicName = this.messageView.Argument["TopicName"] as string;
            string groupID = this.messageView.Argument["GroupID"] as string;

            bool isFavourite = client.IsFavouriteTopic(topicName, groupID);

            this.Dispatcher.Invoke((Action)(() =>
            {
                if (isFavourite)
                {
                    this.ActionBtn.Content = "Liked";
                    this.ActionBtn.Background = new SolidColorBrush(Color.FromRgb(63,182,24));
                    this.ActionBtn.Foreground = new SolidColorBrush(Colors.White);
                }
                else
                {
                    this.ActionBtn.Content = "+Like";
                    this.ActionBtn.Background = SystemColors.MenuHighlightBrush;
                    this.ActionBtn.Foreground = new SolidColorBrush(Colors.White);
                    this.ActionBtn.Click += LikeTopic;
                }
                this.ActionBtn.Visibility = System.Windows.Visibility.Visible;
            }));
        }

        public void Load()
        {
            this.ActionBtn.Visibility = System.Windows.Visibility.Hidden;

            if (this.messageView.Type == MessageViewType.Home)
            {
                this.TitleTB.Text = "Home";
                this.SubTitleTB.Text = this.messageView.Argument["GroupID"] as string;
            }
            else if (this.messageView.Type == MessageViewType.Mention)
            {
                this.TitleTB.Text = "Mention Me";
                this.SubTitleTB.Visibility = System.Windows.Visibility.Hidden;
            }
            else if (this.messageView.Type == MessageViewType.Owner)
            {
                this.TitleTB.Text = "Owned by me";
                this.SubTitleTB.Visibility = System.Windows.Visibility.Hidden;
            }
            else if (this.messageView.Type == MessageViewType.Topic)
            {
                this.TitleTB.Text = "Topic";
                this.SubTitleTB.Text =
                    string.Format("{0}({1})", this.messageView.Argument["TopicName"], this.messageView.Argument["GroupID"]);

                Task loadBtnTask = new Task(LoadTopicActionBtn);
                loadBtnTask.Start();
                //LoadTopicActionBtn();
            }
            else if (this.messageView.Type == MessageViewType.User)
            {
                this.TitleTB.Text = this.messageView.Argument["DisplayName"] as string;
                this.SubTitleTB.Text = this.messageView.Argument["UserID"] as string;
            }
            this.messageView.Load();
        }
    }
}
