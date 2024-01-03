// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using Microsoft.Build.ImplicitPackageReference;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace ImplicitPackageReferenceUnitTests
{
    [TestClass]
    [DoNotParallelize]
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
            TaskItem implicitDependency = new TaskItem("Microsoft.Extensions.DependencyInjection");
            implicitPacker.AssetsFilePath = "test.project.assets.json";
            implicitPacker.ImplicitPackageReferences = new TaskItem[] { implicitDependency };

            JObject assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.Extensions.DependencyInjection"]);
            Assert.IsNull(assetsFile["project"]["frameworks"]["net6.0"]["dependencies"]["Microsoft.Extensions.DependencyInjection"]);

            Assert.IsTrue(implicitPacker.Execute());

            assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
            Assert.AreEqual("[8.0.0, )", assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.Extensions.DependencyInjection"]["version"]);
            Assert.AreEqual("[8.0.0, )", assetsFile["project"]["frameworks"]["net6.0"]["dependencies"]["Microsoft.Extensions.DependencyInjection"]["version"]);
        }

        [TestMethod]
        [DeploymentItem(@"test.project.assets.json")]
        public void WhenTaskIsGivenMultiplePackagesTaskShouldEditJsonFileAndReturnTrue()
        {
            var implicitPacker = new AddImplicitPackageReferences(log);
            TaskItem implicitDependency = new TaskItem("Microsoft.Extensions.DependencyInjection");
            TaskItem implicitDependency2 = new TaskItem("Microsoft.Extensions.Logging.Abstractions");
            TaskItem implicitDependency3 = new TaskItem("System.Runtime.CompilerServices.Unsafe");

            implicitPacker.AssetsFilePath = "test.project.assets.json";
            implicitPacker.ImplicitPackageReferences = new TaskItem[] { implicitDependency, implicitDependency2, implicitDependency3 };

            JObject assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.Extensions.DependencyInjection"]);
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.Extensions.Logging.Abstractions"]);
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["System.Runtime.CompilerServices.Unsafe"]);
            Assert.IsNull(assetsFile["project"]["frameworks"]["net6.0"]["dependencies"]["Microsoft.Extensions.DependencyInjection"]);
            Assert.IsNull(assetsFile["project"]["frameworks"]["net6.0"]["dependencies"]["Microsoft.Extensions.Logging.Abstractions"]);
            Assert.IsNull(assetsFile["project"]["frameworks"]["net6.0"]["dependencies"]["System.Runtime.CompilerServices.Unsafe"]);

            Assert.IsTrue(implicitPacker.Execute());

            assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
            Assert.AreEqual("[8.0.0, )", assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.Extensions.DependencyInjection"]["version"]);
            Assert.AreEqual("[8.0.0, )", assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.Extensions.Logging.Abstractions"]["version"]);
            Assert.AreEqual("[6.0.0, )", assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["System.Runtime.CompilerServices.Unsafe"]["version"]);
            Assert.AreEqual("[8.0.0, )", assetsFile["project"]["frameworks"]["net6.0"]["dependencies"]["Microsoft.Extensions.DependencyInjection"]["version"]);
            Assert.AreEqual("[8.0.0, )", assetsFile["project"]["frameworks"]["net6.0"]["dependencies"]["Microsoft.Extensions.Logging.Abstractions"]["version"]);
            Assert.AreEqual("[6.0.0, )", assetsFile["project"]["frameworks"]["net6.0"]["dependencies"]["System.Runtime.CompilerServices.Unsafe"]["version"]);
        }


        [TestMethod]
        [DeploymentItem(@"test.project.assets.json")]
        public void WhenATargetFrameworkIsDefinedItAppliesOnlyToThatTargetFramework()
        {
            var implicitPacker = new AddImplicitPackageReferences(log);
            TaskItem implicitDependency = new TaskItem("Microsoft.Extensions.DependencyInjection");
            implicitDependency.SetMetadata("TargetFramework", "net6.0");
            TaskItem implicitDependency2 = new TaskItem("Microsoft.Extensions.Logging.Abstractions");
            implicitDependency2.SetMetadata("TargetFramework", "net6.0");
            TaskItem implicitDependency3 = new TaskItem("Microsoft.Extensions.Logging.Abstractions");
            implicitDependency3.SetMetadata("TargetFramework", "netstandard2.0");
            implicitPacker.AssetsFilePath = "test.project.assets.json";
            implicitPacker.ImplicitPackageReferences = new TaskItem[] { implicitDependency, implicitDependency2, implicitDependency3 };

            JObject assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.Extensions.DependencyInjection"]);
            Assert.IsNull(assetsFile["project"]["frameworks"]["net6.0"]["dependencies"]["Microsoft.Extensions.DependencyInjection"]);
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.Extensions.Logging.Abstractions"]);
            Assert.IsNull(assetsFile["project"]["frameworks"]["net6.0"]["dependencies"]["Microsoft.Extensions.Logging.Abstractions"]);

            Assert.IsTrue(implicitPacker.Execute());

            assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.Extensions.DependencyInjection"]);
            Assert.AreEqual("[8.0.0, )", assetsFile["project"]["frameworks"]["net6.0"]["dependencies"]["Microsoft.Extensions.DependencyInjection"]["version"]);

            Assert.AreEqual("[8.0.0, )", assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.Extensions.Logging.Abstractions"]["version"]);
            Assert.AreEqual("[8.0.0, )", assetsFile["project"]["frameworks"]["net6.0"]["dependencies"]["Microsoft.Extensions.Logging.Abstractions"]["version"]);
        }

        [TestMethod]
        [DeploymentItem(@"test.project.assets.json")]
        public void WhenTaskIsGivenAPackageNotFoundInJsonFileTaskReturnsFalse()
        {
            var implicitPacker = new AddImplicitPackageReferences(log);
            TaskItem implicitDependency = new TaskItem("NotAPackage");

            implicitPacker.AssetsFilePath = "test.project.assets.json";
            implicitPacker.ImplicitPackageReferences = new TaskItem[] { implicitDependency };

            JObject assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["NotAPackage"]);
            Assert.IsNull(assetsFile["project"]["frameworks"]["net6.0"]["dependencies"]["NotAPackage"]);

            Assert.IsFalse(implicitPacker.Execute());

            assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["NotAPackage"]);
            Assert.IsNull(assetsFile["project"]["frameworks"]["net6.0"]["dependencies"]["NotAPackage"]);
        }

        [TestMethod]
        public void WhenJsonFileIsNotPresentTaskReturnsFalse()
        {
            var implicitPacker = new AddImplicitPackageReferences(log);
            TaskItem implicitDependency = new TaskItem("Microsoft.AspNetCore.Http.Abstractions");

            implicitPacker.AssetsFilePath = "NotHere.assets.json";
            implicitPacker.ImplicitPackageReferences = new TaskItem[] { implicitDependency };

            Assert.IsFalse(implicitPacker.Execute());
            Assert.AreEqual("Project.assets.json path does not exist at: NotHere.assets.json", log.ErrorMessage);
        }

        [TestMethod]
        [DeploymentItem(@"WrongFile.xml")]
        public void WhenTaskIsPassedXMLFileTaskReturnsFalse()
        {
            var implicitPacker = new AddImplicitPackageReferences(log);
            TaskItem implicitDependency = new TaskItem("Microsoft.AspNetCore.Http.Abstractions");
            implicitPacker.AssetsFilePath = "WrongFile.xml";
            implicitPacker.ImplicitPackageReferences = new TaskItem[] { implicitDependency };
            Assert.IsFalse(implicitPacker.Execute());
            Assert.AreEqual("AddImplicitPackageReferences failed, could not parse file WrongFile.xml. Exception:Unexpected character encountered while parsing value: <. Path '', line 0, position 0.", log.ErrorMessage);
        }

        [TestMethod]
        [DeploymentItem(@"test.project.assets.json")]
        public void WhenTaskHasPrivateAssetsSetSuppressParentsTrue()
        {
            var implicitPacker = new AddImplicitPackageReferences(log);
            TaskItem implicitDependency = new TaskItem("Microsoft.Extensions.DependencyInjection");
            implicitDependency.SetMetadata("privateAssets", "all");
            implicitPacker.AssetsFilePath = "test.project.assets.json";
            implicitPacker.ImplicitPackageReferences = new TaskItem[] { implicitDependency };

            JObject assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.Extensions.DependencyInjection"]);
            Assert.IsNull(assetsFile["project"]["frameworks"]["net6.0"]["dependencies"]["Microsoft.Extensions.DependencyInjection"]);

            Assert.IsTrue(implicitPacker.Execute());

            assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
            Assert.AreEqual("[8.0.0, )", assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.Extensions.DependencyInjection"]["version"]);
            Assert.AreEqual("All", assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.Extensions.DependencyInjection"]["suppressParent"]);
            Assert.AreEqual("[8.0.0, )", assetsFile["project"]["frameworks"]["net6.0"]["dependencies"]["Microsoft.Extensions.DependencyInjection"]["version"]);
            Assert.AreEqual("All", assetsFile["project"]["frameworks"]["net6.0"]["dependencies"]["Microsoft.Extensions.DependencyInjection"]["suppressParent"]);
        }

        [TestMethod]
        [DeploymentItem(@"test.project.assets.json")]
        public void WhenTaskHasMultiplePrivateAssetsSetSuppressParentsTrue()
        {
            var implicitPacker = new AddImplicitPackageReferences(log);
            TaskItem implicitDependency = new TaskItem("Microsoft.Extensions.DependencyInjection");
            implicitDependency.SetMetadata("privateAssets", "contentFiles;analyzers");
            implicitPacker.AssetsFilePath = "test.project.assets.json";
            implicitPacker.ImplicitPackageReferences = new TaskItem[] { implicitDependency };

            JObject assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
            Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.Extensions.DependencyInjection"]);
            Assert.IsNull(assetsFile["project"]["frameworks"]["net6.0"]["dependencies"]["Microsoft.Extensions.DependencyInjection"]);

            Assert.IsTrue(implicitPacker.Execute());

            assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
            Assert.AreEqual("[8.0.0, )", assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.Extensions.DependencyInjection"]["version"]);
            Assert.AreEqual("ContentFiles, Analyzers", assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.Extensions.DependencyInjection"]["suppressParent"]);
            Assert.AreEqual("[8.0.0, )", assetsFile["project"]["frameworks"]["net6.0"]["dependencies"]["Microsoft.Extensions.DependencyInjection"]["version"]);
            Assert.AreEqual("ContentFiles, Analyzers", assetsFile["project"]["frameworks"]["net6.0"]["dependencies"]["Microsoft.Extensions.DependencyInjection"]["suppressParent"]);
        }

        [TestMethod]
        [DeploymentItem(@"test.project.assets.json")]
        public void WhenTaskHasIncludeAssetsSetAssetsAreIncluded()
        {
            // Include, Exclude, expected
            List<(string, string, string)> data = new List<(string, string, string)>()
            {
                ("all", null, "All"),
                ("none", null, "None"),
                ("compile; runtime", null, "Compile, Runtime"),
                (null, "all", "None"),
                (null, "none", "All"),
                (null, "compile; runtime", "ContentFiles, Build, Native, Analyzers"),
                ("all", "all", "None"),
                ("none", "none", "None"),
                ("all", "none", "All"),
                ("none", "all", "None"),
                ("all", "runtime", "Compile, ContentFiles, Build, Native, Analyzers"),
                ("compile; runtime", "runtime", "Compile"),
                ("compile; runtime", "all", "None"),
                ("compile; runtime", "none", "Compile, Runtime"),
            };
            var oldFile = File.ReadAllText("test.project.assets.json");
            foreach (var datum in data)
            {
                (string included, string excluded, string expected) = datum;

                var implicitPacker = new AddImplicitPackageReferences(log);
                TaskItem implicitDependency = new TaskItem("Microsoft.Extensions.DependencyInjection");
                if (included != null)
                {
                    implicitDependency.SetMetadata("includeAssets", included);
                }
                if (excluded != null)
                {
                    implicitDependency.SetMetadata("excludeAssets", excluded);
                }

                implicitPacker.AssetsFilePath = "test.project.assets.json";
                implicitPacker.ImplicitPackageReferences = new TaskItem[] { implicitDependency };

                File.WriteAllText("test.project.assets.json", oldFile);
                JObject assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
                Assert.IsNull(assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.Extensions.DependencyInjection"]);
                Assert.IsNull(assetsFile["project"]["frameworks"]["net6.0"]["dependencies"]["Microsoft.Extensions.DependencyInjection"]);

                Assert.IsTrue(implicitPacker.Execute());

                assetsFile = JObject.Parse(File.ReadAllText("test.project.assets.json"));
                Assert.AreEqual("[8.0.0, )", assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.Extensions.DependencyInjection"]["version"]);
                Assert.AreEqual(expected, assetsFile["project"]["frameworks"]["netstandard2.0"]["dependencies"]["Microsoft.Extensions.DependencyInjection"]["include"]);
                Assert.AreEqual("[8.0.0, )", assetsFile["project"]["frameworks"]["net6.0"]["dependencies"]["Microsoft.Extensions.DependencyInjection"]["version"]);
                Assert.AreEqual(expected, assetsFile["project"]["frameworks"]["net6.0"]["dependencies"]["Microsoft.Extensions.DependencyInjection"]["include"]);
            }
        }
    }
}
