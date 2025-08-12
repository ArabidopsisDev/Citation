using System.Collections.ObjectModel;
using System.ComponentModel;
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public string ToSql()
        {
            var authorsBuild = new StringBuilder();
            foreach (var projectAuthor in Authors)
                authorsBuild.Append($"{projectAuthor}/");
            return
                $"INSERT INTO tb_Basic (ProjectName, ProjectPath, ProjectAuthors, ProjectGuid) " +
                $"VALUES ('{Name}', '{Path}', '{authorsBuild}', '{Guid}');";
        }
    }
}
