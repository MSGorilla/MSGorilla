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
using System.Text.RegularExpressions;

using MSGorilla.Library.Models.ViewModels;
using MSGorilla.OutlookAddin;


namespace MSGorilla.OutlookAddin.GUI
{
    /// <summary>
    /// Interaction logic for MessageItem.xaml
    /// 
    /// Todo: Important, richmessage
    /// </summary>
    public partial class MessageItem : UserControl
    {
        private string _messageID;
        public MessageItem()
        {
            InitializeComponent();

            this.ShowMoreBtn.IsEnabled = false;
            this.ShowMoreBtn.Content = "See More";
            this.ShowMoreBtn.Visibility = System.Windows.Visibility.Hidden;
            this.loadingBar.Visibility = System.Windows.Visibility.Hidden;
        }

        private void GotoMSGorilla(object sender, RoutedEventArgs e)
        {
            Process.Start("https://msgorilla.cloudapp.net/Message/Index?msgID=" + this._messageID);
            //wbSample.Navigate("https://msgorilla.cloudapp.net/");
        }

        private async void ShowRichMessage(object sender, RoutedEventArgs e)
        {
            try
            {
                this.ShowMoreBtn.IsEnabled = false;
                this.ShowMoreBtn.Content = "Loading...";
                this.loadingBar.Visibility = System.Windows.Visibility.Visible;

                string richMsgID = ((Button)sender).Tag as string;

                Task<String> getMsgTask = new Task<string>(() => {
                    return Utils.GetGorillaClient().GetRichMessage(richMsgID);
                });
                getMsgTask.Start();
                string richMessage = await getMsgTask;
                richMessage = Utils.ProcessRichMessage(richMessage);

                string tempPath = System.IO.Path.GetTempFileName() + ".html";
                System.IO.File.WriteAllText(tempPath, richMessage);
                Process.Start(tempPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message + "\n\n" + ex.StackTrace, 
                    "Error", 
                    MessageBoxButton.OK);
            }
            finally
            {
                this.ShowMoreBtn.IsEnabled = true;
                this.ShowMoreBtn.Content = "See More";
                this.loadingBar.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        public void SetContent(DisplayMessage msg)
        {
            this._messageID = msg.ID;
            this.UsernameTB.Text = msg.User.Userid;
            this.TimestampTB.Text = string.Format("{0:F}", msg.PostTime.ToLocalTime());
            this.Thumbnail.Source = 
                new ImageSourceConverter().ConvertFromString(
                    ThumbnailRetriever.GetThumbnail(msg.User.PortraitUrl)
                ) as ImageSource;

            if (msg.Importance == 0)
            {
                //set orange background
                this.MessageTB.Background = 
                    new SolidColorBrush(Color.FromRgb(254, 60, 0));
            }

            if (!string.IsNullOrEmpty(msg.RichMessageID))
            {
                this.ShowMoreBtn.Tag = msg.RichMessageID;
                this.ShowMoreBtn.IsEnabled = true;
                this.ShowMoreBtn.Visibility = System.Windows.Visibility.Visible;
            }

            //create line in rich text box
            RichtextHelper helper = new RichtextHelper(
                msg.Group,
                (sender, args) =>
                {
                    string url = args.Uri.ToString();
                    Regex r = new Regex(@"topic=.*?(?=&|$)", RegexOptions.IgnoreCase);
                    if (r.IsMatch(url))
                    {
                        string topicName = r.Match(url).Value;
                        topicName = topicName.Replace("topic=", "");
                        topicName = topicName.Replace("&", "");

                        MessageViewWindow window = MessageViewWindow.CreateMessageViewWindow(
                                MessageViewType.Topic,
                                topicName,
                                msg.Group
                            );
                        window.Show();
                        window.Load();
                    }
                    else
                    {
                        Process.Start(url);
                    }
                }
                );

            this.MessageTB.Document.Blocks.Remove(this.MessageTB.Document.Blocks.FirstBlock);
            this.MessageTB.Document.Blocks.Add(helper.ParseText(msg.MessageContent));
        }
    }
}
