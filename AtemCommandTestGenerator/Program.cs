using LibAtem.Commands;
using LibAtem.Commands.DataTransfer;
using LibAtem.Test.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using LibAtem.Serialization;
using Xunit;

namespace AtemCommandTestGenerator
{
    class CommandEntry
    {
        public CommandEntry()
        {
        }
        
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
        public object command;
        public string commandHash;
    }

    class Program
    {
        public static readonly Random random = new Random();
        private static byte[] RandomBytes(int size) {
            var res = new byte[size];
            random.NextBytes(res);
            return res;
        }

        private static IEnumerable<CommandEntry> GenerateData(IReadOnlyList<CommandEntry> lastData)
        {
            var lastDataByHash = new Dictionary<string, List<CommandEntry>>();
            foreach (CommandEntry entry in lastData)
            {
                if (string.IsNullOrEmpty(entry.commandHash))
                    continue;

                if (lastDataByHash.TryGetValue(entry.commandHash, out var tmp))
                {
                    tmp.Add(entry);
                }
                else
                {
                    lastDataByHash.Add(entry.commandHash, new List<CommandEntry> {entry});
                }
            }
            
            Assembly assembly = typeof(ICommand).GetTypeInfo().Assembly;
            IEnumerable<Type> types = assembly.GetTypes().Where(t => typeof(ICommand).GetTypeInfo().IsAssignableFrom(t));
            foreach (Type type in types)
            {
                if (type.IsAbstract)
                    continue;

                var commandHash = GenerateCommandHash(type);
                if (lastDataByHash.TryGetValue(commandHash, out var previous))
                {
                    // Re-use the ones from last time, as they must still be valid
                    
                    foreach (var entry in previous)
                        yield return entry;

                    continue;
                }

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

                    var cs1 = new CommandEntry(raw) {commandHash = commandHash};
                    var cs1s = JsonConvert.SerializeObject(cs1, Formatting.Indented);
                    if (cases.Contains(cs1s)) {
                        continue;
                    }

                    cases.Add(cs1s);
                    yield return cs1;
                }
            }
        }

        static string GenerateCommandHash(Type t)
        {
            var str = new StringBuilder();
            
            foreach (PropertyInfo prop in t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var serAttr = prop.GetCustomAttribute<SerializeAttribute>();
                if (serAttr == null)
                    continue;

                str.AppendFormat($"{prop.Name},{serAttr.StartByte},");

                SerializableAttributeBase attr = prop.GetCustomAttributes().OfType<SerializableAttributeBase>().FirstOrDefault();
                if (attr != null)
                {
                    str.AppendFormat($"{attr.GetType().FullName},{attr.Size},{attr.GetHashString()}.");
                    continue;
                }

                Assert.True(false, string.Format("Missing generator attribute for property: {0}", prop.Name));
            }
            
            using var sha = SHA256.Create();
            var hashbytes = sha.ComputeHash(Encoding.UTF8.GetBytes(str.ToString()));
            return BitConverter.ToString(hashbytes);
        }

        static void Main(string[] args)
        {
            List<CommandEntry> lastData = new List<CommandEntry>();
            if (File.Exists("data.json"))
            {
                var lastDataStr = File.ReadAllText("data.json");
                lastData = JsonConvert.DeserializeObject<List<CommandEntry>>(lastDataStr);
            }
            
            var data = GenerateData(lastData).ToList();
            var str = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText("data.json", str);
            //var file = new StreamWriter("commands.json");
            Console.WriteLine("Hello World!");
        }
    }
}
