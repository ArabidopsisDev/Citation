using Citation.Model;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Citation.Utils;

namespace Citation.View
{
    public partial class ProjectEditWindow : Window
    {
        public Project? Project { get; set; }
        private readonly Action<string, bool>? _callback = null;

        public ProjectEditWindow(Project? project = null, Action<string, bool>? callback = null)
        {
            InitializeComponent();
            Project = project ?? new Project
            {
                Name = "新项目",
                Authors = [],
                Guid = System.Guid.NewGuid().ToString(),
                Password = "",
                AesKey = Randomization.RandomSeries(),
                AesIv = Randomization.RandomSeries()
            };

            _callback = callback;
            DataContext = Project;

            if (!App.EnableSecurity)
            {
                PasswordBox.Text = "由于 Citation Security© 已禁用，您无法创建密码";
                PasswordBox.IsEnabled = false;
            }

            PasswordBox.Text = Project.Password;
        }

        private void AddAuthor_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NewAuthorTextBox.Text))
            {
                Project.Authors.Add(NewAuthorTextBox.Text.Trim());
                NewAuthorTextBox.Clear();
            }
        }

        private void RemoveAuthor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: string author })
                Project.Authors?.Remove(author);
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (Project?.Path is null or "")
            {
                MessageBox.Show("请填写项目路径", "创建失败", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Project.Password = Cryptography.ComputeHash(Project.Password!);

            var destinationPath = Path.Combine(Project.Path, "data.accdb");
            if (!Directory.Exists(Project.Path))
                Directory.CreateDirectory(Project.Path);
            var sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dbtemplate.accdb");
            File.Copy(sourcePath, destinationPath, true);

            Acceed.Shared.ReConnect(destinationPath);
            Project.ToSql(Acceed.Shared.Connection);
            MessageBox.Show("项目创建成功！", "创建成功", MessageBoxButton.OK, MessageBoxImage.Information);

            _callback?.Invoke(destinationPath, false);
            Acceed.Shared.Close();
            Close();
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog()
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Multiselect = false
            };

            var result = dialog.ShowDialog();
            if (result == true)
                Project.Path = dialog.FolderName;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (Project != null)
            {
                Project.Password = PasswordBox.Text;
            }
        }
    }
}