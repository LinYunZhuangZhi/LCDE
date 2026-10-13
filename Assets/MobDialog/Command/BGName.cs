namespace MobDialog
{
    public class BGName : Command
    {
        public string name;
        public override void Execute()
        {
            DialogManager.Instance.bg.BGNamePanel.SetActive(!string.IsNullOrEmpty(name));
            if(!string.IsNullOrEmpty(name))
                DialogManager.Instance.bg.BGName.text = name;
            Next();
        }

        public override void Exit()
        {
            
        }
    }
}