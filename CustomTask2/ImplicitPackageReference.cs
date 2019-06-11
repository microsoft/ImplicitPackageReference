using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CustomTask2
{
	public class ImplicitPackageReference : ITask
	{

		[Required]
        public string AssetsFilePath { get; set; }
        //[Required]
		public string PackagesToSearch { get; set; }

		[Output]
		public double Result { get; set; }

		public IBuildEngine BuildEngine { get; set; }
		public ITaskHost HostObject { get; set; }

		public bool Execute()
		{
			FileInfo fi = new FileInfo(AssetsFilePath);
			if (fi.Exists)
			{
                JObject AssetsFile = JObject.Parse(File.ReadAllText(AssetsFilePath));

                if (PackagesToSearch == null) return true;

                foreach (string package in PackagesToSearch.Split(';'))
                {
                    int count = 0;
                    foreach (var library in AssetsFile["libraries"].Children<JProperty>())
                    {
                        string name = library.Name.Split('/')[0];
                        string vers = library.Name.Split('/')[1];

                        if (name == package)
                        {
                            JObject versionedDependency = new JObject();
                            versionedDependency.Add("target", "Package");
                            versionedDependency.Add("version", "[" + vers + ", )");
                            foreach(var framework in AssetsFile["project"]["frameworks"].Children<JProperty>())
                            {
                                if(framework.Value["dependencies"][package] == null)
                                {
                                    AssetsFile["project"]["frameworks"][framework.Name]["dependencies"][name] = versionedDependency;
                                }
                            }
                            count++;
                        }
                    }
                    if (count == 0)
                    {
                        BuildEngine.LogMessageEvent(new BuildMessageEventArgs("Could not find version for " + package, "Check", "FileCheck", MessageImportance.High));
                        BuildEngine.LogMessageEvent(new BuildMessageEventArgs("Pack Failed" + package, "Check", "FileCheck", MessageImportance.High));
                        return false;
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
				BuildEngine.LogMessageEvent(new BuildMessageEventArgs(AssetsFilePath + " could not be found", "Check", "FileCheck", MessageImportance.High));
                return false;
			}

			return true;
		}
	}
}
