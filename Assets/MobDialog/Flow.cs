using UnityEngine;

namespace MobDialog
{
    public class Flow
    {
        public Sprite[] BGs;
        public Character[] characters;
        public Command[] commands;
        public Flow()
        {
        }

        public Flow(Sprite[] bGs, Character[] characters, Command[] commands)
        {
            BGs = bGs;
            this.characters = characters;
            this.commands = commands;
        }
    }
}
