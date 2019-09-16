using LibAtem.Commands;
using LibAtem.Commands.DownstreamKey;
using LibAtem.Test.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AtemCommandTestGenerator
{
    class CommandEntry
    {
        public string name;
        public string bytes;
        public ICommand command;
    }

    class Program
    {
        private static IEnumerable<CommandEntry> GenerateData()
        {
            var allowedTypes = new List<Type>(){
                typeof(DownstreamKeyStateGetCommand),
                typeof(DownstreamKeyAutoCommand),
            };

            Assembly assembly = typeof(ICommand).GetTypeInfo().Assembly;
            IEnumerable<Type> types = assembly.GetTypes().Where(t => typeof(SerializableCommandBase).GetTypeInfo().IsAssignableFrom(t));
            foreach (Type type in types)
            {
                if (type.IsAbstract)
                    continue;

                if (!allowedTypes.Contains(type))
                    continue;

                for (int i = 0; i < 10; i++)
                {
                    ICommand raw = (ICommand)RandomPropertyGenerator.Create(type);
                    string bytes = BitConverter.ToString(raw.ToByteArray());

                    yield return new CommandEntry()
                    {
                        name = CommandManager.FindNameForType(raw),
                        bytes = bytes,
                        command = raw,
                    };
                }
            }

            yield break;
        }

        static void Main(string[] args)
        {
            var data = GenerateData().ToList();
            var str = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText("data.json", str);
            //var file = new StreamWriter("commands.json");
            Console.WriteLine("Hello World!");
        }
    }
}
