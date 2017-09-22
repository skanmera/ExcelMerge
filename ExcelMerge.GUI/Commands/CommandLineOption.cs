using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using CommandLine;

namespace ExcelMerge.GUI.Commands
{
    public class CommandLineOption
    {
        [ValueList(typeof(List<string>))]
        public IList<string> Commands { get; set; } = new List<string>();

        [Option('s', "src-path")]
        public string SrcPath { get; set; } = string.Empty;

        [Option('d', "dst-path")]
        public string DstPath { get; set; } = string.Empty;

        [Option('c', "external-cmd")]
        public string ExternalCommand { get; set; } = string.Empty;

        [Option('i', "immediately-execute-external-cmd")]
        public bool ImmediatelyExecuteExternalCommand { get; set; }

        [Option('w', "wait-external-cmd")]
        public bool WaitExternalCommand { get; set; }

        [Option('v', "validate-extension")]
        public bool ValidateExtension { get; set; }

        [Option('e', "empty-file-name")]
        public string EmptyFileName { get; set; } = string.Empty;

        [Option('k', "keep-file-history")]
        public bool KeepFileHistory { get; set; }


        public CommandType MainCommand
        {
            get
            {
                return (CommandType)Enum.Parse(typeof(CommandType), Commands.FirstOrDefault() ?? CommandType.Diff.ToString(), true);
            }
        }

        public void ConvertToFullPath()
        {
            SrcPath = !string.IsNullOrEmpty(SrcPath) ? Path.GetFullPath(SrcPath) : SrcPath;
            DstPath = !string.IsNullOrEmpty(DstPath) ? Path.GetFullPath(DstPath) : DstPath;
        }
    }
}
