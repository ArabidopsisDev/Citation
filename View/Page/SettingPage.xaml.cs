// SettingPage.xaml.cs
using Citation.Model;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Citation.View.Page
{
    public partial class SettingPage : UserControl
    {
        public List<IFormatter> Formats { get; set; } = [];
        private bool flag = false;

        public SettingPage()
        {
            InitializeComponent();
            FormatComboBox.SelectionChanged += FormatComboBox_SelectionChanged;
        }

        private void SettingPage_Loaded(object sender, RoutedEventArgs e)
        {
            var targetNamespace = "Citation.Model.Format";
            Assembly assembly = Assembly.GetExecutingAssembly();

            var formatters = assembly.GetTypes()
                .Where(t => string.Equals(t.Namespace, targetNamespace, StringComparison.Ordinal)
                            && t.IsClass
                            && !t.IsAbstract
                            && typeof(IFormatter).IsAssignableFrom(t))
                .Select(t => (IFormatter)Activator.CreateInstance(t)!)
                .ToList();

            Formats.Clear();
            Formats.AddRange(formatters!);

            FormatComboBox.ItemsSource = Formats;
            FormatComboBox.SelectedIndex = BuildIndex();
        }

        private void FormatComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Manually quarantine the first visit
            if (!flag)
            {
                flag = true;
                return;
            }

            var index = FormatComboBox.SelectedIndex;
            Acceed.Shared.Execute($"UPDATE tb_Setting SET Formatter = '{Formats[index].FormatName}'");
        }

        private int BuildIndex()
        {
            var reader = Acceed.Shared.Query("SELECT * FROM tb_Setting");
            string formatter = "";
            while (reader.Read())
                formatter = reader["Formatter"].ToString()!;

            var index = 0;

            for (int i = 0; i<Formats.Count; i++)
            {
                IFormatter? item = Formats[i];
                if (item.FormatName == formatter)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }
    }
}