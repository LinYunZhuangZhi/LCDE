namespace MobDialog
{
    public class Synchronization : GroupCommand,ISkipable,IWaitClick
    {
        ushort finish;
        bool wait;
        public Synchronization() { }
        public Synchronization(Command command)
        {
            commands = new Command[] {command };
        }
        public bool Complete
        {
            get
            {
                foreach (var command in commands)
                {
                    if (command is IWaitClick wait)
                    {
                        if (!wait.Complete)
                        {
                            return false;
                        }
                    }
                    else if(command is TweenCommand tc&&tc.tween!=null)
                    {
                        if (tc.tween.active)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        public override void Execute()
        {
            if (commands == null || commands.Length == 0)
            {
                Next();
                return;
            }
            finish = 0;
            wait = false;
            foreach (var command in commands)
            {
                if (command is IWaitClick)
                {
                    wait = true;
                }
                command.Next +=MNext;
                command.Execute();
                void MNext()
                {
                    command.Exit();
                    finish ++;
                    command.Next -= MNext;
                    if (finish==commands.Length&&!wait)
                    {
                        Next();
                    }
                }
            }
        }

        public override void Exit()
        {
            foreach (var command in commands)
            {
                if (command is IWaitClick)
                {
                    command.Exit();
                }
            }
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