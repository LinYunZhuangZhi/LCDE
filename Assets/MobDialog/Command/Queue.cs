namespace MobDialog
{
    public class Queue : GroupCommand,ISkipable
    {
        Command currentCommand;
        public override void Execute()
        {
            if (commands==null||commands.Length==0)
            {
                Next();
                return;
            }
            currentCommand = commands[0];
            for (int i = 1; i < commands.Length; i++)
            {
                Command command = commands[i-1];
                Command command2 = commands[i];
                command.Next += MNext;
                void MNext()
                {
                    command.Exit();
                    command.Next -= MNext;
                    currentCommand = command2;
                    command2.Execute();
                }
            }
            commands[^1].Next=NNext;
            commands[0].Execute();
            void NNext()
            {
                commands[^1].Next -= NNext;
                Next();
                return;
            }
        }

        public override void Exit()
        {

        }

        public void Skip()
        {
            foreach (var command in commands)
            {
                if (command is ISkipable skipable)
                {
                    skipable.Skip();
                }
            }
        }
    }
}
