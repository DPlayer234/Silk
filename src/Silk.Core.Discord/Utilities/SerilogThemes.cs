﻿using System;
using System.IO;
using Serilog.Sinks.SystemConsole.Themes;

namespace Silk.Core.Discord.Utilities
{
    public static class SerilogThemes
    {
        public static BotTheme Bot { get; } = new();
    }

    public class BotTheme : ConsoleTheme
    {
        public override bool CanBuffer => false;

        protected override int ResetCharCount => 0;

        public override void Reset(TextWriter output)
        {
            Console.ResetColor();
        }

        public override int Set(TextWriter output, ConsoleThemeStyle style)
        {
            (ConsoleColor foreground, ConsoleColor background) = style switch
            {
                ConsoleThemeStyle.Number => (ConsoleColor.Magenta, ConsoleColor.Black),
                ConsoleThemeStyle.LevelDebug => (ConsoleColor.Green, ConsoleColor.Black),
                ConsoleThemeStyle.LevelError => (ConsoleColor.White, ConsoleColor.Black),
                ConsoleThemeStyle.LevelFatal => (ConsoleColor.Yellow, ConsoleColor.Black),
                ConsoleThemeStyle.LevelVerbose => (ConsoleColor.Red, ConsoleColor.Yellow),
                ConsoleThemeStyle.LevelWarning => (ConsoleColor.DarkRed, ConsoleColor.Black),
                ConsoleThemeStyle.SecondaryText => (ConsoleColor.Blue, ConsoleColor.Black),
                ConsoleThemeStyle.LevelInformation => (ConsoleColor.DarkBlue, ConsoleColor.Black),
                _ => (ConsoleColor.Yellow, ConsoleColor.Black)
            };
            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background;
            return 0;
        }
    }
}