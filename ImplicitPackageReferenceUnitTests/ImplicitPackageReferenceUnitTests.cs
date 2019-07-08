// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.ImplicitPackageReference;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace ImplicitPackageReferenceUnitTests
{
    [TestClass]
    public class ImplicitPackageReferenceTests
    {
        public TestLogger log;

        [TestInitialize]
        public void Initialize()
        {
            log = new TestLogger();
        }

        [TestMethod]
        [DeploymentItem(@"test.project.assets.json")]
        public void WhenTaskIsGivenSinglePackageTaskShouldEditJsonFileAndReturnTrue()
        {
            var implicitPacker = new ImplicitPackageReferenceBuildTask(log);
            TaskItem implicitDependeny = new Microsoft.Build.Utilities.TaskItem("Microsoft.AspNetCore.Http.Abstractions");
            implicitPacker.AssetsFilePath = "test.project.assets.json";
            implicitPacker.DependenciesToVersionAndPackage = new TaskItem[] { implicitDependeny };

            JObject assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.AspNetCore.Http.Abstractions"]);

            Assert.IsTrue(implicitPacker.Execute());

            assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
            Assert.AreEqual("[2.0.1, )", assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.AspNetCore.Http.Abstractions"]["version"]);
        }

        [TestMethod]
        [DeploymentItem(@"netcoreSample.json")]
        public void WhenTaskIsGivenSinglePackageInNetCoreFrameworkTaskShouldEditJsonFileAndReturnTrue()
        {
            var implicitPacker = new ImplicitPackageReferenceBuildTask(log);
            TaskItem implicitDependeny = new Microsoft.Build.Utilities.TaskItem("Microsoft.NETCore.Platforms");
            implicitPacker.AssetsFilePath = "netcoreSample.json";
            implicitPacker.DependenciesToVersionAndPackage = new TaskItem[] { implicitDependeny };

            JObject assetsFile = JObject.Parse(File.ReadAllText("netcoreSample.json"));
            Assert.IsNull(assetsFile["project"]["frameworks"]["netcoreapp2.1"]["dependencies"]["Microsoft.NETCore.Platforms"]);

            Assert.IsTrue(implicitPacker.Execute());

            assetsFile = JObject.Parse(File.ReadAllText("netcoreSample.json"));
            Assert.AreEqual("[2.1.0, )", assetsFile["project"]["frameworks"]["netcoreapp2.1"]["dependencies"]["Microsoft.NETCore.Platforms"]["version"]);
        }

        [TestMethod]
        [DeploymentItem(@"test.project.assets.json")]
        public void WhenTaskIsGivenMultiplePackagesTaskShouldEditJsonFileAndReturnTrue()
        {
            var implicitPacker = new ImplicitPackageReferenceBuildTask(log);
            TaskItem implicitDependeny = new Microsoft.Build.Utilities.TaskItem("Microsoft.CodeAnalysis.Common");
            TaskItem implicitDependeny2 = new Microsoft.Build.Utilities.TaskItem("Microsoft.AspNetCore.Routing.Abstractions");
            TaskItem implicitDependeny3 = new Microsoft.Build.Utilities.TaskItem("Microsoft.AspNetCore.Routing");

            implicitPacker.AssetsFilePath = "test.project.assets.json";
            implicitPacker.DependenciesToVersionAndPackage = new TaskItem[] { implicitDependeny, implicitDependeny2, implicitDependeny3 };

            JObject assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.CodeAnalysis.Common"]);
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.AspNetCore.Routing"]);
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.AspNetCore.Routing.Abstractions"]);

            Assert.IsTrue(implicitPacker.Execute());

            assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
            Assert.AreEqual("[2.9.0, )", assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.CodeAnalysis.Common"]["version"]);
            Assert.AreEqual("[2.0.1, )", assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.AspNetCore.Routing"]["version"]);
            Assert.AreEqual("[2.0.1, )", assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.AspNetCore.Routing.Abstractions"]["version"]);
        }

        [TestMethod]
        [DeploymentItem(@"test.project.assets.json")]
        public void WhenTaskIsGivenAPackageNotFoundInJsonFileTaskReturnsFalse()
        {
            var implicitPacker = new ImplicitPackageReferenceBuildTask(log);
            TaskItem implicitDependeny = new Microsoft.Build.Utilities.TaskItem("NotAPackage");

            implicitPacker.AssetsFilePath = "test.project.assets.json";
            implicitPacker.DependenciesToVersionAndPackage = new TaskItem[] { implicitDependeny };

            JObject assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["NotAPackage"]);

            Assert.IsFalse(implicitPacker.Execute());

            assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["NotAPackage"]);
        }

        [TestMethod]
        public void WhenJsonFileIsNotPresentTaskReturnsFalse()
        {
            var implicitPacker = new ImplicitPackageReferenceBuildTask(log);
            TaskItem implicitDependeny = new Microsoft.Build.Utilities.TaskItem("Microsoft.AspNetCore.Http.Abstractions");

            implicitPacker.AssetsFilePath = "NotHere.assets.json";
            implicitPacker.DependenciesToVersionAndPackage = new TaskItem[] { implicitDependeny };

            Assert.IsFalse(implicitPacker.Execute());
            Assert.AreEqual("ImplicitPackageReferenceBuildTask failed, could not find: NotHere.assets.json", log.GetError());
        }

        [TestMethod]
        [DeploymentItem(@"WrongFile.xml")]
        public void WhenTaskIsPassedXMLFileTaskReturnsFalse()
        {
            var implicitPacker = new ImplicitPackageReferenceBuildTask(log);
            TaskItem implicitDependeny = new Microsoft.Build.Utilities.TaskItem("Microsoft.AspNetCore.Http.Abstractions");
            implicitPacker.AssetsFilePath = "WrongFile.xml";
            implicitPacker.DependenciesToVersionAndPackage = new TaskItem[] { implicitDependeny };
            Assert.IsFalse(implicitPacker.Execute());
            Assert.AreEqual("ImplicitPackageReferenceBuildTask failed, could not parse file WrongFile.xml. Exception:Unexpected character encountered while parsing value: <. Path '', line 0, position 0.", log.GetError());
        }

        [TestMethod]
        [DeploymentItem(@"MissingLibrary.json")]
        public void WhenJsonFileIsMissingLibrarySectionTaskReturnsFalse()
        {
            var implicitPacker = new ImplicitPackageReferenceBuildTask(log);
            TaskItem implicitDependeny = new Microsoft.Build.Utilities.TaskItem("Microsoft.AspNetCore.Http.Abstractions");
            implicitPacker.AssetsFilePath = "MissingLibrary.json";
            implicitPacker.DependenciesToVersionAndPackage = new TaskItem[] { implicitDependeny };

            JObject assetsFile = JObject.Parse(File.ReadAllText("MissingLibrary.json"));
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.AspNetCore.Http.Abstractions"]);

            Assert.IsFalse(implicitPacker.Execute());
            assetsFile = JObject.Parse(File.ReadAllText("MissingLibrary.json"));
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.AspNetCore.Http.Abstractions"]);
            Assert.AreEqual("ImplicitPackageReferenceBuildTask failed, MissingLibrary.json missing Libraries section.", log.GetError());
        }

        [TestMethod]
        [DeploymentItem("VersionlessPackage.json")]
        public void WhenPackageIsMissingVersionNumberInJsonFileTaskReturnsFalse()
        {
            var implicitPacker = new ImplicitPackageReferenceBuildTask(log);
            TaskItem implicitDependeny = new Microsoft.Build.Utilities.TaskItem("WebApiContrib.Core");
            implicitPacker.AssetsFilePath = "VersionlessPackage.json";
            implicitPacker.DependenciesToVersionAndPackage = new TaskItem[] { implicitDependeny };

            JObject assetsFile = JObject.Parse(File.ReadAllText("VersionlessPackage.json"));
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["VersionlessPackage.json"]);

            Assert.IsFalse(implicitPacker.Execute());

            assetsFile = JObject.Parse(File.ReadAllText("VersionlessPackage.json"));
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["VersionlessPackage.json"]);
            Assert.AreEqual("ImplicitPackageReferenceBuildTask failed, VersionlessPackage.json formatted incorrectly", log.GetError());
        }

        [TestMethod]
        [DeploymentItem(@"test.project.assets.json")]
        public void WhenTaskHasPrivateAssetsSetSupressParentsTrue()
        {
            var implicitPacker = new ImplicitPackageReferenceBuildTask(log);
            TaskItem implicitDependeny = new Microsoft.Build.Utilities.TaskItem("Microsoft.AspNetCore.Http");
            implicitDependeny.SetMetadata("privateAssets", "all");
            implicitPacker.AssetsFilePath = "test.project.assets.json";
            implicitPacker.DependenciesToVersionAndPackage = new TaskItem[] { implicitDependeny };

            JObject assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.AspNetCore.Http"]);

            Assert.IsTrue(implicitPacker.Execute());

            assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
            Assert.AreEqual("[2.0.1, )", assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.AspNetCore.Http"]["version"]);
            Assert.AreEqual("all", assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.AspNetCore.Http"]["suppressParent"]);
        }

        [TestMethod]
        [DeploymentItem(@"test.project.assets.json")]
        public void WhenTaskHasMultiplePrivateAssetsSetSupressParentsTrue()
        {
            var implicitPacker = new ImplicitPackageReferenceBuildTask(log);
            TaskItem implicitDependeny = new Microsoft.Build.Utilities.TaskItem("Microsoft.AspNetCore.Hosting");
            implicitDependeny.SetMetadata("privateAssets", "contentFiles;analyzers");
            implicitPacker.AssetsFilePath = "test.project.assets.json";
            implicitPacker.DependenciesToVersionAndPackage = new TaskItem[] { implicitDependeny };

            JObject assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.AspNetCore.Hosting"]);

            Assert.IsTrue(implicitPacker.Execute());

            assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
            Assert.AreEqual("[2.0.1, )", assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.AspNetCore.Hosting"]["version"]);
            Assert.AreEqual("contentFiles;analyzers", assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.AspNetCore.Hosting"]["suppressParent"]);
        }
    }

}
