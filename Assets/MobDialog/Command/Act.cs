using System;
namespace MobDialog
{
    public class Act : Command
    {
        public Action action;
        public Act() { }

        public Act(Action action)
        {
            this.action = action;
        }

        public override void Execute()
        {
            action?.Invoke();
            Next();
        }

        public override void Exit()
        {
            
        }
    }
}