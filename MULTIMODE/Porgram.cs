using System;
using System.ServiceProcess;
using System.Windows.Forms;
using Application = System.Windows.Forms.Application;

namespace MULTIMODE
{
    static class Porgram
    {
        static void Main(string[] args)
        {
            string mode = args.Length > 0 ? args[0].ToLower() : "-cli";

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
        private static ConsoleKeyInfo keyInfo;

        private static bool active;

        private static Service service;

        public static void Run()
        {
            active = false;

            service = new Service();

            do
            {
                keyInfo = Console.ReadKey(intercept: true);

                if (keyInfo.Key == ConsoleKey.X)
                {
                    if (active)
                    {
                        service.DeInitialize();
                    }
                }
                else
                {
                    if (keyInfo.Key == ConsoleKey.S)
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
            } while (keyInfo.Key != ConsoleKey.X);
        }
    }

    class GUI : Form
    {
        private static bool active;

        private static Service service;

        private readonly Button _startstop = new Button { Text = "Start", Dock = DockStyle.Top };

        public GUI()
        {
            active = false;

            service = new Service();

            Text = "GUI";

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
            ServiceName = "SVC";
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