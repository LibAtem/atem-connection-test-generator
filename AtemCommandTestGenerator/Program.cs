﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AtemCommandTestGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            List<CommandEntry> lastData = new List<CommandEntry>();
            if (File.Exists("data.json"))
            {
                var lastDataStr = File.ReadAllText("data.json");
                lastData = JsonConvert.DeserializeObject<List<CommandEntry>>(lastDataStr, new Int64Converter());
            }

            var data = AutoCommandGenerator.GenerateData(lastData).ToList();
            var str = JsonConvert.SerializeObject(data, Formatting.Indented, new Int64Converter());
            File.WriteAllText("data.json", str);
            //var file = new StreamWriter("commands.json");
            Console.WriteLine("Test Cases Written!");
        }
    }
}
