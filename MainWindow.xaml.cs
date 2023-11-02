using System.Reflection;
using System;
using System.Windows;
using BlazorStrap;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BlazorTestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            mainWindow.Title = "6433-08-MagstimEMGTestApp " + getRunningVersion();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddWpfBlazorWebView();
            serviceCollection.AddBlazorWebViewDeveloperTools();
            serviceCollection.AddBlazorStrap();
            Resources.Add("services", serviceCollection.BuildServiceProvider());
            WindowState = WindowState.Maximized;
        }
        private string getRunningVersion()
        {
            try
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
            catch (Exception)
            {
                return "1.2.0";
            }
        }
    }
}
