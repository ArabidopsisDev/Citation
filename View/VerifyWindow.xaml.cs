using Citation.Model;
using Microsoft.Win32;
using System.Windows;

namespace Citation.View
{
    public partial class VerifyWindow : Window
    {
        private readonly Func<string, bool, FailedMessage> _authorizeCallback;

        public VerifyWindow(Func<string, bool, FailedMessage> callback)
        {
            InitializeComponent();
            _authorizeCallback = callback;
        }

        private void PasswordAuthButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PasswordBox.Password))
            {
                StatusText.Text = "请输入授权密码";
                return;
            }

            var reason = _authorizeCallback(PasswordBox.Password, true);
            var result = reason.Reason == FailedMessage.FailedReason.Success;

            if (result)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                StatusText.Text = reason.Message;
                PasswordBox.Clear();
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "授权文件 (*.clc)|*.clc"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                FilePathTextBox.Text = openFileDialog.FileName;
            }
        }

        private void FileAuthButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(FilePathTextBox.Text))
            {
                StatusText.Text = "请选择授权文件";
                return;
            }

            var reason = _authorizeCallback(FilePathTextBox.Text, false);
            var result = reason.Reason == FailedMessage.FailedReason.Success;

            if (result)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                StatusText.Text = reason.Message;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}