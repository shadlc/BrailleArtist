using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace BrailleArtist.View
{
    public partial class IMessageBox : Window
    {
        public static bool Show(string msg = null, string title = null, string yestext = null, string notext = null)
        {
            IMessageBox msgBox = new IMessageBox() { Title = title, Message = msg, YesLabel = yestext, NoLabel = notext };
            bool? state = msgBox.ShowDialog();
            if (state == true) return true;
            else return false;
        }

        private IMessageBox()
        {
            InitializeComponent();
        }

        private string yesText = "Yes";
        private string noText = "No";

        public new string Title
        {
            get { return this.TitleLabel.Text; }
            set { this.TitleLabel.Text = value; }
        }
        public string Message
        {
            get { return this.MsgLabel.Text; }
            set { this.MsgLabel.Text = value; }
        }
        public string YesLabel
        {
            get { return this.yesText; }
            set { this.yesText = value; }
        }
        public string NoLabel
        {
            get { return this.noText; }
            set { this.noText = value; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            if (yesText == null) YesButton.Visibility = Visibility.Collapsed;
            else YesButton.Content = "  " + yesText + "  ";
            if (noText == null) NoButton.Visibility = Visibility.Collapsed;
            else NoButton.Content = "  " + noText + "  ";
        }

        private void WindowMove(object sender, MouseEventArgs e)
        {
            this.DragMove();
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var story = (Storyboard)this.Resources["WindowDisappearAnimation"];
            if (story != null)
            {
                story.Completed += delegate { Close(); };
                story.Begin(this);
            }
        }
        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;
            var story = (Storyboard)this.Resources["WindowDisappearAnimation"];
            if (story != null)
            {
                story.Completed += delegate { this.DialogResult = true; };
                story.Begin(this);
            }
        }
        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;
            var story = (Storyboard)this.Resources["WindowDisappearAnimation"];
            if (story != null)
            {
                story.Completed += delegate { this.DialogResult = false; };
                story.Begin(this);
            }
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Alt && e.SystemKey == Key.Space)
            {
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Alt && e.SystemKey == Key.F4)
            {
                e.Handled = true;
                CloseButton_Click(null, null);
            }
            else if (e.Key == Key.Escape) CloseButton_Click(null, null);
        }

    }
}
