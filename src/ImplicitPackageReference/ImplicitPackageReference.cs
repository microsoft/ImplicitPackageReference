// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Build.ImplicitPackageReference
{
    public class ImplicitPackageReferenceBuildTask : Task
    {
        /// <summary>
        /// This msbuild task is designed to parse a project.assets.json file and be able to take a string of dependiencies that need versions and be able to find the correct version
        /// inside the project.assets.json file.
        /// </summary>

        /// <AssetsFilePath>This is a path to the project.assets.json from which the implicit versions for dependencies will be found</AssetsFilePath>
        [Required]
        public string AssetsFilePath { get; set; }
        /// <DependenciesToVersionAndPackage>This is a string of packages that are ; separated that need versions</DependenciesToVersionAndPackage>
        [Required]
        public ITaskItem[] DependenciesToVersionAndPackage { get; set; }

        private bool versionlessPackagesFound = false;

        private string namesOfVersionlessPackages = "";

        private Logger log { get; set; }

        public ImplicitPackageReferenceBuildTask() : base()
        {
            log = new StandardLogger(Log);
        }

        public ImplicitPackageReferenceBuildTask(Logger log)
        {
            this.log = log;
        }

        public override bool Execute()
        {
            if (File.Exists(AssetsFilePath))
            {
                JObject AssetsFile;
                try
                {
                    AssetsFile = JObject.Parse(File.ReadAllText(AssetsFilePath));
                }catch(Exception e)
                {
                    log.LogError("ImplicitPackageReferenceBuildTask failed, could not parse file " + AssetsFilePath + ". Exception:" + e.Message);
                    return false;
                }
                
                foreach (var package in DependenciesToVersionAndPackage)
                {
                    bool found = false;
                    if(AssetsFile["libraries"] == null)
                    {
                        log.LogError("ImplicitPackageReferenceBuildTask failed, " +AssetsFilePath + " missing Libraries section.");
                        return false;
                    }
                    
                    foreach (var library in AssetsFile["libraries"].Children<JProperty>())
                    {
                        //Name is index [0], Version is index [1]
                        string[] nameAndVersion = library.Name.Split('/');
                        if(nameAndVersion.Length != 2)
                        {
                            log.LogError("ImplicitPackageReferenceBuildTask failed, " + AssetsFilePath + " formatted incorrectly");
                            return false;
                        }

                        if (nameAndVersion[0] == package.ItemSpec)
                        {
                            JObject versionedDependency = new JObject();
                            versionedDependency.Add("target", "Package");
                            if (package.GetMetadata("PrivateAssets") == "")
                            {
                                versionedDependency.Add("version", "[" + nameAndVersion[1] + ", )");
                                versionedDependency.Add("suppressParent", "none");
                            }
                            else
                            {
                                versionedDependency.Add("suppressParent", package.GetMetadata("PrivateAssets"));
                                versionedDependency.Add("version", "[" + nameAndVersion[1] + ", )");
                            }
                            
                            foreach (var framework in AssetsFile["project"]["frameworks"].Children<JProperty>())
                            {
                                if (framework.Value["dependencies"][package.ItemSpec] == null)
                                {
                                    AssetsFile["project"]["frameworks"][framework.Name]["dependencies"][nameAndVersion[0]] = versionedDependency;
                                }
                            }

                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        versionlessPackagesFound = true;
                        namesOfVersionlessPackages += package.ItemSpec + ", ";
                    }
                }

                if (versionlessPackagesFound)
                {
                    log.LogError("ImplicitPackageReferenceBuildTask failed, could not find packages in known dependencies for: " + namesOfVersionlessPackages.Substring(0,namesOfVersionlessPackages.Length-1));
                    return false;
                }
                try
                {
                    using (StreamWriter file = File.CreateText(AssetsFilePath))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Formatting = Formatting.Indented;
                        serializer.Serialize(file, AssetsFile);
                    }
                }
                catch (Exception e)
                {
                    log.LogError("ImplicitPackageReferenceBuildTask failed, failed to write out changes to " + AssetsFilePath + ". Exception:"  + e.Message);
                    return false;
                }
            }
            else
            {
                log.LogError("ImplicitPackageReferenceBuildTask failed, could not find: " + AssetsFilePath);
                return false;
            }

            return true;
        }
    }
}
