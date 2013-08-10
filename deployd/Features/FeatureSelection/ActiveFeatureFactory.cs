﻿using System;
using deployd.AppStart;
using deployd.Extensibility.Configuration;
using deployd.Features.AppConfiguration;
using deployd.Features.AppExtraction;
using deployd.Features.AppInstallation;
using deployd.Features.AppLocating;
using deployd.Features.Help;
using deployd.Features.PurgeOldBackups;
using deployd.Features.ShowState;
using deployd.Features.Update;
using log4net;
using Ninject;

namespace deployd.Features.FeatureSelection
{
    public class ActiveFeatureFactory
    {
        private readonly IKernel _kernel;
        private readonly IInstanceConfiguration _instanceConfiguration;
        private readonly ILog _log;
        private readonly ILoggingConfiguration _loggingConfiguration;

        public ActiveFeatureFactory(IKernel kernel, IInstanceConfiguration instanceConfiguration, ILog log, ILoggingConfiguration loggingConfiguration)
        {
            _kernel = kernel;
            _instanceConfiguration = instanceConfiguration;
            _log = log;
            _loggingConfiguration = loggingConfiguration;
        }

        public CommandCollection BuildCommands()
        {
            if (_instanceConfiguration.Verbose)
            {
                _loggingConfiguration.SetLogLevelToDebug();
            }

            var commandCollection = _kernel.GetService<CommandCollection>();

            if (_instanceConfiguration.ShowState)
            {
                commandCollection.Add(_kernel.GetService<ShowStateCommand>());
                return commandCollection;
            }

            if (!string.IsNullOrWhiteSpace(_instanceConfiguration.SetConfigurationValue))
            {
                commandCollection.Add(_kernel.GetService<ConfigureCommand>());
                return commandCollection;
            }

            if (_instanceConfiguration.Update)
            {
                if (string.IsNullOrEmpty(_instanceConfiguration.Environment))
                {
                    commandCollection.Add(_kernel.GetService<HelpCommand>());
                }
                else
                {
                    commandCollection.Add(_kernel.GetService<UpdateCommand>());
                }
                return commandCollection;
            }

            if (_instanceConfiguration.Help
                || string.IsNullOrWhiteSpace(_instanceConfiguration.AppName))
            {
                commandCollection.Add(_kernel.GetService<HelpCommand>());
                return commandCollection;
            }

            if (!_instanceConfiguration.Install && !_instanceConfiguration.Prep)
            {
                if (string.IsNullOrEmpty(_instanceConfiguration.Environment))
                {
                    commandCollection.Add(_kernel.GetService<HelpCommand>());
                }
                else
                {
                    commandCollection.Add(_kernel.GetService<HelpCommand>());
                }
                return commandCollection;
            }


            if (_instanceConfiguration.Install || _instanceConfiguration.Prep)
            {
                commandCollection.Add(_kernel.GetService<AppLocatingCommand>());
                commandCollection.Add(_kernel.GetService<AppExtractionCommand>());

                if (_instanceConfiguration.Install)
                {
                    commandCollection.Add(_kernel.GetService<SetEnvironmentCommand>());
                    commandCollection.Add(_kernel.GetService<AppInstallationCommand>());
                }

                commandCollection.Add(_kernel.GetService<PurgeOldBackupsCommand>());
            }
            return commandCollection;
        }
    }
}
