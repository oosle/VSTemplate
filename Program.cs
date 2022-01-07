using System;
using System.IO;
using VSLangProj;
using EnvDTE; // VS2017 and above extension, COM automation

namespace VSTemplate
{
    internal class Program
    {
        // Visual Studio project type GUID, this one is for C#
        private static Guid _typeProject = new Guid("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}");

        // Not standard project templates, have been modified for internal use, dependency
        private static string _sTemplates = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");

        private static string _outputPath = string.Empty;
        private static string _slnName = string.Empty;
        private static string _dalPath = string.Empty;
        private static string _libPath = string.Empty;
        private static string _slnPath = string.Empty;

        private static bool ParseArgs(string[] args)
        {
            bool retValue = false;

            // Basic command line parser to decode some required information
            if (args.Length == 4)
            {
                if (args[0].ToLower() == "-p" || args[0].ToLower() == "/p")
                    _outputPath = args[1];
                if (args[2].ToLower() == "-s" || args[2].ToLower() == "/s")
                    _slnName = args[3];

                if (!_outputPath.IsNullOrEmpty() && !_slnName.IsNullOrEmpty())
                {
                    _dalPath = Path.Combine(_outputPath, _slnName + ".DAL");
                    _libPath = Path.Combine(_outputPath, _slnName + ".ControlLib");
                    _slnPath = Path.Combine(_outputPath, _slnName);

                    retValue = true;
                }
            }

            if (retValue == false)
            {
                Console.WriteLine(@"Complex CLI solution template generator utility.");
                Console.WriteLine(@"Syntax : VSTemplate.exe -p <path> -s <solutuon>");
                Console.WriteLine(@"Example: VSTemplate.exe -p C:\Temp -s ExampleTemp");
            }

            return (retValue);
        }

        // Find a project by name from the projects within the passed solution
        private static Project FindSolutionProject(Solution soln, string name)
        {
            foreach (Project project in soln.Projects)
            {
                if (project.Name == name)
                    return (project);
            }

            return (null);
        }

        // Find a project by name from the projects within the passed solution
        private static Reference FindProjectReference(VSProject proj, string name)
        {
            foreach (Reference refObject in proj.References)
            {
                if (refObject.Name == name)
                    return (refObject);
            }

            return (null);
        }

        private static bool SetupDALProject(Solution solution)
        {
            bool retValue = false;

            try
            {
                Console.WriteLine("[{0}.DAL] Project Setup...", _slnName);

                string template =
                    Path.Combine(_sTemplates, @"ClassLibrary\csClassLibrary.vstemplate");
                string niceTableName = string.Empty;
                string dataPath = string.Empty;

                solution.AddFromTemplate(template, _dalPath, _slnName + ".DAL");
                Project proj = FindSolutionProject(solution, _slnName + ".DAL");

                dataPath = Path.Combine(_dalPath, "Classes");
                if (Directory.Exists(dataPath))
                {
                    Directory.CreateDirectory(dataPath);
                }
                else proj.ProjectItems.AddFolder("Classes");

                dataPath = Path.Combine(_dalPath, "Interfaces");
                if (Directory.Exists(dataPath))
                {
                    Directory.CreateDirectory(dataPath);
                }
                else proj.ProjectItems.AddFolder("Interfaces");

                dataPath = Path.Combine(_dalPath, "Repositories");
                if (Directory.Exists(dataPath))
                {
                    Directory.CreateDirectory(dataPath);
                }
                else proj.ProjectItems.AddFolder("Repositories");

                retValue = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: [{0}.DAL] project setup failure.", _slnName);
                Console.WriteLine("Exception: {0}", ex.Message);
            }

            return (retValue);
        }

        private static bool SetupControlLibProject(Solution solution)
        {
            bool retValue = false;

            try
            {
                Console.WriteLine("[{0}.ControlLib] Project Setup...", _slnName);

                string template =
                    Path.Combine(_sTemplates, @"WPFControlLibrary\csWPFControlLibrary.vstemplate");

                solution.AddFromTemplate(template, _libPath, _slnName + ".ControlLib");

                // Bind the DAL layer to the WPF user control as reference
                Project dalProj = FindSolutionProject(solution, _slnName + ".DAL");
                Project libProj = FindSolutionProject(solution, _slnName + ".ControlLib");

                VSProject proj = (VSProject)libProj.Object; // Can't typecast COM directly
                proj.References.AddProject(dalProj);

                retValue = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: [{0}.ControlLib] project setup failure.", _slnName);
                Console.WriteLine("Exception: {0}", ex.Message);
            }

            return (retValue);
        }

        private static bool SetupFormProject(Solution solution)
        {
            bool retValue = false;

            try
            {
                Console.WriteLine("[{0}] Project Setup...", _slnName);

                string template =
                    Path.Combine(_sTemplates, @"WPFApplication\csWPFApplication.vstemplate");

                solution.AddFromTemplate(template, _slnPath, _slnName);

                // Bind DAL layer and WPF user control to sandbox binary project as reference
                Project dalProj = FindSolutionProject(solution, _slnName + ".DAL");
                Project libProj = FindSolutionProject(solution, _slnName + ".ControlLib");
                Project binProj = FindSolutionProject(solution, _slnName);

                VSProject proj = (VSProject)binProj.Object; // Can't typecast COM directly
                proj.References.AddProject(dalProj);
                proj.References.AddProject(libProj);

                retValue = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: [{0}] project setup failure.", _slnName);
                Console.WriteLine("Exception: {0}", ex.Message);
            }

            return (retValue);
        }

        private static bool SolutionPostProcessing()
        {
            bool retValue = false;

            try
            {
                Console.WriteLine("[{0}] Solution Post Processing...", _slnName);

                Utility.ReplaceText(_slnPath, "*.xaml.cs", "(%RootFormName%)", _slnName);
                Utility.ReplaceText(_libPath, "*.xaml.cs", "(%RootFormName%)", _slnName);

                string ctrlName = (_slnName + ".ControlLib").ToTitleCaseSQL();

                File.Move(
                    Path.Combine(_libPath, "PartnerControl.xaml"),
                    Path.Combine(_libPath, ctrlName + ".xaml"));

                File.Move(
                    Path.Combine(_libPath, "PartnerControl.xaml.cs"),
                    Path.Combine(_libPath, ctrlName + ".xaml.cs"));

                Utility.ReplaceText(_libPath, "*.*", "PartnerControl", ctrlName);
                Utility.ReplaceText(_slnPath, "*.*", "PartnerControl", ctrlName);

                retValue = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: [{0}] solution post processing.", _slnName);
                Console.WriteLine("Exception: {0}", ex.Message);
            }

            return (retValue);
        }

        // ------------------------------------------------------------------------------------------------
        // Force a single thread for COM based console application; by default C# console applications
        // are declared with the MTAThread model. VB will default to STAThread, if you don't do this
        // the COM code of Visual Studio is NOT multi-threaded, this leads to unpredictable behaviour
        // when using COM with random "application is busy" exceptions, causing the progam to crash.
        // ------------------------------------------------------------------------------------------------
        [STAThread]
        private static void Main(string[] args)
        {
            Console.WriteLine("");

            // Try VS2022 / VS2019 instances
            Type type = Type.GetTypeFromProgID("VisualStudio.DTE.17.0", false);
            if (type == null)
                type = Type.GetTypeFromProgID("VisualStudio.DTE.16.0");

            if (type != null)
            {
                Console.WriteLine("VS2022/VS2019 is installed, tool can be used.");
            }

            Object obj = Activator.CreateInstance(type, true);
            DTE dte = (DTE)obj;

            Console.WriteLine(string.Format("{0} - [v{1}.{2}.{3}.{4}]",
                Utility.pAssembly,
                Utility.pVersion.Major, Utility.pVersion.Minor,
                Utility.pVersion.Build, Utility.pVersion.Revision));

            if (!Utility.pDBConnect.IsNullOrEmpty())
                Console.WriteLine("DB = {0}", Utility.pDBConnect);

            if (ParseArgs(args))
            {
                Console.WriteLine("");

                if (dte == null)
                {
                    Console.WriteLine("Error: VS2022 or VS2019 needs to be installed to use this tool.");
                    return;
                }

                if (Directory.Exists(_outputPath))
                {
                    Console.WriteLine("Specified output directory already exists, aborting.");
                    return;
                }

                // Error conditions for pre-existing elements that need to exist (dependancies)
                if (!Directory.Exists(_sTemplates))
                {
                    Console.WriteLine("Error: Templates directory was not found.");
                    return;
                }

                if (!Directory.Exists(Path.Combine(_sTemplates, "ClassLibrary")))
                {
                    Console.WriteLine("Error: ClassLibrary template was not found.");
                    return;
                }

                if (!Directory.Exists(Path.Combine(_sTemplates, "WPFControlLibrary")))
                {
                    Console.WriteLine("Error: WPFControlLibrary template was not found.");
                    return;
                }

                if (!Directory.Exists(Path.Combine(_sTemplates, "WPFApplication")))
                {
                    Console.WriteLine("Error: WPFApplication template was not found.");
                    return;
                }

                // COM multi-threading handling code for VS2022 & VS2019, hide OLE errors
                MessageFilter.Register();

                // Create a new solution from scratch using DTE COM interface
                Console.WriteLine("[{0}] Solution Setup...", _slnName);
                dte.SuppressUI = true;
                dte.Solution.Create(_outputPath, _slnName);
                Solution solution = dte.Solution;

                if (SetupDALProject(solution))
                {
                    if (SetupControlLibProject(solution))
                    {
                        if (SetupFormProject(solution))
                        {
                            if (SolutionPostProcessing())
                            {
                                Console.WriteLine(
                                    "Solution created: {0}\\{1}.sln", _outputPath, _slnName);
                            }
                        }
                    }
                }

                solution.Properties.Item("StartupProject").Value = _slnName;
                dte.ExecuteCommand("File.SaveAll");
                solution.Close();

                dte.Quit();

                MessageFilter.Revoke();
            }
        }
    }
}
