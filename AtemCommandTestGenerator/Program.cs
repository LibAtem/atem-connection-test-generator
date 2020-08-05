using LibAtem.Commands;
using LibAtem.Commands.DataTransfer;
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
        public CommandEntry(ICommand raw)
        {
            var info = CommandManager.FindNameAndVersionForType(raw);
            name = info.Item1;
            firstVersion = (uint) info.Item2;
            bytes = BitConverter.ToString(raw.ToByteArray());
            command = raw;
        }

        public string name;
        public uint firstVersion;
        public string bytes;
        public ICommand command;
    }

    class Program
    {
        public static readonly Random random = new Random();
        private static byte[] RandomBytes(int size) {
            var res = new byte[size];
            random.NextBytes(res);
            return res;
        }

        private static IEnumerable<CommandEntry> GenerateData()
        {
            Assembly assembly = typeof(ICommand).GetTypeInfo().Assembly;
            IEnumerable<Type> types = assembly.GetTypes().Where(t => typeof(ICommand).GetTypeInfo().IsAssignableFrom(t));
            foreach (Type type in types)
            {
                if (type.IsAbstract)
                    continue;

                if (!typeof(SerializableCommandBase).GetTypeInfo().IsAssignableFrom(type)) {
                    if (type == typeof(DataTransferDataCommand)) {
                        // TODO 
                        yield return new CommandEntry(new DataTransferDataCommand{
                            TransferId = 0x1bf4,
                            Body = RandomBytes(12)
                        });
                        yield return new CommandEntry(new DataTransferDataCommand{
                            TransferId = 0x001b,
                            Body = RandomBytes(242)
                        });
                    }

                    continue;
                }

                // if (type != typeof(PowerStatusCommand))
                //     continue;

                var cases = new List<string>();

                for(int i = 0; i < 10; i++)
                {
                    ICommand raw = (ICommand)RandomPropertyGenerator.Create(type);

                    var cs1 = new CommandEntry(raw);
                    var cs1s = JsonConvert.SerializeObject(cs1, Formatting.Indented);
                    if (cases.Contains(cs1s)) {
                        continue;
                    }

                    cases.Add(cs1s);
                    yield return cs1;
                }
            }
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
