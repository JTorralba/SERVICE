
using System;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Windows.Forms;

using Application = System.Windows.Forms.Application;

using Standard;

namespace MULTIMODE
{
    static class Porgram
    {
        static void Main(string[] args)
        {
            string mode = args.Length > 0 ? args[0].ToLower() : "-cli";

            string assemblyFileDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            string assemblyFileName = Assembly.GetExecutingAssembly().GetName().Name;

            string logFileDirectory = $"{assemblyFileDirectory}\\logs";

            string logFileName = string.Empty;

            logFileName = $"{assemblyFileName}.log";

            string logFile = Path.Combine(logFileDirectory, logFileName);

            Directory.CreateDirectory(@logFileDirectory);

            Console.SetOut(new DualWriter($"{logFile}"));

            switch (mode)
            {
                case "-cli":
                    CLI.Run();
                    break;

                case "-gui":
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new GUI());
                    break;

                case "-svc":
                    ServiceBase.Run(new SVC());
                    break;

                default:
                    Console.WriteLine("-cli | -gui | -svc");
                    break;
            }
        }
    }

    static class CLI
    {
        private static ConsoleKeyInfo consoleKeyInfo;

        private static bool active;

        private static Service service;

        public static void Run()
        {
            active = false;

            service = new Service();

            do
            {
                consoleKeyInfo = Console.ReadKey(intercept: true);

                if (consoleKeyInfo.Key == ConsoleKey.X)
                {
                    if (active)
                    {
                        service.DeInitialize();
                    }
                }
                else
                {
                    if (consoleKeyInfo.Key == ConsoleKey.S)
                    {
                        active = !active;

                        if (!active)
                        {
                            service.DeInitialize();
                        }
                        else
                        {
                            service.Initialize();
                        }
                    }
                }
            } while (consoleKeyInfo.Key != ConsoleKey.X);
        }
    }

    class GUI : Form
    {
        private static bool active;

        private static Service service;

        private static Button _startstop = new Button { Text = "Start", Dock = DockStyle.Top };

        public GUI()
        {
            active = false;

            service = new Service();

            Text = "MULTIMODE";

            Controls.Add(_startstop);

            _startstop.Click += (s, e) =>
            {
                active = !active;

                if (!active)
                {
                    service.DeInitialize();
                    
                    _startstop.Text = "Start";
                }
                else
                {
                    service.Initialize();

                    _startstop.Text = "Stop";
                }
            };

            FormClosing += (s, e) =>
            {
                if (active)
                {
                    service.DeInitialize();
                }
            }
            ;
        }
    }

    class SVC : ServiceBase
    {
        private static Service service;

        public SVC()
        {
            ServiceName = "MULTIMODE";
            CanStop = true;
            CanPauseAndContinue = false;
            AutoLog = true;
            service = new Service();
        }

        protected override void OnStart(string[] args)
        {
            service.Initialize();
        }

        protected override void OnStop()
        {
            service.DeInitialize();
        }
    }
}