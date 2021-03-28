using Microsoft.DotNet.PlatformAbstractions;
using SharedLibraryCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace IW4M_Restart
{
    public class FindProcess
    {
        private const string UNIX_PID_REGX = @"\w+\s+(\d+).*";
        private const string WIND_PID_REGX = @".*\s+(\d+)";
        private Server server;

        public Server MyServer
        {
            get { return server; }
            set { server = value; }
        }

        public void FindAndKillServer()
        {
            List<string> pidList = new List<string>();
            List<string> list;
            string port = server.Port.ToString();

            server.Logger.WriteInfo($"Attempting to kill process on port {port}");

            switch (GetOSName())
            {
                case Platform.Linux:
                    list = FindUnixProcess();
                    list = FilterProcessListBy(processList: list, filter: ":" + port);

                    foreach (string pidString in list)
                    {
                        string pid = GetPidFrom(pidString: pidString, pattern: UNIX_PID_REGX);

                        if (!string.IsNullOrEmpty(pid))
                        {
                            pidList.Add(pid);
                        }
                    }

                    break;
                case Platform.Windows:
                    list = FindWindowsProcess();
                    list = FilterProcessListBy(processList: list, filter: ":" + port);

                    foreach (string pidString in list)
                    {
                        string pid = GetPidFrom(pidString: pidString, pattern: WIND_PID_REGX);

                        if (!string.IsNullOrEmpty(pid))
                        {
                            pidList.Add(pid);
                        }
                    }

                    break;
                default:
                    server.Logger.WriteError("This plugin was meant for Linux or Windows");
                    break;
            }

            foreach (string pid in pidList)
            {
                KillProcessBy(pidString: pid);
            }
        }

        public Platform GetOSName()
        {
            string os = Environment.OSVersion.VersionString;
            server.Logger.WriteDebug($"OS Info: {os}");

            if (os != null && os.ToLower().Contains("unix"))
            {
                return Platform.Linux;
            }

            else
            {
                return Platform.Windows;
            }
        }

        public void KillProcessBy(string pidString)
        {
            if (pidString != null && int.TryParse(s: pidString, result: out int pid))
            {
                Process p = Process.GetProcessById(pid);
                p.Kill();
                server.Logger.WriteInfo($"Killed PID {pidString}");
            }

            else
            {
                server.Logger.WriteError($"Process not found for: {pidString}");
            }

        }

        public List<string> FindUnixProcess()
        {
            ProcessStartInfo processStart = new ProcessStartInfo
            {
                FileName = "bash",
                Arguments = "-c lsof -i",

                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = new Process
            {
                StartInfo = processStart
            };

            process.Start();

            string outStr = process.StandardOutput.ReadToEnd();

            process.Close();
            process.Dispose();
            return SplitByLineBreak(outStr);
        }

        public List<string> FindWindowsProcess()
        {
            ProcessStartInfo processStart = new ProcessStartInfo
            {
                FileName = "netstat.exe",
                Arguments = "-aon",

                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = new Process
            {
                StartInfo = processStart
            };

            process.Start();

            string outStr = process.StandardOutput.ReadToEnd();

            process.Close();
            process.Dispose();
            return SplitByLineBreak(outStr);
        }

        public List<string> SplitByLineBreak(string processLines)
        {
            List<string> processList = new List<string>();

            if (processLines != null)
            {
                string[] tokens = processLines.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                processList.AddRange(collection: tokens);
            }

            return processList;
        }

        public List<string> FilterProcessListBy(List<string> processList, string filter)
        {
            if (processList == null)
            {
                return new List<string>();
            }

            if (filter == null)
            {
                return processList;
            }

            return processList.FindAll(i => i != null && i.ToLower().Contains(filter.ToLower()));
        }

        public string GetPidFrom(string pidString, string pattern)
        {
            MatchCollection matches = Regex.Matches(pidString, pattern);

            if (matches != null && matches.Count > 0)
            {
                return matches[0].Groups[1].Value;
            }

            server.Logger.WriteError("MatchCollection is empty or null");
            return "";
        }
    }
}