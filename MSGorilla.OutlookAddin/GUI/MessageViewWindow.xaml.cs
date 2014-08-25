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

        public void Load()
        {
            if (this.messageView.Type == MessageViewType.Home)
            {
                this.TitleTB.Text = "Home";
                this.SubTitleTB.Text = this.messageView.Argument["GroupID"] as string;
                this.Btn.Visibility = System.Windows.Visibility.Hidden;
            }
            else if (this.messageView.Type == MessageViewType.Mention)
            {
                this.TitleTB.Text = "Mention Me";
                this.SubTitleTB.Visibility = System.Windows.Visibility.Hidden;
                this.Btn.Visibility = System.Windows.Visibility.Hidden;
            }
            else if (this.messageView.Type == MessageViewType.Owner)
            {
                this.TitleTB.Text = "Owned by me";
                this.SubTitleTB.Visibility = System.Windows.Visibility.Hidden;
                this.Btn.Visibility = System.Windows.Visibility.Hidden;
            }
            else if (this.messageView.Type == MessageViewType.Topic)
            {
                this.TitleTB.Text = "Topic";
                this.SubTitleTB.Text =
                    string.Format("{0}({1})", this.messageView.Argument["TopicName"], this.messageView.Argument["GroupID"]);
                this.Btn.Visibility = System.Windows.Visibility.Hidden;
            }
            else if (this.messageView.Type == MessageViewType.User)
            {
                this.TitleTB.Text = this.messageView.Argument["DisplayName"] as string;
                this.SubTitleTB.Text = this.messageView.Argument["UserID"] as string;
                this.Btn.Visibility = System.Windows.Visibility.Hidden;
            }
            this.messageView.Load();
        }
    }
}
