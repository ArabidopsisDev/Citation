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
            Authorization.IpAddresses!.Add(NewIpTextBox.Text.Trim());
            NewIpTextBox.Text = string.Empty;
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
            if (NewIpTextBox.Text == "输入本机序列号 (例如: 0A06A2-123733-5AC45C)")
            {
                NewIpTextBox.Text = "";
                NewIpTextBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void NewIpTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewIpTextBox.Text))
            {
                NewIpTextBox.Text = "输入本机序列号 (例如: 0A06A2-123733-5AC45C)";
                NewIpTextBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow!.Limited) return;

            Authorization.CreateTime = DateTime.Now;
            var saveString = JsonSerializer.Serialize(Authorization);
            var byteArray = new Cryptography().Encrypt(saveString);

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

                mainWindow.ShowToast("授权文件保存成功");
            }
            Close();
        }
    }
}