using Citation.Model;
using Citation.Utils;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace Citation.View
{
    public partial class AuthorizationWindow : Window, INotifyPropertyChanged
    {
        public Authorization Authorization
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged(nameof(Authorization));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public AuthorizationWindow()
        {
            InitializeComponent();

            Authorization = new Authorization()
            {
                ExpirationTime = DateTime.Now,
                IpAddresses = [],
                Password = ""
            };

            DataContext = this;
        }

        private void AddIp_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NewIpTextBox.Text) &&
                NewIpTextBox.Text != "输入IP地址 (例如: 192.168.1.1)")
            {
                Authorization.IpAddresses!.Add(NewIpTextBox.Text.Trim());
            }
        }

        private void RemoveIp_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { CommandParameter: string ipAddress })
            {
                Authorization.IpAddresses!.Remove(ipAddress);
            }
        }

        private void NewIpTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (NewIpTextBox.Text == "输入IP地址 (例如: 192.168.1.1)")
            {
                NewIpTextBox.Text = "";
                NewIpTextBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void NewIpTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewIpTextBox.Text))
            {
                NewIpTextBox.Text = "输入IP地址 (例如: 192.168.1.1)";
                NewIpTextBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.This.Limited) return;

            var saveString = JsonSerializer.Serialize(Authorization);
            var byteArray = Cryptography.Encrypt(saveString);

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "license",
                Filter = "Citation授权文件 (*.clc)|*.clc"
            };

            var result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                var filePath = saveFileDialog.FileName;

                using var fs = new FileStream(filePath, FileMode.Create);
                using var binaryWriter = new BinaryWriter(fs);
                binaryWriter.Write(byteArray);

                MainWindow.This.ShowToast("授权文件保存成功");
            }
            Close();
        }
    }
}