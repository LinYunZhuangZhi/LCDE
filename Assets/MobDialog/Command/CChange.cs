namespace MobDialog
{
    public class CChange:Command
    {
        [Portrait(true)]
        public byte portrait;
        public byte face;


        public override void Execute()
        {
            if(portrait< CharacterManager.Instance.portraits.Count)
                CharacterManager.Instance.portraits[portrait].ChangePortrait(face);
            Next();
        }
        public override void Exit()
        {
            
        }
    }
}