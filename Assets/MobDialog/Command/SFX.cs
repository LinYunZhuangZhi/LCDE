namespace MobDialog
{
    public class SFX : Command
    {
        [Audio(false)]
        public string path;

        public override void Execute()
        {
            if (!string.IsNullOrEmpty(path))
            {
                AudioManager.Instance.PlaySfx(path);
            }
            Next();
        }

        public override void Exit()
        {

        }
    }
}