using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.IO.Compression;
using Aardvark.ViewModels;
using Aardvark.Domain;

namespace ImportExportProcessExamples
{
    class Program
    {
        private static IAppConfig _appConfig;

        static void Main(string[] args)
        {
            //args = new[]
            //{
            //    "/action:importone",
            //    "/source:" + @"D:\Temp\MedAssets.zip"
            //};

            var parsedArgs = args.Select(s => s.Split(new[] { ':' }, 2)).ToDictionary(s => s[0], s => s[1]);

            string _action = GetArgValue(parsedArgs, "/action");
            string _processId = GetArgValue(parsedArgs, "/processid");           
            string _source = GetArgValue(parsedArgs, "/source");
            string _destination = GetArgValue(parsedArgs, "/destination");
            string _csv = GetArgValue(parsedArgs, "/csv");           

            Console.OpenStandardInput();

            _appConfig = new AppConfig();

            if (!ValidAppConfig(_appConfig))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Invalid app.config values. Please check to make sure your app.config file has been configured.");
                Console.ReadKey();
                return;
            }

            switch (_action)
            {
                case "exportone":
                    if (_processId == null || _destination == null)
                    {
                        goto default;
                    }

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Exporting Single Project");
                    
                    ExportOne(_processId, _destination);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Done");

                    break;

                case "exportall":
                    if (_destination == null)
                    {
                        goto default;
                    }

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Exporting All Projects");

                    ExportAll(_destination);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Done");

                    break;

                case "zipall":
                    if (_source == null || _destination == null)
                    {
                        goto default;
                    }

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Zipping Process Templates Folders");

                    ZipFolderAll(_source, _destination);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Done");

                    break;

                case "zipone":
                    if (_source == null || _destination == null)
                    {
                        goto default;
                    }

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Zipping Single Process Template");

                    ZipFolderSingle(_source, _destination);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Done");

                    break;

                case "importone":
                    if (_source == null)
                    {
                        goto default;
                    }

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Importing Project");

                    ImportSingle(_source);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Done");

                    break;

                case "importbulk":
                    if (_csv == null)
                    {
                        goto default;
                    }

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Importing Bulk Project");

                    ImportBulk(_csv);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Done");

                    break;

                case "bugsbehavior":

                    if (_csv == null)
                    {
                        goto default;
                    }

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Updating BugsBehavior Setting");

                    UpdateBugsBehavior(_csv);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Done");

                    break;

                default:
                    ShowHelp();                 

                    break;
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            Console.ReadKey();

            return;           
        }

        private static Boolean ValidAppConfig(IAppConfig appConfig)
        {
            if (appConfig.ReadSetting("account") == "Not Found")
            {
                return false;
            }

            if (appConfig.ReadSetting("login") == "Not Found")
            {
                return false;
            }

            if (appConfig.ReadSetting("password") == "Not Found")
            {
                return false;
            }

            return true;
                    
        }

        private static string GetArgValue(Dictionary<string, string> args, string key)
        {
            if (args.ContainsKey(key)) {
                return args[key].ToString();
            }
            else {
                return null;
            }
        }

        private static List<ImportData> ListOfImportData(string csv)
        {
            if (!File.Exists(csv))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Invalid argument, csv file not found");
               
                return null;
            }

            List<ImportData> list = null;

            try
            {
                var reader = new StreamReader(File.OpenRead(@csv));
                var x = 1;
                list = new List<ImportData>();


                while (!reader.EndOfStream)
                {                    
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    if (x != 1)
                    {
                        ImportData item = new ImportData()
                        {
                            Project = values[0],
                            ProcessId = values[1],
                            BugsBehavior = values[2],
                            ZipFilePath = values[3]
                        };
                        list.Add(item);
                    }

                    x = x + 1;                 
                }               
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error loadin data from csv: " + ex.Message);

                return null;
            }       
            
            return list;
        }

        private static void ExportOne(string processId, string destinationFolder)
        {
            ExportOne(processId, destinationFolder, String.Empty);
        }

        private static void ExportOne(string processId, string destinationFolder, string name)
        {
            if (!Directory.Exists(destinationFolder))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("invlaid path argument: folder not found");
                return;
            }

            Console.ForegroundColor = ConsoleColor.White;
            
            if (name.Equals(String.Empty))
            {
                Console.Write("Exporting Process '" + processId + "': ");
            }
            else
            {
                Console.Write("Exporting Process '" + name + "': ");
            }          

            Processes process = new Processes(_appConfig);

            StandardResponseViewModel response = process.Export(processId, destinationFolder, name);

            if (response.Success)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Success");               
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;                              
                Console.WriteLine("Failed: " + response.Message);                  
            }

            process = null;
        }

        private static void ExportAll(string destinationFolder)
        {
            if (! Directory.Exists(destinationFolder))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Invlaid path argument: folder not found");
                return;
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Getting list of processes from account: ");

            Processes process = new Processes(_appConfig);

            ProcessesListViewModel vm = process.GetListOfProcessessRESTCall();

            if (vm == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Failed to get list of processes");                
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Success");               

                foreach (var item in vm.value)
                {
                    ExportOne(item.id, destinationFolder, item.name);
                }
            }

            vm = null;
            process = null;
        }

        private static void ZipFolderAll(string sourceFolder, string destinationFolder)
        {
            if (!Directory.Exists(sourceFolder))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Invlaid argument, start folder not found");
                return;
            }

            if (!Directory.Exists(destinationFolder))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Invlaid argument, destination folder not found");
                return;
            }

            if (Directory.GetFiles(destinationFolder).Length != 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Destination folder must be empty");
                return;
            }

            Processes process = new Processes(_appConfig);
            
            var folders = Directory.GetDirectories(sourceFolder);

            foreach (var folder in folders)
            {
                var name = folder.Replace(sourceFolder + "\\", "");
                var fullZipPath = destinationFolder + @"\" + name + ".zip";

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Creating '" + fullZipPath + "': ");

                ZipFile.CreateFromDirectory(folder, fullZipPath);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Success");
            };            
        }

        private static void ZipFolderSingle(string sourceFolder, string destinationFolder)
        {
            if (!Directory.Exists(sourceFolder))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Invalid argument, start folder not found");
                return;
            }

            if (!Directory.Exists(destinationFolder))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Invlaid argument, destination folder not found");
                return;
            }                      

            var lastIndex = sourceFolder.LastIndexOf("\\");
            var name = sourceFolder.Substring(lastIndex + 1);
            var fullZipPath = destinationFolder + @"\" + name + ".zip";

            if (File.Exists(fullZipPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Zip file already exists in folder");
                return;
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Creating '" + fullZipPath + "': ");

            ZipFile.CreateFromDirectory(sourceFolder, fullZipPath);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Success");
        }

        private static void ImportBulk(string csv)
        {
            if (!File.Exists(csv))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Invalid argument, csv file not found");
                return;
            }

            Processes process = new Processes(_appConfig);

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Getting list of process templates to import: ");

            var list = ListOfImportData(csv);

            if (list != null || list.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Success");
            }   
            else
            {
                return;
            }                     
            
            foreach (var item in list)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Importing process '" + item.ZipFilePath + "': ");

                ImportViewModel vm = process.ImportSingleProcessRESTCall(item.ZipFilePath);

                if (!vm.Success)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: " + vm.Message);

                    foreach (var result in vm.validationResults)
                    {
                        Console.WriteLine("Line " + result.line + " : " + result.description);
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Success");
                }
            }

            process = null;
        }

        private static void UpdateBugsBehavior(string csv)
        {
            if (!File.Exists(csv))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Invalid argument, csv file not found");
                return;
            }

            Processes process = new Processes(_appConfig);

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Getting list of projects: ");

            var list = ListOfImportData(csv);

            if (list != null || list.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Success");
            }
            else
            {
                return;
            }

            foreach (var item in list)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Updating Project '" + item.Project + "': ");

                var result = process.BugsBehaviorRESTCall(item.Project, new TeamSettings { bugsBehavior = item.BugsBehavior });

                if (!result.Success)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: " + result.Message);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Success");
                }
            }

            list = null;
            process = null;       
        }

        private static void ImportSingle(string zipFile)
        {
            if (!File.Exists(zipFile))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Invlaid argument, zip file not found");
                return;
            }

            Processes process = new Processes(_appConfig);
                       
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Importing process '" + zipFile + "': ");

            ImportViewModel vm = process.ImportSingleProcessRESTCall(zipFile);

            if (!vm.Success)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: " + vm.Message);
                Console.WriteLine("");

                foreach (var item in vm.validationResults)
                {
                    Console.WriteLine("Line " + item.line + " : " + item.description);
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Success");

               
            }            

            process = null;
        }

        public static void ShowHelp()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("");
            Console.WriteLine(@"         (`.  : \               __..----..__");
            Console.WriteLine(@"          `.`.| |:          _,-':::''' '  `:`-._");
            Console.WriteLine(@"            `.:\||       _,':::::'         `::::`-.");
            Console.WriteLine(@"              \\`|    _,':::::::'     `:.     `':::`.");
            Console.WriteLine(@"               ;` `-''  `::::::.                  `::\");
            Console.WriteLine(@"            ,-'      .::'  `:::::.         `::..    `:\");
            Console.WriteLine(@"          ,' /_) -.            `::.           `:.     |");
            Console.WriteLine(@"        ,'.:     `    `:.        `:.     .::.          \");
            Console.WriteLine(@"   __,-'   ___,..-''-.  `:.        `.   /::::.         |");
            Console.WriteLine(@"  |):'_,--'           `.    `::..       |::::::.      ::\");
            Console.WriteLine(@"   `-'                 |`--.:_::::|_____\::::::::.__  ::|");
            Console.WriteLine(@"                       |   _/|::::|      \::::::|::/\  :|");
            Console.WriteLine(@"                       /:./  |:::/        \__:::):/  \  :\");
            Console.WriteLine(@"                     ,'::'  /:::|        ,'::::/_/    `. ``-.__");
            Console.WriteLine(@"                     ''''   (//|/\      ,';':,-'         `-.__  `'--..__");
            Console.WriteLine(@"                                                             `''---::::");
       
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Project Aardvark");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("dan.hellem@Microsoft.com");
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Automation of managing processes for customers using phase 1 of process customization");
            Console.WriteLine("");

            //Console.ForegroundColor = ConsoleColor.Cyan;
            //Console.Write("Aardvark /action:");
            //Console.ForegroundColor = ConsoleColor.Gray;
            //Console.Write("[value] ");
            //Console.ForegroundColor = ConsoleColor.Cyan;
            //Console.Write("/processid: ");
            //Console.ForegroundColor = ConsoleColor.Gray;
            //Console.Write("[value] ");
            //Console.ForegroundColor = ConsoleColor.Cyan;
            //Console.Write("/source: ");
            //Console.ForegroundColor = ConsoleColor.Gray;
            //Console.Write("[value] ");
            //Console.ForegroundColor = ConsoleColor.Cyan;
            //Console.Write("/destination: ");
            //Console.ForegroundColor = ConsoleColor.Gray;
            //Console.Write("[value] ");
            //Console.ForegroundColor = ConsoleColor.Cyan;
            //Console.Write("/csv: ");
            //Console.ForegroundColor = ConsoleColor.Gray;
            //Console.Write("[value] ");
            //Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("/action:");              
            Console.WriteLine("[exportone]        Export a single process from VSTS to local folder");
            Console.WriteLine("[exportall]        Export all processes from VSTS to a local folder");
            Console.WriteLine("[zipone]           Zip up a local folder to be used for import");
            Console.WriteLine("[zipall]           Zip up a process folders in a root folder to be used for import");
            Console.WriteLine("[importone]        Import a process from local into VSTS");
            Console.WriteLine("[importbulk]       Import several processes from local into VSTS using csv file");
            Console.WriteLine("[bugsonbacklog]    Update BugsOnBacklog setting in VSTS using a csv file");

            Console.WriteLine(""); 

            Console.WriteLine("/processid:        guid for a process");
            Console.WriteLine("/source:           Source path of content");
            Console.WriteLine("/destination:      Destination path of content");
            Console.WriteLine("/csv:              Path of csv file");

            Console.WriteLine("");     

            //exportone
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Aardvark /action:");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("exportone ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("/processid:");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("[value] ");
            Console.ForegroundColor = ConsoleColor.Cyan;   
            Console.Write("/destination:");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("[value] ");         
            Console.WriteLine("");

            //export all
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Aardvark /action:");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("exportall ");           
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("/destination:");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("[value] ");           
            Console.WriteLine("");

            //zip one
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Aardvark /action:");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("zipone ");          
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("/source:");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("[value] ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("/destination:");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("[value] ");           
            Console.WriteLine("");

            //zip all
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Aardvark /action:");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("zipall ");          
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("/source:");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("[value] ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("/destination:");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("[value] ");           
            Console.WriteLine("");

            //import one
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Aardvark /action:");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("importone ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("/source:");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("[value] ");          
            Console.WriteLine("");

            //import bulk
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Aardvark /action:");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("importbulk ");           
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("/csv:");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("[value] ");
            Console.WriteLine("");           

            //bugs on backlog
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Aardvark /action:");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("bugsbehavior");           
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("/csv:");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("[value] ");
            Console.WriteLine("");
        }

    }
    public class ImportData
    {
        public string Project { get; set; }
        public string ProcessId { get; set; }
        public string ZipFilePath { get; set; }
        public string BugsBehavior { get; set; }
    }
}
