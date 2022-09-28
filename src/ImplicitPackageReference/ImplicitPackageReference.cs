﻿// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------
namespace Microsoft.Build.ImplicitPackageReference
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// This msbuild task is designed to parse a project.assets.json file and be able to take a string of dependiencies that need versions and be able to find the correct version
    /// inside the project.assets.json file.
    /// </summary>
    public class AddImplicitPackageReferences : Task
    {
        /// <summary>
        /// This is a path to the project.assets.json from which the implicit versions for dependencies will be found
        /// </summary>
        [Required]
        public string AssetsFilePath { get; set; }
        /// <summary>
        /// This is a string of packages that are ; separated that need versions
        /// </summary>
        [Required]
        public ITaskItem[] DependenciesToVersionAndPackage { get; set; }

        private ILogger log { get; set; }

        public AddImplicitPackageReferences() : base()
        {
            log = new StandardLogger(Log);
        }

        public AddImplicitPackageReferences(ILogger log)
        {
            this.log = log;
        }

        public override bool Execute()
        {
            if(DependenciesToVersionAndPackage.Length == 0)
            {
                log.LogMessage("AddImplicitPackageReferences was not given any packages to version");
                return true;
            }

            if (File.Exists(AssetsFilePath))
            {
                bool versionlessPackagesFound = false;
                StringBuilder namesOfVersionlessPackages = new StringBuilder();
                JObject assetsFile;
                try
                {
                    assetsFile = JObject.Parse(File.ReadAllText(AssetsFilePath));
                }
                catch (Exception e)
                {
                    log.LogError("AddImplicitPackageReferences failed, could not parse file " + AssetsFilePath + ". Exception:" + e.Message);
                    return false;
                }

                if (assetsFile["libraries"] == null)
                {
                    log.LogError("AddImplicitPackageReferences failed, " + AssetsFilePath + " missing Libraries section.");
                    return false;
                }

                foreach (var package in DependenciesToVersionAndPackage)
                {
                    bool found = false;

                    foreach (var library in assetsFile["libraries"].Children<JProperty>())
                    {
                        //Name is index [0], Version is index [1]
                        string[] nameAndVersion = library.Name.Split('/');
                        if (nameAndVersion.Length != 2)
                        {
                            log.LogError("AddImplicitPackageReferences failed, " + AssetsFilePath + " formatted incorrectly");
                            return false;
                        }

                        if (nameAndVersion[0] == package.ItemSpec)
                        {
                            JObject versionedDependency = new JObject();

                            string includedAssets = ParseIncludedAssets(package);
                            string suppressParent = ParseSuppressParent(package);
                            if (includedAssets != null)
                            {
                                versionedDependency.Add("include", includedAssets);
                            }
                            if (suppressParent != null)
                            {
                                versionedDependency.Add("suppressParent", suppressParent);
                            }
                            versionedDependency.Add("target", "Package");
                            versionedDependency.Add("version", "[" + nameAndVersion[1] + ", )");

                            foreach (var framework in assetsFile["project"]["frameworks"].Children<JProperty>())
                            {
                                if (framework.Value["dependencies"][package.ItemSpec] == null)
                                {
                                    assetsFile["project"]["frameworks"][framework.Name]["dependencies"][nameAndVersion[0]] = versionedDependency;
                                }
                            }

                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        versionlessPackagesFound = true;
                        namesOfVersionlessPackages.Append(package.ItemSpec).Append(", ");
                    }
                }

                if (versionlessPackagesFound)
                {
                    log.LogError("AddImplicitPackageReferences failed, could not find packages in known dependencies for: " + namesOfVersionlessPackages.ToString().Substring(0, namesOfVersionlessPackages.Length - 1));
                    return false;
                }
                try
                {
                    using (StreamWriter file = File.CreateText(AssetsFilePath))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Formatting = Formatting.Indented;
                        serializer.Serialize(file, assetsFile);
                    }
                }
                catch (Exception e)
                {
                    log.LogError("AddImplicitPackageReferences failed, failed to write out changes to " + AssetsFilePath + ". Exception:" + e.Message);
                    return false;
                }
            }
            else
            {
                log.LogError("AddImplicitPackageReferences failed, could not find: " + AssetsFilePath);
                return false;
            }

            return true;
        }

        [Flags]
        private enum AssetTypes
        {
            None = 0,
            Compile = 1,
            Runtime = 2,
            ContentFiles = 4,
            Build = 8,
            Native = 16,
            Analyzers = 32,
            All = 63
        }

        private string ParseIncludedAssets(ITaskItem packageReference)
        {
            var includeAssetsMetadata = packageReference.GetMetadata("IncludeAssets");
            var excludeAssetsMetadata = packageReference.GetMetadata("ExcludeAssets");

            if (string.IsNullOrEmpty(includeAssetsMetadata) && string.IsNullOrEmpty(excludeAssetsMetadata))
            {
                return null;
            }

            var includeAssets = AssetTypes.All;
            var excludeAssets = AssetTypes.None;

            if (!string.IsNullOrEmpty(includeAssetsMetadata))
            {
                includeAssets = ParseAssetsMetadata(includeAssetsMetadata);
            }
            if (!string.IsNullOrEmpty(excludeAssetsMetadata))
            {
                excludeAssets = ParseAssetsMetadata(excludeAssetsMetadata);
            }

            var includedAssets = includeAssets & ~excludeAssets;

            return AssetTypesToString(includedAssets);

        }

        private string ParseSuppressParent(ITaskItem packageReference)
        {
            var metadata = packageReference.GetMetadata("PrivateAssets");
            if (string.IsNullOrEmpty(metadata))
            {
                return null;
            }

            return AssetTypesToString(ParseAssetsMetadata(metadata));
        }

        private AssetTypes ParseAssetsMetadata(string metadata)
        {
            AssetTypes assetType = AssetTypes.None;
            foreach (string part in metadata.Split(';').Select(a => a.Trim()))
            {
               if (Enum.TryParse(part, true, out AssetTypes partType))
               {
                    assetType |= partType;
               }
            }

            return assetType;                
        }

        private string AssetTypesToString(AssetTypes assetTypes)
        {
            if (assetTypes == AssetTypes.None)
            {
                return "None";
            }

            if (assetTypes == AssetTypes.All)
            {
                return "All";
            }

            List<string> parts = new List<string>();

            foreach (AssetTypes value in Enum.GetValues(typeof(AssetTypes)))
            {
                if (value == AssetTypes.None || value == AssetTypes.All)
                {
                    continue;
                }

                if ((assetTypes & value) != AssetTypes.None)
                {
                    parts.Add(Enum.GetName(typeof(AssetTypes), value));
                }
            }

            return string.Join(", ", parts);
        }
    }
}
