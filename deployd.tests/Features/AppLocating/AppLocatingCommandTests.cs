﻿using System.Collections.Generic;
using System.IO;
using System.Text;
using Moq;
using NUnit.Framework;
using deployd.Extensibility.Configuration;
using deployd.Features.AppLocating;
using deployd.Features.FeatureSelection;
using log4net;

namespace deployd.tests.Features.AppLocating
{
    [TestFixture]
    public class AppLocatingCommandTests
    {
        private List<IAppInstallationLocator> _finders;
        private AppLocatingCommand _cmd;
        private Mock<ILog> _logger;
        private IInstanceConfiguration _instanceConfig;
        private TextWriter _output = new StringWriter(new StringBuilder());

        [SetUp]
        public void SetUp()
        {
            _logger = new Mock<ILog>();
            _finders = new List<IAppInstallationLocator>();
            _instanceConfig = new InstanceConfiguration {AppName = "MyApp"};
            _cmd = new AppLocatingCommand(_finders, _logger.Object, _instanceConfig, _output);
        }

        [Test]
        public void Invoked_ChecksAllApplicationLocatorsToSeeIfAnySupportConfiguredPathType()
        {
            var mockFinder = new Mock<IAppInstallationLocator>();
            mockFinder.Setup(x => x.SupportsPathType()).Returns(false);
            var mockFinder2 = new Mock<IAppInstallationLocator>();
            mockFinder2.Setup(x => x.SupportsPathType()).Returns(false);
            _finders.Add(mockFinder.Object);
            _finders.Add(mockFinder2.Object);

            _cmd.Execute();

            mockFinder.Verify(x=>x.SupportsPathType());
            mockFinder2.Verify(x=>x.SupportsPathType());
        }

        [Test]
        public void Invoked_WithAnActiveApplicationLocator_LocatorAskedToFindPackageByName()
        {
            var mockFinder = new Mock<IAppInstallationLocator>();
            _finders.Add(mockFinder.Object);
            mockFinder.Setup(x => x.SupportsPathType()).Returns(true);

            _cmd.Execute();

            mockFinder.Verify(x => x.CanFindPackageAsObject(_instanceConfig.AppName,null));
        }

        [Test]
        public void Invoked_LocatorFindsPackage_PackageLocationSetInConfigurationForUseBySubsequentCommands()
        {
            var package = new PackageLocation<object>();
            var mockFinder = new Mock<IAppInstallationLocator>();
            _finders.Add(mockFinder.Object);

            mockFinder.Setup(x => x.SupportsPathType()).Returns(true);
            mockFinder.Setup(x => x.CanFindPackageAsObject(_instanceConfig.AppName, null)).Returns(package);

            _cmd.Execute();

            Assert.That(_instanceConfig.PackageLocation, Is.EqualTo(package));
        }

        [Test]
        public void Invoked_LocatorFindsPackage_LogMessageWritten()
        {
            var package = new PackageLocation<object>();
            var mockFinder = new Mock<IAppInstallationLocator>();
            _finders.Add(mockFinder.Object);

            mockFinder.Setup(x => x.SupportsPathType()).Returns(true);
            mockFinder.Setup(x => x.CanFindPackageAsObject(_instanceConfig.AppName, null)).Returns(package);

            _cmd.Execute();

            Assert.That(_output.ToString(), Is.StringContaining("Found " + package.PackageDetails));
        }
        
        [Test]
        public void Invoked_LocatorFindsNothing_PackageLocationNotSet()
        {
            var mockFinder = new Mock<IAppInstallationLocator>();
            _finders.Add(mockFinder.Object);

            mockFinder.Setup(x => x.SupportsPathType()).Returns(true);
            mockFinder.Setup(x => x.CanFindPackageAsObject(_instanceConfig.AppName, null))
                      .Returns((PackageLocation<object>) null);

            _cmd.Execute();

            Assert.That(_instanceConfig.PackageLocation, Is.Null);
        }
    }
}
