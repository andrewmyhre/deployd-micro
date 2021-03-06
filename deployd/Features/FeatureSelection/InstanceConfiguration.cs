﻿using System.Collections.Generic;
using NDesk.Options;
using NuGet;
using deployd.Extensibility.Configuration;

namespace deployd.Features.FeatureSelection
{
    public class InstanceConfiguration : IInstanceConfiguration
    {
        public string AppName { get; set; }
        public bool Install { get; set; }
        public bool Prep { get; set; }
        public bool Help { get; set; }
        public bool Verbose { get; set; }
        public object Version { get; set; }
        
        public List<string> ExtraParams { get; set; }
        public OptionSet OptionSet { get; set; }

        public PackageLocation<object> PackageLocation { get; set; }

        public IApplicationMap ApplicationMap { get; set; }
        public string Environment { get; set; }
        public bool ShowState { get; set; }
        public bool Update { get; set; }

        public bool ForceDownload { get; set; }

        public string SetConfigurationValue { get; set; }

        public bool ForceUnpack { get; set; }
        public string InstallPath { get; set; }
        public string PackageSource { get; set; }

        public InstanceConfiguration()
        {
            ApplicationMap = new ApplicationMap(string.Empty, string.Empty);
        }
    }
}