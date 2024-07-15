using System.Windows;

namespace date_update
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            frmContent.Content = new Pages.MainPage();
        }
    }
}
