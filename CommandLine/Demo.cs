using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using static System.Console;

namespace CommandLine
{
    class Demo
    {
        static void Main(string[] args)
        {
            Console.Title = typeof(Demo).Name;
            Run(args, args.Length > 0);
        }

        static void Run(string[] args, bool runOnce = false)
        {
            var cmd = Read(args, runOnce);

            if (string.IsNullOrEmpty(cmd))
                Run(args);

            var options = Regex.Split(cmd, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*) (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            if (options.Length >= 1)
                Run(options);

            if (terminators.Count(t => t == cmd.ToLower()) > 0)
                Terminate();

            Run(args);
        }

        static readonly List<string> terminators = new List<string> { "bye", "exit" };

        static void Terminate()
        {
            WriteLine("Continue on exit? (Y/N)");
            var result = Console.ReadKey();
            WriteLine();
            if (char.ToUpper(result.KeyChar) == 'Y')
            {
                Write("Goodbye!");
                Thread.Sleep(1000);
                Environment.Exit(0);
            }
            else
            {
                if (char.ToUpper(result.KeyChar) != 'N')
                {
                    WriteLine("Invalid input!");
                    Terminate();
                }
            }
        }

        static string Read(string[] args, bool runOnce, string promptMsg = "")
        {
            ReadOptions(args);

            if (runOnce)
                Environment.Exit(0);

            Write("Demo > {0} ", promptMsg.Trim());
            return Console.ReadLine().Trim();
        }

        static bool IsOption(string arg)
        {
            return arg.StartsWith("-") || arg.StartsWith("/");
        }

        static void RemoveDuplicateOptions(ref string[] options, int index = 0)
        {
            if (index >= options.Length)
                return;

            string option = options[index];
            if (options.Count(o => o == option) > 1)
            {
                options = options.Where(val => val != option).ToArray();
                var newLength = options.Length + 1;
                Array.Resize(ref options, newLength);
                options[newLength - 1] = option;

                RemoveDuplicateOptions(ref options, index);
            }
            else
                RemoveDuplicateOptions(ref options, index + 1);
        }

        static void ReadOptions(string[] args)
        {
            RemoveDuplicateOptions(ref args);

            foreach (var item in args)
            {
                if (!IsOption(item))
                {
                    WriteLine("'{0}' is currently not recognized by this cmd tool.", item);
                    continue;
                }

                var cmd = item.Substring(1).ToLower();
                switch (cmd)
                {
                    case "li":
                    case "list":
                        ListItems(Environment.CurrentDirectory);
                        break;
                    case "h":
                    case "help":
                        ShowHelp();
                        break;
                    default:
                        WriteLine("'-{0}' is currently not recognized by this cmd tool.", cmd);
                        break;
                }
            }

        }

        #region Option Commands
        static readonly int padLength = 4;
        static readonly string truncated = "... ";

        static string breakLine(int clusters)
        {
            return new string('=', padLength * clusters);
        }

        static string appendTab(string value, int padCount)
        {
            if (padCount < 1)
                return value;

            var padding = new string(' ', padLength * padCount);
            var result = value + padding;
            if (result.Length > padding.Length)
            {
                if (result[padding.Length - 1] != ' ')
                    result = result.Substring(0, padding.Length - truncated.Length) + truncated;
                else
                    result = result.Substring(0, padding.Length);
            }
            return result;
        }

        static string appendTab(object value, int padCount)
        {
            return appendTab(value.ToString(), padCount);
        }

        static readonly int fileNameLimit = 10;
        static readonly int attributeLimit = 8;
        static readonly int sizeLimit = 3;

        static void WriteFileInfo(string fileName)
        {
            var info = new FileInfo(fileName);
            WriteLine(appendTab(info.Name, fileNameLimit) + appendTab(info.Attributes, attributeLimit) +
                appendTab(string.Format("{0:#,#0}", info.Length), sizeLimit));
        }

        static void WriteDirectoryInfo(string directory)
        {
            var info = new DirectoryInfo(directory);
            WriteLine(appendTab(info.Name, fileNameLimit) + appendTab(info.Attributes, attributeLimit));
        }

        static void ListItems(string directory, bool recursiveList = false)
        {
            var br = breakLine(fileNameLimit) + breakLine(attributeLimit) + breakLine(sizeLimit);
            try
            {
                WriteLine(br);
                WriteLine(appendTab("File Name", fileNameLimit) + appendTab("Attributes", attributeLimit) + appendTab("Size", sizeLimit));
                WriteLine(br);
                if (recursiveList)
                    foreach (string d in Directory.GetDirectories(directory))
                    {
                        foreach (string f in Directory.GetFiles(d))
                            WriteFileInfo(f);

                        ListItems(d);
                    }
                foreach (string d in Directory.GetDirectories(directory))
                    WriteDirectoryInfo(d);
                foreach (string f in Directory.GetFiles(directory))
                    WriteFileInfo(f);
            }
            catch (Exception e)
            {
                WriteLine(e.Message);
            }
            WriteLine(br);
        }

        static void ShowHelp()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            WriteLine("{0} Version {1}", fvi.ProductName, fvi.ProductVersion);
            WriteLine(fvi.LegalCopyright);
            WriteLine(fvi.Comments);
            WriteLine();
            WriteLine("Available commands:");
            WriteLine();
            WriteLine(appendTab("", 2) + appendTab("li, list", 4) + appendTab("Lists all files in the current directory.", 0));
            WriteLine(appendTab("", 2) + appendTab("h, help", 4) + appendTab("Displays 'Help' contents.", 0));
            WriteLine();
        }
        #endregion
    }
}
