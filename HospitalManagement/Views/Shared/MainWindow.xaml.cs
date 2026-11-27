using System.Windows;
using HospitalManagement.ViewModels;

namespace HospitalManagement.Views.Shared
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}