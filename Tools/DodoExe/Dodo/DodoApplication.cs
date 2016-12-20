using CommandLine;
using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Threading;

namespace Dodo
{
    public class DodoApplication
    {
        #region PSHost stuff
        private bool shouldExit;
        private int exitCode;
        public bool ShouldExit
        {
            get { return this.shouldExit; }
            set { this.shouldExit = value; }
        }
        public int ExitCode
        {
            get { return this.exitCode; }
            set { this.exitCode = value; }
        }
        #endregion

        public const string _DODOModuleManifestPath = @"DODO\dodo.psd1";


        private static void Main(string[] args)
        {
            var cleanup = true;
            try
            {
                Welcome();
                var arguments = VerifyInput(args);
                var executionPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                if (arguments.Export)
                {
                    Console.WriteLine("Performing DODO module export...");
                    GenerateDODOModule(executionPath);
                    cleanup = false;
                    return;
                }

                var me = new DodoApplication();
                var host = new DodoPsHost(me);

                Console.WriteLine("Generating DODO module assets...");
                GenerateDODOModule(executionPath);

                var sessionState = InitialSessionState.CreateDefault();
                sessionState.ImportPSModulesFromPath(Path.Combine(executionPath, _DODOModuleManifestPath));

                using (var runSpace = RunspaceFactory.CreateRunspace(host, sessionState))
                {
                    runSpace.Open();

                    using (var powerShell = PowerShell.Create(sessionState))
                    {
                        powerShell.Runspace = runSpace;
                        powerShell.AddScript("$ErrorActionPreference = 'Stop'");
                        powerShell.AddScript("Import-Module" + " \"" + Path.Combine(executionPath, _DODOModuleManifestPath) + "\"");
                        powerShell.Invoke();

                        powerShell.AddCommand("Run-DODO");

                        if (!String.IsNullOrEmpty(arguments.TemplatePath))
                        {
                            powerShell.AddParameter("ConfigurationJSONPath", arguments.TemplatePath);
                        }
                      
                        if (!String.IsNullOrEmpty(arguments.ContainerName))
                        {
                            powerShell.AddParameter("ContainerName", arguments.ContainerName);
                        }

                        if (!String.IsNullOrEmpty(arguments.Command))
                        {
                            powerShell.AddParameter("Command", arguments.Command);
                        }

                        if (!String.IsNullOrEmpty(arguments.Arguments))
                        {
                            powerShell.AddParameter("Arguments", arguments);
                        }

                        if (!String.IsNullOrEmpty(arguments.ParaneterPath))
                        {
                            powerShell.AddParameter("ParametersJSONPath", arguments.ParaneterPath);
                        }

                        var output = new PSDataCollection<PSObject>();
                        output.DataAdding += new EventHandler<DataAddingEventArgs>(Output_DataAdding);

                        powerShell.Streams.Debug.DataAdding += Debug_DataAdding;
                        powerShell.Streams.Error.DataAdding += Error_DataAdding;
                        IAsyncResult result = powerShell.BeginInvoke<PSObject, PSObject>(null, output);

                        while (result.IsCompleted == false)
                        {
                            Thread.Sleep(1000);
                        }

                        Console.WriteLine("Execution has stopped. The pipeline state: " + powerShell.InvocationStateInfo.State);

                        if (powerShell.InvocationStateInfo.State == PSInvocationState.Failed)
                        {
                            var color = Console.ForegroundColor;
                            Console.ForegroundColor = ConsoleColor.Red;

                            Console.WriteLine("Reason: " + powerShell.InvocationStateInfo.Reason);
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                if (cleanup)
                {
                    Console.WriteLine("Execution finished, cleaning up module files...");
                    var executionPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    var dodoPath = Path.Combine(executionPath, "DODO");

                    if (Directory.Exists(dodoPath))
                    {
                        Directory.Delete(dodoPath, true);
                    }
                }
            }
        }

        #region Stream Events
      
        private static void Error_DataAdding(object sender, DataAddingEventArgs e)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: " + e.ItemAdded);
            Console.ForegroundColor = color;
        }
      
        private static void Output_DataAdding(object sender, DataAddingEventArgs e)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("Output: " + e.ItemAdded);
            Console.ForegroundColor = color;
        }

        private static void Debug_DataAdding(object sender, DataAddingEventArgs e)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("Debug: " + e.ItemAdded);
            Console.ForegroundColor = color;
        }

        #endregion

        /// <summary>
        /// Outputs the DODO modules to file
        /// This can then be consumed by the PS session manager instance
        /// </summary>
        /// <param name="path"></param>
        private static void GenerateDODOModule(string path)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resources = assembly.GetManifestResourceNames().Where(r => r.Contains(".psm") || r.Contains(".psd")).ToList();

            var dodoPath = Path.Combine(path, "DODO");
            if (!Directory.Exists(dodoPath))
            {
                Directory.CreateDirectory(dodoPath);
            }

            foreach (var resource in resources)
            {
                using (Stream stream = assembly.GetManifestResourceStream(resource))
                using (StreamReader reader = new StreamReader(stream))
                {
                    var fileName = resource.Replace("Dodo.Modules.", String.Empty);
                    var script = reader.ReadToEnd();
                    File.WriteAllText(Path.Combine(dodoPath, fileName), script);
                }
            }
        }

        #region Welcome
        private static void Welcome()
        {
            var color = Console.ForegroundColor;

            if (Console.ForegroundColor != ConsoleColor.DarkGreen)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
            }

            Console.WriteLine(@"     ______ __");
            Console.WriteLine(@"   {-_-_= '. `'.");
            Console.WriteLine(@"    {=_=_-  \   \");
            Console.WriteLine(@"     {_-_   |   /");
            Console.WriteLine(@"      '-.   |  /    .===,");
            Console.WriteLine(@"   .--.__\  |_(_,==`  ( o)'-.");
            Console.WriteLine(@"  `---.=_ `     ;      `/    \");
            Console.WriteLine(@"      `,-_       ;    .'--') /");
            Console.WriteLine(@"        {=_       ;=~  `  `");
            Console.WriteLine(@"         `//__,-=~");
            Console.WriteLine(@"         <<__ \\__");
            Console.WriteLine(@"         /`)))/`)))");

            Console.ForegroundColor = color;
        }

        #endregion

        public static DodoArguments VerifyInput(string[] args)
        {
            var options = new DodoArguments();
             
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                if (options.Help)
                {
                    Console.WriteLine(CommandLine.Text.HelpText.AutoBuild(options));
                    return null;
                }
            }

            Console.WriteLine("Input - Path: " + options.TemplatePath);
            Console.WriteLine("Input - Arguments: " + options.Arguments);
            
            if (String.IsNullOrWhiteSpace(options.TemplatePath) && !options.Export)
            {
                Console.WriteLine("Please enter the full path of your JSON file:");
                options.TemplatePath = Console.ReadLine();
            }
            
            return options;
        }
    }

    public class DodoArguments
    {
        [Option('T', "Template", Required = false, HelpText = "Specifies the JSON template path")]
        public string TemplatePath { get; set; }

        [Option('P', "Parameters", Required = false, HelpText = "Specifies the JSON template parameter file path")]
        public string ParaneterPath { get; set; }

        [Option('N', "ContainerName", Required = false, HelpText = "Specifies the JSON template container name to deploy")]
        public string ContainerName { get; set; }

        [Option('H', "help", HelpText = "Prints this help", Required = false, DefaultValue = false)]
        public bool Help { get; set; }

        [Option('C', "Command", Required = false, HelpText = "Specifies the command to run on a deployment container")]
        public string Command { get; set; }

        [Option('A', "Arguments", Required = false, HelpText = "Specifies the optional additional arguments for a command")]
        public string Arguments { get; set; }

        [Option('E', "export", Required = false, DefaultValue = false, HelpText = "Export DODO module to the current directory")]
        public bool Export { get; set; }

    }

}
