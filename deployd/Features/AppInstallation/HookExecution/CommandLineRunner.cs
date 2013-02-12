﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using deployd.Extensibility.Configuration;
using deployd.Features.AppInstallation.Hooks;
using log4net;

namespace deployd.Features.AppInstallation.HookExecution
{
    public class CommandLineRunner : IHookRunner
    {
        private readonly ILog _log;
        private readonly IInstanceConfiguration _config;

        private static readonly Dictionary<string, string> ExecutableMap = new Dictionary<string, string>
            {
                {"ps1", "powershell"},
                {"rb", "ruby"},
                {"py", "python"},
                {"cgi", "perl"},
                {"php", "php"},
                {"js", "node"},
            };

        public CommandLineRunner(ILog log, IInstanceConfiguration config)
        {
            _log = log;
            _config = config;
        }

        public bool SupportsHook(Hook hook)
        {
            return hook.Type == HookType.File;
        }

        public void ExecuteHook(Hook hook)
        {
            _log.Info("Executing package hookFileName: " + hook.FileName);

            var hookFilename = hook.FileName;
            var startInfo = new ProcessStartInfo
                {
                    FileName = hookFilename,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                };

            CopyVariablesToEnvironment(startInfo, _config);
            PrefixCommonScriptRuntimes(hookFilename, startInfo);
            StartProcess(hookFilename, startInfo);
        }

        private void StartProcess(string hookFileName, ProcessStartInfo startInfo)
        {
            var process = Process.Start(startInfo);

            using (var outputStream = process.StandardOutput)
            {
                process.WaitForExit();
                _log.Info(outputStream.ReadToEnd());
            }

            VerifyProcessExitCode(hookFileName, process);
        }

        public static void VerifyProcessExitCode(string hookFileName, Process process)
        {
            if (process.ExitCode != 0)
            {
                throw new HookFailureException(hookFileName, process.ExitCode);
            }
        }

        public static void PrefixCommonScriptRuntimes(string hookFileName, ProcessStartInfo startInfo)
        {
            foreach (var extension in ExecutableMap.Where(ext => hookFileName.EndsWith("." + ext.Key)))
            {
                startInfo.FileName = extension.Value + " " + startInfo.FileName;
            }
        }

        public static void CopyVariablesToEnvironment(ProcessStartInfo startInfo, IInstanceConfiguration config)
        {
            var envrs = config.ApplicationMap.GetType().GetProperties()
                              .Select(fi => new {Field = fi.Name, Value = fi.GetValue(config.ApplicationMap)})
                              .ToList();

            foreach (var variable in envrs)
            {
                startInfo.EnvironmentVariables.Add("Deployd." + variable.Field, variable.Value.ToString());
            }
        }
    }
}