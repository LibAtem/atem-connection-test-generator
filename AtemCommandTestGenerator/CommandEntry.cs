using LibAtem.Commands;
using System;

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
            firstVersion = (uint)info.Item2;
            bytes = BitConverter.ToString(raw.ToByteArray());
            command = raw;
        }

        public string name;
        public uint firstVersion;
        public string bytes;
        public object command;
        public string commandHash;
    }
}
