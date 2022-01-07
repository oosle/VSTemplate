using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Configuration;
using System.Collections.Generic;
$if$ ($targetframeworkversion$ >= 3.5)using System.Linq;
$endif$using System.Text;
$if$ ($targetframeworkversion$ >= 4.5)using System.Threading.Tasks;
$endif$using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using %RootFormName%.DAL;
using %RootFormName%.ControlLib;

namespace $safeprojectname$
{
    /// <summary>
    /// Interaction logic for $safeprojectname$.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PartnerControl frmChild { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void PartnerControl_Loaded(object sender, RoutedEventArgs e)
        {
            string dbString = ConfigurationManager.ConnectionStrings["Connection"].ToString();
            frmChild = (PartnerControl)sender;
            frmChild.SetupDataLayer(dbString, "EntityRef", 1);
        }

        private void ExecuteCommand(string command)
        {
            try
            {
                string path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
                path = path.Substring(6);

                var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardError = false;
                processInfo.RedirectStandardOutput = false;
                processInfo.WorkingDirectory = path + @"\Deploy";

                var process = Process.Start(processInfo);
                process.WaitForExit();
                process.Close();
            }
            catch (Exception ex)
            {
                MessageBoxResult msgBoxResult = MessageBox.Show(this,
                    ex.Message, "Error Occured", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Call the bound deployment script to push the embedded form to the DEV server
        private void btnDeployClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult msgBoxResult = MessageBox.Show(this,
                "Are you sure you want to deploy the Form to the [Partner] server?",
                "Deploy Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question);

            if (msgBoxResult == MessageBoxResult.OK)
            {
                ExecuteCommand("DeployForm.bat");
            }
        }
    }
}
