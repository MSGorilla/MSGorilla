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
using MSGorilla.WebAPI.Models.ViewModels;

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

        private async void LikeTopic(object sender, RoutedEventArgs e)
        {
            this.loadingBar.Visibility = System.Windows.Visibility.Visible;

            Task<bool> likeTopicTask = new Task<bool>(() =>
            {
                string topicName = this.messageView.Argument["TopicName"] as string;
                string groupID = this.messageView.Argument["GroupID"] as string;
                GorillaWebAPI client = Utils.GetGorillaClient();
                client.AddFavouriteTopic(topicName, groupID);

                bool isLiked = client.IsFavouriteTopic(topicName, groupID);
                return isLiked;
            });
            likeTopicTask.Start();
            bool isliked = await likeTopicTask;

            if (isliked)
            {
                this.ActionBtn.Content = "Remove from Favourite";
                this.ActionBtn.Click -= LikeTopic;
                this.ActionBtn.Click += UnlikeTopic;
            }

            this.loadingBar.Visibility = System.Windows.Visibility.Hidden;
        }

        private async void UnlikeTopic(object sender, RoutedEventArgs e)
        {
            this.loadingBar.Visibility = System.Windows.Visibility.Visible;

            Task<bool> likeTopicTask = new Task<bool>(() =>
            {
                string topicName = this.messageView.Argument["TopicName"] as string;
                string groupID = this.messageView.Argument["GroupID"] as string;
                GorillaWebAPI client = Utils.GetGorillaClient();
                client.RemoveFavouriteTopic(topicName, groupID);

                bool isLiked = client.IsFavouriteTopic(topicName, groupID);
                return isLiked;
            });
            likeTopicTask.Start();
            bool isliked = await likeTopicTask;

            if (!isliked)
            {
                this.ActionBtn.Content = "Add to Favourite";
                this.ActionBtn.Click += LikeTopic;
                this.ActionBtn.Click -= UnlikeTopic;
            }

            this.loadingBar.Visibility = System.Windows.Visibility.Hidden;
        }

        void LoadTopicActionBtn()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                this.loadingBar.Visibility = System.Windows.Visibility.Visible;

                GorillaWebAPI client = Utils.GetGorillaClient();
                string topicName = this.messageView.Argument["TopicName"] as string;
                string groupID = this.messageView.Argument["GroupID"] as string;

                bool isFavourite = client.IsFavouriteTopic(topicName, groupID);

                if (isFavourite)
                {
                    this.ActionBtn.Content = "Remove from Favourite";
                    this.ActionBtn.Click += UnlikeTopic;
                }
                else
                {
                    this.ActionBtn.Content = "Add to Favourite";
                    this.ActionBtn.Click += LikeTopic;
                }
                this.ActionBtn.Visibility = System.Windows.Visibility.Visible;

                this.loadingBar.Visibility = System.Windows.Visibility.Hidden;
            }));
        }

        void LoadJoinedGroup()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                this.loadingBar.Visibility = System.Windows.Visibility.Visible;
                GorillaWebAPI client = Utils.GetGorillaClient();
                List<DisplayMembership> groups = client.GetJoinedGroup();

                GroupComboBox.ItemsSource = groups;
                GroupComboBox.SelectedItem = groups[0];
                ComboxBoxPanel.Visibility = System.Windows.Visibility.Visible;
                this.loadingBar.Visibility = System.Windows.Visibility.Hidden;
            }));
        }

        public void Load()
        {
            this.ActionBtn.Visibility = System.Windows.Visibility.Hidden;
            this.loadingBar.Visibility = System.Windows.Visibility.Hidden;

            if (this.messageView.Type == MessageViewType.Home)
            {
                this.TitleTB.Text = "Home";
                this.SubTitleTB.Text = this.messageView.Argument["GroupID"] as string;
                this.headDockPanel.Children.Remove(ComboxBoxPanel);
                this.messageView.Load();
            }
            else if (this.messageView.Type == MessageViewType.Mention)
            {
                this.TitleTB.Text = "Mention Me";
                this.SubTitleTB.Visibility = System.Windows.Visibility.Hidden;
                this.headDockPanel.Children.Remove(ComboxBoxPanel);
                this.messageView.Load();
            }
            else if (this.messageView.Type == MessageViewType.Owner)
            {
                this.TitleTB.Text = "Owned by me";
                this.SubTitleTB.Visibility = System.Windows.Visibility.Hidden;
                this.headDockPanel.Children.Remove(ComboxBoxPanel);
                this.messageView.Load();
            }
            else if (this.messageView.Type == MessageViewType.Topic)
            {
                this.TitleTB.Text = "Topic";
                this.SubTitleTB.Text =
                    string.Format("{0}({1})", this.messageView.Argument["TopicName"], this.messageView.Argument["GroupID"]);

                Task loadBtnTask = new Task(LoadTopicActionBtn);
                loadBtnTask.Start();

                this.headDockPanel.Children.Remove(ComboxBoxPanel);
                this.messageView.Load();
            }
            else if (this.messageView.Type == MessageViewType.User)
            {
                this.TitleTB.Text = this.messageView.Argument["DisplayName"] as string;
                this.SubTitleTB.Text = this.messageView.Argument["UserID"] as string;

                new Task(LoadJoinedGroup).Start();
            }
        }

        private void GroupComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox SelectBox = (ComboBox)sender;
            string groupID = ((DisplayMembership)SelectBox.SelectedItem).GroupID;
            this.messageView.Argument["GroupID"] = groupID;
            this.messageView.token = null;
            this.messageView.Load();
        }
    }
}
