﻿using Moq;
using NUnit.Framework;
using NuGet;
using deployd.Extensibility.Configuration;
using deployd.Features.AppLocating;
using IFileSystem = System.IO.Abstractions.IFileSystem;

namespace deployd.tests.Features.AppLocating
{
    [TestFixture]
    public class NuGetFeedAppInstallationLocatorTests
    {
        private Mock<IFileSystem> _fs;
        private Mock<IGetLatestNuGetPackageByNameQuery> _packageQuery;
        private Mock<IGetNuGetPackageByNameAndVersionQuery> _packageByVersionQuery;
        private Mock<IPackageSourceConfiguration> _packageSourceConfiguration = new Mock<IPackageSourceConfiguration>();

        [SetUp]
        public void SetUp()
        {
            _fs = new Mock<IFileSystem>();
            _packageQuery = new Mock<IGetLatestNuGetPackageByNameQuery>();
            _packageByVersionQuery = new Mock<IGetNuGetPackageByNameAndVersionQuery>();
        }

        [Test]
        public void WhenPackageSourceIsAUri_IsHttpIsTrue()
        {
            _packageSourceConfiguration.Setup(x => x.PackageSource).Returns("http://tempuri.org/blah");

            var locator = new NuGetFeedAppInstallationLocator(_packageSourceConfiguration.Object, _fs.Object, _packageQuery.Object, _packageByVersionQuery.Object);
            
            Assert.That(locator.IsHttp, Is.True);
        }

        [Test]
        public void WhenPackageSourceIsADirectoryPath_IsHttpIsFalse()
        {
            _packageSourceConfiguration.Setup(x => x.PackageSource).Returns("c:\\myrepo");

            var locator = new NuGetFeedAppInstallationLocator(_packageSourceConfiguration.Object, _fs.Object, _packageQuery.Object, _packageByVersionQuery.Object);
            
            Assert.That(locator.IsHttp, Is.False);
        }

        [Test]
        public void SupportsPathType_WhenPackageSourceIsHttp_ReturnsTrue()
        {
            _packageSourceConfiguration.Setup(x => x.PackageSource).Returns("http://tempuri.org/blah");

            var locator = new NuGetFeedAppInstallationLocator(_packageSourceConfiguration.Object, _fs.Object, _packageQuery.Object, _packageByVersionQuery.Object);

            Assert.That(locator.SupportsPathType(), Is.True);
        }

        [Test]
        public void SupportsPathType_WhenPackageSourceIsDirectoryAndDirectoryExists_ReturnsTrue()
        {
            _packageSourceConfiguration.Setup(x => x.PackageSource).Returns("c:\\myrepo");

            _fs.Setup(x => x.Directory.Exists(_packageSourceConfiguration.Object.PackageSource)).Returns(true);

            var locator = new NuGetFeedAppInstallationLocator(_packageSourceConfiguration.Object, _fs.Object, _packageQuery.Object, _packageByVersionQuery.Object);

            Assert.That(locator.SupportsPathType(), Is.True);
        }

        [Test]
        public void SupportsPathType_WhenPackageSourceIsDirectoryAndDirectoryDoesntExists_ReturnsFalse()
        {
            _packageSourceConfiguration.Setup(x => x.PackageSource).Returns("c:\\myrepo");

            _fs.Setup(x => x.Directory.Exists(_packageSourceConfiguration.Object.PackageSource)).Returns(false);

            var locator = new NuGetFeedAppInstallationLocator(_packageSourceConfiguration.Object, _fs.Object, _packageQuery.Object, _packageByVersionQuery.Object);

            Assert.That(locator.SupportsPathType(), Is.False);
        }

        [Test]
        public void CanFindPackage_WhenPackageIsFound_PackageIsReturnedInWrapper()
        {
            _fs.Setup(x => x.Directory.Exists(It.IsAny<string>())).Returns(true);
            var package = MockPackage("1.0.0.0");
            _packageQuery.Setup(x => x.GetLatestVersionOf(It.IsAny<string>(), _packageSourceConfiguration.Object.PackageSource))
                         .Returns(package.Object);

            var locator = new NuGetFeedAppInstallationLocator(_packageSourceConfiguration.Object, _fs.Object, _packageQuery.Object, _packageByVersionQuery.Object);

            var returnedPackage = locator.CanFindPackage("appName");

            Assert.That(returnedPackage.PackageDetails, Is.EqualTo(package.Object));
            Assert.That(returnedPackage.PackageVersion, Is.EqualTo("1.0.0.0"));
        }

        [Test]
        public void CanFindPackageAsObject_WhenPackageIsFound_PackageIsReturnedInWrapper()
        {
            _fs.Setup(x => x.Directory.Exists(It.IsAny<string>())).Returns(true);
            var package = MockPackage("1.0.0.0");
            _packageQuery.Setup(x => x.GetLatestVersionOf(It.IsAny<string>(), _packageSourceConfiguration.Object.PackageSource))
                         .Returns(package.Object);

            var locator = new NuGetFeedAppInstallationLocator(_packageSourceConfiguration.Object, _fs.Object, _packageQuery.Object, _packageByVersionQuery.Object);

            var returnedPackage = locator.CanFindPackageAsObject("appName");

            Assert.That(returnedPackage.PackageDetails, Is.EqualTo(package.Object));
            Assert.That(returnedPackage.PackageVersion, Is.EqualTo("1.0.0.0"));
        }

        private static Mock<IPackage> MockPackage(string version)
        {
            var v = SemanticVersion.Parse("1.0.0.0");
            var p = new Mock<IPackage>();
            p.Setup(x => x.Version).Returns(v);
            return p;
        }
    }
}