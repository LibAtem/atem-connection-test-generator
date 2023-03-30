using LibAtem.Commands;
using LibAtem.Commands.DataTransfer;
using LibAtem.Serialization;
using LibAtem.Test.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace AtemCommandTestGenerator
{
    class AutoCommandGenerator
    {
        public static IEnumerable<CommandEntry> GenerateData(IReadOnlyList<CommandEntry> lastData)
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
                    lastDataByHash.Add(entry.commandHash, new List<CommandEntry> { entry });
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

                if (!typeof(SerializableCommandBase).GetTypeInfo().IsAssignableFrom(type))
                {
                    if (type == typeof(DataTransferDataCommand))
                    {
                        // TODO 
                        yield return new CommandEntry(new DataTransferDataCommand
                        {
                            TransferId = 0x1bf4,
                            Body = Util.RandomBytes(12)
                        });
                        yield return new CommandEntry(new DataTransferDataCommand
                        {
                            TransferId = 0x001b,
                            Body = Util.RandomBytes(242)
                        });
                    }

                    continue;
                }

                // if (type != typeof(PowerStatusCommand))
                //     continue;

                var cases = new List<string>();

                for (int i = 0; i < 10; i++)
                {
                    ICommand raw = (ICommand)RandomPropertyGenerator.Create(type);

                    var cs1 = new CommandEntry(raw) { commandHash = commandHash };
                    var cs1s = JsonConvert.SerializeObject(cs1, Formatting.Indented, new Int64Converter());
                    if (cases.Contains(cs1s))
                    {
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

            var cmdAttr = t.GetCustomAttribute<CommandNameAttribute>();
            str.Append($"{cmdAttr.Name},{cmdAttr.MinimumVersion},{cmdAttr.Length}:");

            foreach (PropertyInfo prop in t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var serAttr = prop.GetCustomAttribute<SerializeAttribute>();

                SerializableAttributeBase attr = prop.GetCustomAttributes().OfType<SerializableAttributeBase>().FirstOrDefault();
                if (attr != null)
                {
                    str.Append($"{prop.Name},{serAttr?.StartByte ?? 0},{attr.GetType().FullName},{attr.Size},{attr.GetHashString()}.");
                    continue;
                }

                if (serAttr == null)
                    continue;

                Assert.True(false, string.Format("Missing generator attribute for property: {0}", prop.Name));
            }

            using var sha = SHA256.Create();
            var hashbytes = sha.ComputeHash(Encoding.UTF8.GetBytes(str.ToString()));
            return BitConverter.ToString(hashbytes);
        }
    }
}
