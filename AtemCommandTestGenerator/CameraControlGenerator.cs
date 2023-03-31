using LibAtem.Commands.CameraControl;
using LibAtem.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AtemCommandTestGenerator
{
    class CameraControlGenerator
    {
        public static IEnumerable<CommandEntry> GenerateGetCommands(Dictionary<string, List<CommandEntry>> lastData)
        {
            var commandHash = AutoCommandGenerator.GenerateCommandHash(typeof(CameraControlGetCommand));
            if (lastData.TryGetValue(commandHash, out var previous))// Comment out this block to force regeneration
            {
                // Re-use the ones from last time, as they must still be valid

                foreach (var entry in previous)
                    yield return entry;

                yield break;
            }

            for (int i = 0; i < 50; i++)
            {
                var cmd = new CameraControlGetCommand();
                cmd.PeriodicFlushEnabled = Randomiser.Bool();
                FillCameraControlCommandBase(cmd);

                yield return new CommandEntry(cmd);
            }


            yield break;

        }
        public static IEnumerable<CommandEntry> GenerateSetCommands(Dictionary<string, List<CommandEntry>> lastData)
        {
            var commandHash = AutoCommandGenerator.GenerateCommandHash(typeof(CameraControlSetCommand));
            if (lastData.TryGetValue(commandHash, out var previous)) // Comment out this block to force regeneration
            {
                // Re-use the ones from last time, as they must still be valid

                foreach (var entry in previous)
                    yield return entry;

                yield break;
            }

            for (int i = 0; i < 50; i++)
            {
                var cmd = new CameraControlSetCommand();
                cmd.Relative = Randomiser.Bool();
                FillCameraControlCommandBase(cmd);

                yield return new CommandEntry(cmd);
            }


            yield break;

        }

        private static void FillCameraControlCommandBase(CameraControlCommandBase cmd)
        {
            cmd.Input = (VideoSource)Randomiser.RangeInt(255);
            cmd.Category = Randomiser.RangeInt(255);
            cmd.Parameter = Randomiser.RangeInt(255);
            cmd.Type = Randomiser.EnumValue<CameraControlDataType>();

            int count = Randomiser.RangeInt(1, 4);

            switch (cmd.Type)
            {
                case CameraControlDataType.Bool:
                    cmd.BoolData = Enumerable.Repeat(false, count).ToArray();
                    for (int o = 0; o < count; o++)
                        cmd.BoolData[o] = Randomiser.Bool();

                    break;
                case CameraControlDataType.SInt8:
                    cmd.IntData = Enumerable.Repeat(0, count).ToArray();
                    for (int o = 0; o < count; o++)
                        cmd.IntData[o] = Randomiser.RangeInt(sbyte.MinValue, sbyte.MaxValue);
                    break;
                case CameraControlDataType.SInt16:
                    cmd.IntData = Enumerable.Repeat(0, count).ToArray();
                    for (int o = 0; o < count; o++)
                        cmd.IntData[o] = Randomiser.RangeInt(short.MinValue, short.MaxValue);
                    break;
                case CameraControlDataType.SInt32:
                    cmd.IntData = Enumerable.Repeat(0, count).ToArray();
                    for (int o = 0; o < count; o++)
                        cmd.IntData[o] = Randomiser.RangeInt(int.MinValue, int.MaxValue);
                    break;
                case CameraControlDataType.SInt64:
                    cmd.LongData = Enumerable.Repeat(0L, count).ToArray();
                    for (int o = 0; o < count; o++)
                        cmd.LongData[o] = Randomiser.RangeLong(long.MinValue, long.MaxValue);
                    break;
                case CameraControlDataType.String:
                    cmd.StringData = Randomiser.String(count);
                    break;
                case CameraControlDataType.Float:
                    cmd.FloatData = Enumerable.Repeat(0.0, count).ToArray();
                    for (int o = 0; o < count; o++)
                        cmd.FloatData[o] = Randomiser.Range(-15, 15, 1);
                    break;
                default:
                    throw new Exception(string.Format("Unknown CameraControlDataType: {0}", cmd.Type));
            }
        }
    }
}
