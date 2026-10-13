namespace MobDialog
{
    public class CEmo : Command
    {
        [Portrait]
        public byte portrait;
        public byte emo;
        public override void Execute()
        {
            CharacterManager.Instance.Emo(portrait, emo);
            Next();
        }

        public override void Exit()
        {
        }
    }
}