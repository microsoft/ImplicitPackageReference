using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace ImplicitPackageReferenceSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var testOject = JObject.Parse(File.ReadAllText("test.json"));
            Console.WriteLine(testOject["message"]);
            Console.ReadLine();
        }
    }
}
