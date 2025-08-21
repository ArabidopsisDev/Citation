using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace Citation.View
{
    public partial class VerifyWindow : Window
    {
        private readonly Func<string, bool, bool> _authorizeCallback;

        public VerifyWindow(Func<string, bool, bool> callback)
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

            bool result = _authorizeCallback(PasswordBox.Password, true);

            if (result)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                StatusText.Text = "授权失败，请检查密码是否正确";
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

            var result = _authorizeCallback(FilePathTextBox.Text, false);

            if (result)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                StatusText.Text = "授权文件无效或已损坏";
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}