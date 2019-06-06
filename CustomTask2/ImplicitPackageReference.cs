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

namespace ImplicitPackage
{
    public class ImplicitPackageReference : Task
    {
        /// AssetsFilePath:This is a path to the project.assets.json from which the implicit versions for dependinces will be found
        /// DependenciesToVersionAndPackage: This is a strong of packages that are ; seperated that need versions
        /// Result: Return true if task executed successfully, and returns false if there was a problem

        public string AssetsFilePath = "obj\\project.assets.json";

        [Required]
        public string DependenciesToVersionAndPackage { get; set; }

        [Output]
        public double Result { get; set; }

        private bool VersionlessPackagesFound = false;

        private string NamesOfVersionlessPackages = "";

        public ImplicitPackageReference() : base()
        {
       
        }

        public override bool Execute()
        {
            FileInfo fi = new FileInfo(AssetsFilePath);
            if (fi.Exists)
            {
                JObject AssetsFile;
                try
                {
                    AssetsFile = JObject.Parse(File.ReadAllText(AssetsFilePath));
                }catch(Exception e)
                {
                    if (this.BuildEngine != null) Log.LogError(AssetsFilePath + " could not be parsed", null);
                    return false;
                }
                
                foreach (string package in DependenciesToVersionAndPackage.Split(';'))
                {
                    int count = 0;
                    if(AssetsFile["libraries"] == null)
                    {
                        if (this.BuildEngine != null) Log.LogError(AssetsFilePath + " missing Libraries section.", null);
                        return false;
                    }

                    foreach (var library in AssetsFile["libraries"].Children<JProperty>())
                    {
                        //Name is index [0], Version is index [1]
                        var nameAndVersion = library.Name.Split('/');

                        if (nameAndVersion[0] == package)
                        {
                            JObject versionedDependency = new JObject();
                            versionedDependency.Add("target", "Package");
                            versionedDependency.Add("version", "[" + nameAndVersion[1] + ", )");
                            foreach (var framework in AssetsFile["project"]["frameworks"].Children<JProperty>())
                            {
                                if (framework.Value["dependencies"][package] == null)
                                {
                                    AssetsFile["project"]["frameworks"][framework.Name]["dependencies"][nameAndVersion[0]] = versionedDependency;
                                }
                            }
                            count++;
                            break;
                        }
                    }
                    if (count == 0)
                    {
                        VersionlessPackagesFound = true;
                        NamesOfVersionlessPackages += package + ",";
                    }
                }

                using (StreamWriter file = File.CreateText(AssetsFilePath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(file, AssetsFile);
                }
            }
            else
            {
                //TODO:Turn into error
                if(this.BuildEngine != null) Log.LogError(AssetsFilePath + " could not be found", null);
                return false;
            }

            if (VersionlessPackagesFound)
            {
                //TODO:Turn into error
                if (this.BuildEngine != null) Log.LogError("Could not find packages in known dependencys for: " + NamesOfVersionlessPackages, null);
                return false;
            }

            return true;
        }
    }
}
