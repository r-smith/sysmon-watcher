using System.Windows;
using System.Windows.Input;

namespace Sysmon_Watcher.UI
{
    public partial class MessageWindow : Window
    {
        public MessageWindow(string message)
        {
            InitializeComponent();

            Message.Text = message;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
    }
}
