using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.OleDb;
using System.Text;

namespace Citation.Model
{
    public class Project : INotifyPropertyChanged
    {
        public string? Name
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string? Path
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(Path));
                }
            }
        }

        public string? Password
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(Password));
                }
            }
        }

        public ObservableCollection<string> Authors
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged(nameof(Authors));
            }
        }

        public string? Guid { get; set; }

        public string? AesKey { get; set; }
        public string? AesIv { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public void ToSql(OleDbConnection connection)
        {
            var authorsBuild = new StringBuilder();
            foreach (var projectAuthor in Authors)
                authorsBuild.Append($"{projectAuthor}/");
            var sqlCommand = 
                $"INSERT INTO tb_Basic (ProjectName, ProjectPath, ProjectAuthors, ProjectGuid, ProjectPassword, ProjectKey, ProjectIv) " +
                $"VALUES (?, ?, ?, ?, ?, ?, ?);";

            var command = new OleDbCommand(sqlCommand, connection);

            command.Parameters.AddWithValue("?", Name);
            command.Parameters.AddWithValue("?", Path);
            command.Parameters.AddWithValue("?", authorsBuild.ToString());
            command.Parameters.AddWithValue("?", Guid);
            command.Parameters.AddWithValue("?", Password);
            command.Parameters.AddWithValue("?", AesKey);
            command.Parameters.AddWithValue("?", AesIv);

            command.ExecuteNonQuery();
        }
    }
}
