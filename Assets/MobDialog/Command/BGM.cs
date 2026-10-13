namespace MobDialog
{
    public class BGM:Command
    {
        [Audio(true)]
        public string path;

        public override void Execute()
        {
            if (string.IsNullOrEmpty(path))
            {
                AudioManager.Instance.StopBgm();
            }
            else
            {
                AudioManager.Instance.PlayBgm(path);
            }
            Next();
        }

        public override void Exit()
        {
            
        }
    }
}