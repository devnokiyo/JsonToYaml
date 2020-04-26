using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;

namespace JsonToYaml
{
    class Program
    {
        private static readonly ISerializer serializer = new SerializerBuilder()
            .WithEventEmitter(q => new ForceQuotedStringValuesEventEmitter(q))
            .Build();

        static void Main(string[] args)
        {
            var filePaths = new List<string>(Environment.GetCommandLineArgs());

            if (filePaths.Count <= 1)
            {
                WriteLineAndReadKey("ファイルがありません。");
                return;
            }

            foreach (var filePath in filePaths.Skip(1))
            {
                var fileName = Path.GetFileName(filePath);
                if (Path.GetExtension(filePath) != ".json")
                {
                    WriteLineAndReadKey($"拡張子がjsonではありません。({fileName})");
                    return;
                }

                try
                {
                    string json;
                    using (var sr = new StreamReader(filePath))
                        json = sr.ReadToEnd();

                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                    var yaml = serializer.Serialize(JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter()));

                    using var sw = new StreamWriter($@"{Path.GetDirectoryName(filePath)}\{fileNameWithoutExtension}.yml");
                    sw.Write(yaml);
                }
                catch
                {
                    Console.WriteLine($"YAMLへの変換に失敗しました。({fileName})");
                }
            }

            WriteLineAndReadKey("変換が終わりました。");
        }

        private static void WriteLineAndReadKey(string value)
        {
            Console.WriteLine(value);
            Console.WriteLine("何かキーを押してください。");
            Console.ReadKey();
        }
    }
}