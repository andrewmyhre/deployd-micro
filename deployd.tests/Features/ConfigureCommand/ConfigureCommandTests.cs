﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using deployd.Extensibility.Configuration;
using deployd.Features.FeatureSelection;

namespace deployd.tests.Features.ConfigureCommand
{
    [TestFixture]
    [Ignore("Mock file system makes this very difficult to test")]
    public class ConfigureCommandTests
    {
        [TestCase("PackageType=nuget")]
        [TestCase("PackageSource=http://some/nuget/feed/url")]
        [TestCase("InstallRoot=c:\\some\\drive\\path")]
        public void CanSetPackageTypeOptionFromAString(string configCommand)
        {
            var configFileStream = new MemoryStream();
            var fileSystem = new Mock<IFileSystem>();
            IDictionary<string, MockFileData> fileData=new Dictionary<string, MockFileData>()
                {
                    {"c:\\config.json", new MockFileData("{\"PackageType\":0,\"PackageSource\":\"http://192.168.20.25:1337/nuget/justgivingapplications\",\"InstallRoot\":\"d:\\wwwcom\"}")}
                };
            fileSystem.SetupGet(x => x.Path).Returns(new MockPath(new MockFileSystem(fileData)));
            fileSystem.SetupGet(x => x.File).Returns(new MockFile(new MockFileSystem(fileData)));
            var appFolderLocator = new Mock<IApplicationFolderLocator>();
            appFolderLocator.SetupGet(x=>x.ApplicationFolder).Returns("c:\\");
            fileSystem.Setup(x => x.File.Open(It.IsAny<string>(), FileMode.Create, FileAccess.Write)).Returns(configFileStream);

            IInstanceConfiguration instanceConfiguration=new InstanceConfiguration()
                {
                    SetConfigurationValue = configCommand
                };

            var deploydConfiguration=new DeploydConfiguration();
            var configurationManager = new DeploydConfigurationManager(fileSystem.Object, appFolderLocator.Object);
            TextWriter output = new StringWriter(new StringBuilder());
            var command = new deployd.Features.AppConfiguration.ConfigureCommand(instanceConfiguration,
                                                                                 configurationManager, output);
            command.Execute();

            Assert.That(deploydConfiguration.PackageType, Is.EqualTo(PackageType.NuGet));
        }
    }
}
