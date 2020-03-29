using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Management;
using System.Xml;
using Confuser.Core;
using Confuser.Core.Project;
using NDesk.Options;

namespace Confuser.CLI
{
    internal class Program
    {
        private static string hwid;

        private static int Main(string[] args)
        {
            ConsoleColor original = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Title = "DarksProtector v" + ConfuserEngine.Version;
            Title();
            int result;
            try
            {
                bool noPause = false;
                bool debug = false;
                string outDir = null;
                List<string> probePaths = new List<string>();
                List<string> plugins = new List<string>();
                OptionSet p = new OptionSet
                            {
                                {
                                    "n|nopause",
                                    "no pause after finishing protection.",
                                    delegate(string value)
                                    {
                                        noPause = (value != null);
                                    }
                                },
                                {
                                    "o|out=",
                                    "specifies output directory.",
                                    delegate(string value)
                                    {
                                        outDir = value;
                                    }
                                },
                                {
                                    "probe=",
                                    "specifies probe directory.",
                                    delegate(string value)
                                    {
                                        probePaths.Add(value);
                                    }
                                },
                                {
                                    "plugin=",
                                    "specifies plugin path.",
                                    delegate(string value)
                                    {
                                        plugins.Add(value);
                                    }
                                },
                                {
                                    "debug",
                                    "specifies debug symbol generation.",
                                    delegate(string value)
                                    {
                                        debug = (value != null);
                                    }
                                }
                            };
                List<string> files;
                try
                {
                    files = p.Parse(args);
                    bool flag = files.Count == 0;
                    if (flag)
                    {
                        throw new ArgumentException("No input files specified.");
                    }
                }
                catch (Exception ex)
                {
                    Input("ERROR: " + ex.Message);
                    return -1;
                }
                ConfuserParameters parameters = new ConfuserParameters();
                bool flag2 = files.Count == 1 && Path.GetExtension(files[0]) == ".darkpr";
                if (flag2)
                {
                    ConfuserProject proj = new ConfuserProject();
                    try
                    {
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(files[0]);
                        proj.Load(xmlDoc);
                        proj.BaseDirectory = Path.Combine(Path.GetDirectoryName(files[0]), proj.BaseDirectory);
                    }
                    catch (Exception ex2)
                    {
                        Program.WriteLineWithColor(ConsoleColor.Red, "Failed to load project:");
                        Program.WriteLineWithColor(ConsoleColor.Red, ex2.ToString());
                        return -1;
                    }
                    parameters.Project = proj;
                }
                int retVal = Program.RunProject(parameters);
                bool flag5 = Program.NeedPause() && !noPause;
                if (flag5)
                {
                    Credits();
                    Input("Press any key to close...");
                    Console.ReadKey(true);
                }
                result = retVal;
            }
            finally
            {
                Console.ForegroundColor = original;
            }
            return result;

            /*Input("Logging in...");
            if(!File.Exists("key.txt"))
            {
                Input("ERROR: Invalid key, please fill key.txt with a valid c.to auth key!");
                Input("Press any key to close...");
                Console.ReadKey(true);
                Environment.Exit(-1);
                return -1;
            }
            string key = File.ReadAllText("key.txt");
            try
            {
                using (HttpRequest httpRequest = new HttpRequest())
                {
                    httpRequest.IgnoreProtocolErrors = true;
                    httpRequest.Proxy = null;
                    httpRequest.UserAgent = Http.ChromeUserAgent();
                    httpRequest.ConnectTimeout = 30000;
                    string text = httpRequest.Post("https://cracked.to/auth.php", "a=auth&k=" + key + "&hwid=" + gethwid(), "application/x-www-form-urlencoded").ToString();
                    Dictionary<string, string> response = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);
                    if (text.Contains("error"))
                    {
                        Input("ERROR: Invalid key, please fill key.txt with a valid c.to auth key!");
                        Input("Press any key to close...");
                        Console.ReadKey(true);
                        Environment.Exit(-1);
                        return -1;
                    }
                    else if (text.Contains("auth"))
                    {
                        int result;
                        try
                        {
                            bool noPause = false;
                            bool debug = false;
                            string outDir = null;
                            List<string> probePaths = new List<string>();
                            List<string> plugins = new List<string>();
                            OptionSet p = new OptionSet
                            {
                                {
                                    "n|nopause",
                                    "no pause after finishing protection.",
                                    delegate(string value)
                                    {
                                        noPause = (value != null);
                                    }
                                },
                                {
                                    "o|out=",
                                    "specifies output directory.",
                                    delegate(string value)
                                    {
                                        outDir = value;
                                    }
                                },
                                {
                                    "probe=",
                                    "specifies probe directory.",
                                    delegate(string value)
                                    {
                                        probePaths.Add(value);
                                    }
                                },
                                {
                                    "plugin=",
                                    "specifies plugin path.",
                                    delegate(string value)
                                    {
                                        plugins.Add(value);
                                    }
                                },
                                {
                                    "debug",
                                    "specifies debug symbol generation.",
                                    delegate(string value)
                                    {
                                        debug = (value != null);
                                    }
                                }
                            };
                            List<string> files;
                            try
                            {
                                files = p.Parse(args);
                                bool flag = files.Count == 0;
                                if (flag)
                                {
                                    throw new ArgumentException("No input files specified.");
                                }
                            }
                            catch (Exception ex)
                            {
                                Input("ERROR: " + ex.Message);
                                return -1;
                            }
                            ConfuserParameters parameters = new ConfuserParameters();
                            bool flag2 = files.Count == 1 && Path.GetExtension(files[0]) == ".darkpr";
                            if (flag2)
                            {
                                ConfuserProject proj = new ConfuserProject();
                                try
                                {
                                    XmlDocument xmlDoc = new XmlDocument();
                                    xmlDoc.Load(files[0]);
                                    proj.Load(xmlDoc);
                                    proj.BaseDirectory = Path.Combine(Path.GetDirectoryName(files[0]), proj.BaseDirectory);
                                }
                                catch (Exception ex2)
                                {
                                    Program.WriteLineWithColor(ConsoleColor.Red, "Failed to load project:");
                                    Program.WriteLineWithColor(ConsoleColor.Red, ex2.ToString());
                                    return -1;
                                }
                                parameters.Project = proj;
                            }
                            int retVal = Program.RunProject(parameters);
                            bool flag5 = Program.NeedPause() && !noPause;
                            if (flag5)
                            {
                                Credits();
                                Input("Press any key to close...");
                                Console.ReadKey(true);
                            }
                            result = retVal;
                        }
                        finally
                        {
                            Console.ForegroundColor = original;
                        }
                        return result;
                    }
                    else
                    {
                        Input("ERROR: Invalid key, please fill key.txt with a valid c.to auth key!");
                        Environment.Exit(-1);
                        return -1;
                    }
                }
            }
            catch (Exception ex)
            {
                Input("ERROR: " + ex.Message);
                Environment.Exit(-1);
                return -1;
            }*/

        }

        private static string gethwid()
        {
            if (string.IsNullOrEmpty(hwid))
            {
                DriveInfo[] drives = DriveInfo.GetDrives();
                int num = drives.Length - 1;
                for (int i = 0; i <= num; i++)
                {
                    DriveInfo driveInfo = drives[i];
                    if (driveInfo.IsReady)
                    {
                        hwid = driveInfo.RootDirectory.ToString();
                        break;
                    }
                }
            }
            if (!string.IsNullOrEmpty(hwid) && hwid.EndsWith(":\\"))
            {
                hwid = hwid.Substring(0, hwid.Length - 2);
            }
            string result;
            using (ManagementObject managementObject = new ManagementObject("win32_logicaldisk.deviceid=\"" + hwid + ":\""))
            {
                managementObject.Get();
                result = managementObject["VolumeSerialNumber"].ToString();
            }
            return result;
        }

        // Token: 0x06000002 RID: 2 RVA: 0x00002530 File Offset: 0x00000730
        private static int RunProject(ConfuserParameters parameters)
        {
            Program.ConsoleLogger logger = new Program.ConsoleLogger();
            parameters.Logger = logger;
            Console.Title = "DarksProtector v" + ConfuserEngine.Version + " - Running...";
            ConfuserEngine.Run(parameters, null).Wait();
            return logger.ReturnValue;
        }

        // Token: 0x06000003 RID: 3 RVA: 0x00002578 File Offset: 0x00000778
        private static bool NeedPause()
        {
            return Debugger.IsAttached || string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROMPT"));
        }

        // Token: 0x06000005 RID: 5 RVA: 0x0000260C File Offset: 0x0000080C
        private static void WriteLineWithColor(ConsoleColor color, string txt)
        {
            ConsoleColor original = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(txt);
            Console.ForegroundColor = original;
        }

        // Token: 0x06000006 RID: 6 RVA: 0x00002635 File Offset: 0x00000835
        private static void WriteLine(string txt)
        {
            Console.WriteLine(txt);
        }

        // Token: 0x06000007 RID: 7 RVA: 0x0000263F File Offset: 0x0000083F
        private static void WriteLine()
        {
            Console.WriteLine();
        }

        public static void Input(string text)
        {
            Colorful.Console.Write(string.Format("                         [{0}] ", DateTime.Now), Color.DarkRed);
            Colorful.Console.Write(text + "\n", Color.White);
        }

        public static void Credits()
        {
            Colorful.Console.Write(string.Format("                         [{0}] ", DateTime.Now), Color.DarkRed);
            Colorful.Console.Write("Protector made by dark#5000\n", Color.Red);
        }

        private static void Title()
        {
            System.Console.Clear();
            Colorful.Console.WriteLine();
            Colorful.Console.Write("                                        ▓█████▄  ▄▄▄       ██▀███   ██ ▄█▀  ██████                   \n", Color.Red);
            Colorful.Console.Write("                                        ▒██▀ ██▌▒████▄    ▓██ ▒ ██▒ ██▄█▒ ▒██    ▒                   \n", Color.Red);
            Colorful.Console.Write("                                        ░██   █▌▒██  ▀█▄  ▓██ ░▄█ ▒▓███▄░ ░ ▓██▄                     \n", Color.Red);
            Colorful.Console.Write("                                        ░▓█▄   ▌░██▄▄▄▄██ ▒██▀▀█▄  ▓██ █▄   ▒   ██▒                  \n", Color.Red);
            Colorful.Console.Write("                                        ░▒████▓  ▓█   ▓██▒░██▓ ▒██▒▒██▒ █▄▒██████▒▒                  \n", Color.Red);
            Colorful.Console.Write("                                         ▒▒▓  ▒  ▒▒   ▓▒█░░ ▒▓ ░▒▓░▒ ▒▒ ▓▒▒ ▒▓▒ ▒ ░                  \n", Color.Red);
            Colorful.Console.Write("                                         ░ ▒  ▒   ▒   ▒▒ ░  ░▒ ░ ▒░░ ░▒ ▒░░ ░▒  ░ ░                  \n", Color.Red);
            Colorful.Console.Write("                                         ░ ░  ░   ░   ▒     ░░   ░ ░ ░░ ░ ░  ░  ░                    \n", Color.Red);
            Colorful.Console.Write("                                           ░          ░  ░   ░     ░  ░         ░                    \n", Color.Red);
            Colorful.Console.Write("                                         ░                                                           \n", Color.Red);
            Colorful.Console.Write("                       ██▓███   ██▀███   ▒█████  ▄▄▄█████▓▓█████  ▄████▄  ▄▄▄█████▓ ▒█████   ██▀███  \n", Color.Red);
            Colorful.Console.Write("                      ▓██░  ██▒▓██ ▒ ██▒▒██▒  ██▒▓  ██▒ ▓▒▓█   ▀ ▒██▀ ▀█  ▓  ██▒ ▓▒▒██▒  ██▒▓██ ▒ ██▒\n", Color.Red);
            Colorful.Console.Write("                      ▓██░ ██▓▒▓██ ░▄█ ▒▒██░  ██▒▒ ▓██░ ▒░▒███   ▒▓█    ▄ ▒ ▓██░ ▒░▒██░  ██▒▓██ ░▄█ ▒\n", Color.Red);
            Colorful.Console.Write("                      ▒██▄█▓▒ ▒▒██▀▀█▄  ▒██   ██░░ ▓██▓ ░ ▒▓█  ▄ ▒▓▓▄ ▄██▒░ ▓██▓ ░ ▒██   ██░▒██▀▀█▄  \n", Color.Red);
            Colorful.Console.Write("                      ▒██▒ ░  ░░██▓ ▒██▒░ ████▓▒░  ▒██▒ ░ ░▒████▒▒ ▓███▀ ░  ▒██▒ ░ ░ ████▓▒░░██▓ ▒██▒\n", Color.Red);
            Colorful.Console.Write("                      ▒▓▒░ ░  ░░ ▒▓ ░▒▓░░ ▒░▒░▒░   ▒ ░░   ░░ ▒░ ░░ ░▒ ▒  ░  ▒ ░░   ░ ▒░▒░▒░ ░ ▒▓ ░▒▓░\n", Color.Red);
            Colorful.Console.Write("                      ░▒ ░       ░▒ ░ ▒░  ░ ▒ ▒░     ░     ░ ░  ░  ░  ▒       ░      ░ ▒ ▒░   ░▒ ░ ▒░\n", Color.Red);
            Colorful.Console.Write("                      ░░         ░░   ░ ░ ░ ░ ▒    ░         ░   ░          ░      ░ ░ ░ ▒    ░░   ░ \n", Color.Red);
            Colorful.Console.Write("                                  ░         ░ ░              ░  ░░ ░                   ░ ░     ░     \n", Color.Red);
            Colorful.Console.Write("                                                                 ░                                   \n", Color.Red);
            Colorful.Console.Write("\n");
        }

        // Token: 0x0200000A RID: 10
        private class ConsoleLogger : ILogger
        {
            // Token: 0x0600006F RID: 111 RVA: 0x000042EB File Offset: 0x000024EB
            public ConsoleLogger()
            {
                this.begin = DateTime.Now;
            }

            // Token: 0x17000015 RID: 21
            // (get) Token: 0x06000070 RID: 112 RVA: 0x00004300 File Offset: 0x00002500
            // (set) Token: 0x06000071 RID: 113 RVA: 0x00004308 File Offset: 0x00002508
            public int ReturnValue { get; private set; }

            public void Log(string msg) => Input(msg);

            public void LogFormat(string format, params object[] args) => Input(string.Format(format, args));

            public void Error(string msg) => Input(msg);

            public void ErrorException(string msg, Exception ex)
            {
                Input("ERROR: " + msg);
                Input("Exception: " + ex);
            }

            public void ErrorFormat(string format, params object[] args) => Input("ERROR: " + string.Format(format, args));

            // Token: 0x0600007C RID: 124 RVA: 0x00004429 File Offset: 0x00002629
            public void Progress(int progress, int overall)
            {
            }

            // Token: 0x0600007D RID: 125 RVA: 0x00004429 File Offset: 0x00002629
            public void EndProgress()
            {
            }

            // Token: 0x0600007E RID: 126 RVA: 0x0000442C File Offset: 0x0000262C
            public void Finish(bool successful)
            {
                DateTime now = DateTime.Now;
                string timeString = string.Format("at {0}, {1}:{2:d2} elapsed.", now.ToShortTimeString(), (int)now.Subtract(this.begin).TotalMinutes, now.Subtract(this.begin).Seconds);
                if (successful)
                {
                    Console.Title = "DarksProtector v" + ConfuserEngine.Version + " - Success";
                    Input("Finished " + timeString);
                    this.ReturnValue = 0;
                }
                else
                {
                    Console.Title = "DarksProtector v" + ConfuserEngine.Version + " - Fail";
                    Input("Failed " + timeString);
                    this.ReturnValue = 1;
                }
            }

            // Token: 0x04000017 RID: 23
            private readonly DateTime begin;
        }
    }
}
