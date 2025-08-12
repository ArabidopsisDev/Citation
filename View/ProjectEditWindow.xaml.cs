using Citation.Model;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Citation.View
{
    public partial class ProjectEditWindow : Window
    {
        public Project? Project { get; set; }
        private Action<string, bool>? _Callback = null;

        public ProjectEditWindow(Project? project = null, Action<string, bool>? callback = null)
        {
            InitializeComponent();

            Project = project ?? new Project
            {
                Name = "新项目",
                Authors = new ObservableCollection<string>(),
                Guid = System.Guid.NewGuid().ToString(),
            };
            _Callback = callback;

            DataContext = Project;
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
                System.Windows.MessageBox.Show("请填写项目路径", "创建失败",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var destinationPath = Path.Combine(Project.Path, "data.accdb");
            if (!Directory.Exists(Project.Path))
                Directory.CreateDirectory(Project.Path);
            var sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "dbtemplate.accdb");
            File.Copy(sourcePath, destinationPath, true);

            Acceed.Shared.ReConnect(destinationPath);
            Acceed.Shared.Execute(Project.ToSql());
            System.Windows.MessageBox.Show("项目创建成功！", "创建成功",
                MessageBoxButton.OK, MessageBoxImage.Information);

            _Callback.Invoke(destinationPath, false);
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
    }
}