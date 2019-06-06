using System;
using System.IO;
using ImplicitPackage;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Moq;

namespace ImplicitPackageReferenceUnitTests
{
    [TestClass]
    public class ImplicitPackageReferenceTests
    {
        private Mock<TaskLoggingHelper> mockLogger;

        [TestInitialize]
        public void Initialize()
        {
            mockLogger = new Mock<TaskLoggingHelper>();
        }

        [TestMethod]
        [DeploymentItem(@"project.assets.json")]
        public void SingleImplicitReferencePass()
        {
            var ImplicitPacker = new ImplicitPackageReference();

            ImplicitPacker.AssetsFilePath = "project.assets.json";
            ImplicitPacker.DependenciesToVersionAndPackage = "Microsoft.AspNetCore.Http.Abstractions";
            
            Assert.IsTrue(ImplicitPacker.Execute());

            JObject AssetsFile = JObject.Parse(File.ReadAllText("project.assets.json"));
            Assert.IsNotNull(AssetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.AspNetCore.Http.Abstractions"]);
        }

        [TestMethod]
        [DeploymentItem(@"project.assets.json")]
        public void SingleImplicitReferenceMultiplePackagePass()
        {
            var ImplicitPacker = new ImplicitPackageReference();
            ImplicitPacker.AssetsFilePath = "project.assets.json";
            ImplicitPacker.DependenciesToVersionAndPackage = "Microsoft.AspNetCore.Http.Abstractions;Microsoft.AspNetCore.Routing.Abstractions;Microsoft.AspNetCore.Routing";

            Assert.IsTrue(ImplicitPacker.Execute());

            JObject AssetsFile = JObject.Parse(File.ReadAllText("project.assets.json"));
            Assert.IsNotNull(AssetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.AspNetCore.Http.Abstractions"]);
            Assert.IsNotNull(AssetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.AspNetCore.Routing"]);
            Assert.IsNotNull(AssetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.AspNetCore.Routing.Abstractions"]);
        }

        [TestMethod]
        [DeploymentItem(@"project.assets.json")]
        public void SingleImplicitReferenceReferenceNotFoundFail()
        {
            var ImplicitPacker = new ImplicitPackageReference();
            
            ImplicitPacker.AssetsFilePath = "project.assets.json";
            ImplicitPacker.DependenciesToVersionAndPackage = "NotAPackage";

            Assert.IsFalse(ImplicitPacker.Execute());
        }

        [TestMethod]
        [DeploymentItem(@"project.assets.json")]
        public void SingleImplicitReferenceNoAssetsFileFail()
        {
            var ImplicitPacker = new ImplicitPackageReference();
            ImplicitPacker.AssetsFilePath = "NotHere.assets.json";
            ImplicitPacker.DependenciesToVersionAndPackage = "Microsoft.AspNetCore.Http.Abstractions";

            Assert.IsFalse(ImplicitPacker.Execute());
        }

        [TestMethod]
        [DeploymentItem(@"WrongFile.xml")]
        public void HandedNonJsonFileFail()
        {
            var ImplicitPacker = new ImplicitPackageReference();
            ImplicitPacker.AssetsFilePath = "WrongFile.xml";
            ImplicitPacker.DependenciesToVersionAndPackage = "Microsoft.AspNetCore.Http.Abstractions";
            
            Assert.IsFalse(ImplicitPacker.Execute());
        }

        [TestMethod]
        [DeploymentItem(@"MissingLibrary.json")]
        public void JsonFileMissingInfoFail()
        {
            var ImplicitPacker = new ImplicitPackageReference();
            ImplicitPacker.AssetsFilePath = "MissingLibrary.json";
            ImplicitPacker.DependenciesToVersionAndPackage = "Microsoft.AspNetCore.Http.Abstractions";

            Assert.IsFalse(ImplicitPacker.Execute());
        }
    }
}
