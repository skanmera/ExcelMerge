namespace ExcelMerge.GUI.Commands
{
    public static class CommandFactory
    {
        public static ICommand Create(CommandLineOption option)
        {
            switch (option.MainCommand)
            {
                case CommandType.None:
                    return new DiffCommand(option);
                case CommandType.Diff:
                    return new DiffCommand(option);
            }

            throw new Exceptions.ExcelMergeException(true, $"{option.MainCommand} is unkown command");
        }
    }
}
