using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

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

            Write(string.Format("Invoked: {0}", cmd));
            Run(args);
        }

        static readonly List<string> terminators = new List<string> { "bye", "exit" };

        static void Terminate()
        {
            Write("Continue on exit? (Y/N)");
            var result = Console.ReadKey();
            Write();
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
                    Write("Invalid input!");
                    Terminate();
                }
            }
        }

        static void Write(string msg = "")
        {
            Console.WriteLine(msg);
        }

        static string Read(string[] args, bool runOnce, string promptMsg = "")
        {
            ReadOptions(args);

            if (runOnce)
                Environment.Exit(0);

            Console.Write("demo > {0} ", promptMsg.Trim());
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
                    continue;

                var cmd = item.Substring(1).ToLower();
                switch (cmd)
                {
                    case "li":
                    case "list":
                        ListItems(Environment.CurrentDirectory);
                        break;
                    default:
                        break;
                }
            }
        }

        #region Option Commands
        static readonly int padLength = 4;
        static readonly string truncated = "... ";

        static string breakLine(int count, int padCount)
        {
            return new string('=', padLength * padCount * count);
        }

        static string appendTab(string value, int padCount)
        {
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
            Write(appendTab(info.Name, fileNameLimit) + appendTab(info.Attributes, attributeLimit) +
                appendTab(string.Format("{0:#,#0}", info.Length), sizeLimit));
        }

        static void WriteDirectoryInfo(string directory)
        {
            var info = new DirectoryInfo(directory);
            Write(appendTab(info.Name, fileNameLimit) + appendTab(info.Attributes, attributeLimit));
        }

        static void ListItems(string directory, bool recursiveList = false)
        {
            var br = breakLine(1, fileNameLimit) + breakLine(1, attributeLimit) + breakLine(1, sizeLimit);
            try
            {
                Write(br);
                Write(appendTab("File Name", fileNameLimit) + appendTab("Type", attributeLimit) + appendTab("Size", sizeLimit));
                Write(br);
                if (recursiveList)
                    foreach (string d in Directory.GetDirectories(directory))
                    {
                        foreach (string f in Directory.GetFiles(d))
                            WriteFileInfo(f);

                        ListItems(d);
                    }                foreach (string d in Directory.GetDirectories(directory))
                    WriteDirectoryInfo(d);
                foreach (string f in Directory.GetFiles(directory))
                    WriteFileInfo(f);
            }
            catch (Exception e)
            {
                Write(e.Message);
            }
            Write(br);
        }
        #endregion
    }
}
