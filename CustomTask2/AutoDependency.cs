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
	public class AutoDependency : ITask
	{

		[Required]
        public string Path { get; set; }
		public string Search { get; set; }

		[Output]
		public double Result { get; set; }

		public IBuildEngine BuildEngine { get; set; }
		public ITaskHost HostObject { get; set; }

		public bool Execute()
		{
			FileInfo fi = new FileInfo(Path);
			if (fi.Exists)
			{
				BuildEngine.LogMessageEvent(new BuildMessageEventArgs("File Exists", "Check", "FileCheck", MessageImportance.High));

                dynamic o1 = JObject.Parse(File.ReadAllText(Path));

                if (Search == null) return true;

                foreach (string item in Search.Split(';'))
                {
                    int count = 0;
                    Console.WriteLine(item);
                    foreach (var child in o1["libraries"].Children())
                    {
                        string name = child.Name.Split('/')[0];
                        string vers = child.Name.Split('/')[1];

                        if (name == item)
                        {
                            JObject j = new JObject();
                            j.Add("target", "Package");
                            j.Add("version", "[" + vers + ", )");
                            if (o1["project"]["frameworks"]["netstandard2.0"]["dependencies"][item] == null)
                            {
                                o1["project"]["frameworks"]["netstandard2.0"]["dependencies"].Add(name, j);
                                o1["projectFileDependencyGroups"][".NETStandard,Version=v2.0"].Add(name + " >= " + vers);
                            }
                            count++;
                        }
                        if (count == 0)
                        {
                            BuildEngine.LogMessageEvent(new BuildMessageEventArgs("Could not find version for " + item, "Check", "FileCheck", MessageImportance.High));
                        }
                    }
                    Console.WriteLine("-------------------");
                }

                using (StreamWriter file = File.CreateText(@"obj\project.assets.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(file, o1);
                }
            }
			else
			{
				BuildEngine.LogMessageEvent(new BuildMessageEventArgs("File does not Exists", "Check", "FileCheck", MessageImportance.High));
			}

			return true;
		}
	}
}
