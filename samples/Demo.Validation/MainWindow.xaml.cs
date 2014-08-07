using System.Windows;

namespace Demo.Validation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var company = new Company
            {
                Name = "The Company",
                Address = "Some Road",
                Website = "http://thecompany.com",
            };

            DataContext = new MainViewModel(company);
        }
    }
}
