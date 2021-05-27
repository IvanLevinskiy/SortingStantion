using System.Windows.Controls;

namespace SortingStantion.frameMain
{
    /// <summary>
    /// Логика взаимодействия для frameMain.xaml
    /// </summary>
    public partial class frameMain : UserControl
    {
        public frameMain()
        {
            InitializeComponent();

            //DataBridge.SettingsFile = new Models.SettingsFile(@"AppData\Settings.xml");
           // var s = DataBridge.SettingsFile.GetValue("SrvL3Url");
        }
    }
}
