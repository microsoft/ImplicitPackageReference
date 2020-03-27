// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

using System.IO;
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
            var implicitPacker = new AddImplicitPackageReferences(log);
            TaskItem implicitDependency = new Microsoft.Build.Utilities.TaskItem("Microsoft.Spatial");
            implicitPacker.AssetsFilePath = "test.project.assets.json";
            implicitPacker.DependenciesToVersionAndPackage = new TaskItem[] { implicitDependency };

            JObject assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.Spatial"]);

            Assert.IsTrue(implicitPacker.Execute());

            assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
            Assert.AreEqual("[7.6.1, )", assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.Spatial"]["version"]);
        }

        [TestMethod]
        [DeploymentItem(@"netcoreSample.json")]
        public void WhenTaskIsGivenSinglePackageInNetCoreFrameworkTaskShouldEditJsonFileAndReturnTrue()
        {
            var implicitPacker = new AddImplicitPackageReferences(log);
            TaskItem implicitDependency = new Microsoft.Build.Utilities.TaskItem("Microsoft.NETCore.Platforms");
            implicitPacker.AssetsFilePath = "netcoreSample.json";
            implicitPacker.DependenciesToVersionAndPackage = new TaskItem[] { implicitDependency };

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
            var implicitPacker = new AddImplicitPackageReferences(log);
            TaskItem implicitDependency = new Microsoft.Build.Utilities.TaskItem("Microsoft.OData.Edm");
            TaskItem implicitDependency2 = new Microsoft.Build.Utilities.TaskItem("Microsoft.OData.Client");

            implicitPacker.AssetsFilePath = "test.project.assets.json";
            implicitPacker.DependenciesToVersionAndPackage = new TaskItem[] { implicitDependency, implicitDependency2};

            JObject assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.OData.Edm"]);
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.OData.Client"]);

            Assert.IsTrue(implicitPacker.Execute());

            assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
            Assert.AreEqual("[7.6.1, )", assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.OData.Edm"]["version"]);
            Assert.AreEqual("[7.6.1, )", assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.OData.Client"]["version"]);
        }

        [TestMethod]
        [DeploymentItem(@"test.project.assets.json")]
        public void WhenTaskIsGivenAPackageNotFoundInJsonFileTaskReturnsFalse()
        {
            var implicitPacker = new AddImplicitPackageReferences(log);
            TaskItem implicitDependency = new Microsoft.Build.Utilities.TaskItem("NotAPackage");

            implicitPacker.AssetsFilePath = "test.project.assets.json";
            implicitPacker.DependenciesToVersionAndPackage = new TaskItem[] { implicitDependency };

            JObject assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["NotAPackage"]);

            Assert.IsFalse(implicitPacker.Execute());

            assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["NotAPackage"]);
        }

        [TestMethod]
        public void WhenJsonFileIsNotPresentTaskReturnsFalse()
        {
            var implicitPacker = new AddImplicitPackageReferences(log);
            TaskItem implicitDependency = new Microsoft.Build.Utilities.TaskItem("Microsoft.AspNetCore.Http.Abstractions");

            implicitPacker.AssetsFilePath = "NotHere.assets.json";
            implicitPacker.DependenciesToVersionAndPackage = new TaskItem[] { implicitDependency };

            Assert.IsFalse(implicitPacker.Execute());
            Assert.AreEqual("AddImplicitPackageReferences failed, could not find: NotHere.assets.json", log.ErrorMessage);
        }

        [TestMethod]
        [DeploymentItem(@"WrongFile.xml")]
        public void WhenTaskIsPassedXMLFileTaskReturnsFalse()
        {
            var implicitPacker = new AddImplicitPackageReferences(log);
            TaskItem implicitDependency = new Microsoft.Build.Utilities.TaskItem("Microsoft.AspNetCore.Http.Abstractions");
            implicitPacker.AssetsFilePath = "WrongFile.xml";
            implicitPacker.DependenciesToVersionAndPackage = new TaskItem[] { implicitDependency };
            Assert.IsFalse(implicitPacker.Execute());
            Assert.AreEqual("AddImplicitPackageReferences failed, could not parse file WrongFile.xml. Exception:Unexpected character encountered while parsing value: <. Path '', line 0, position 0.", log.ErrorMessage);
        }

        [TestMethod]
        [DeploymentItem(@"MissingLibrary.json")]
        public void WhenJsonFileIsMissingLibrarySectionTaskReturnsFalse()
        {
            var implicitPacker = new AddImplicitPackageReferences(log);
            TaskItem implicitDependency = new Microsoft.Build.Utilities.TaskItem("Microsoft.AspNetCore.Http.Abstractions");
            implicitPacker.AssetsFilePath = "MissingLibrary.json";
            implicitPacker.DependenciesToVersionAndPackage = new TaskItem[] { implicitDependency };

            JObject assetsFile = JObject.Parse(File.ReadAllText("MissingLibrary.json"));
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.AspNetCore.Http.Abstractions"]);

            Assert.IsFalse(implicitPacker.Execute());
            assetsFile = JObject.Parse(File.ReadAllText("MissingLibrary.json"));
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.AspNetCore.Http.Abstractions"]);
            Assert.AreEqual("AddImplicitPackageReferences failed, MissingLibrary.json missing Libraries section.", log.ErrorMessage);
        }

        [TestMethod]
        [DeploymentItem("VersionlessPackage.json")]
        public void WhenPackageIsMissingVersionNumberInJsonFileTaskReturnsFalse()
        {
            var implicitPacker = new AddImplicitPackageReferences(log);
            TaskItem implicitDependency = new Microsoft.Build.Utilities.TaskItem("WebApiContrib.Core");
            implicitPacker.AssetsFilePath = "VersionlessPackage.json";
            implicitPacker.DependenciesToVersionAndPackage = new TaskItem[] { implicitDependency };

            JObject assetsFile = JObject.Parse(File.ReadAllText("VersionlessPackage.json"));
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["VersionlessPackage.json"]);

            Assert.IsFalse(implicitPacker.Execute());

            assetsFile = JObject.Parse(File.ReadAllText("VersionlessPackage.json"));
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["VersionlessPackage.json"]);
            Assert.AreEqual("AddImplicitPackageReferences failed, VersionlessPackage.json formatted incorrectly", log.ErrorMessage);
        }

        [TestMethod]
        [DeploymentItem(@"test.project.assets.json")]
        public void WhenTaskHasPrivateAssetsSetSuppressParentsTrue()
        {
            var implicitPacker = new AddImplicitPackageReferences(log);
            TaskItem implicitDependency = new Microsoft.Build.Utilities.TaskItem("Microsoft.OData.Core");
            implicitDependency.SetMetadata("privateAssets", "all");
            implicitPacker.AssetsFilePath = "test.project.assets.json";
            implicitPacker.DependenciesToVersionAndPackage = new TaskItem[] { implicitDependency };

            JObject assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.OData.Core"]);

            Assert.IsTrue(implicitPacker.Execute());

            assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
            Assert.AreEqual("[7.6.1, )", assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.OData.Core"]["version"]);
            Assert.AreEqual("all", assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.OData.Core"]["suppressParent"]);
        }

        [TestMethod]
        [DeploymentItem(@"test.project.assets.json")]
        public void WhenTaskHasMultiplePrivateAssetsSetSuppressParentsTrue()
        {
            var implicitPacker = new AddImplicitPackageReferences(log);
            TaskItem implicitDependency = new Microsoft.Build.Utilities.TaskItem("Microsoft.NETCore.Platforms");
            implicitDependency.SetMetadata("privateAssets", "contentFiles;analyzers");
            implicitPacker.AssetsFilePath = "test.project.assets.json";
            implicitPacker.DependenciesToVersionAndPackage = new TaskItem[] { implicitDependency };

            JObject assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.NETCore.Platforms"]);

            Assert.IsTrue(implicitPacker.Execute());

            assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
            Assert.AreEqual("[1.1.0, )", assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.NETCore.Platforms"]["version"]);
            Assert.AreEqual("contentFiles;analyzers", assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.NETCore.Platforms"]["suppressParent"]);
        }
    }

}
