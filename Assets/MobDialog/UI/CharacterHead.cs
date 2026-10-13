using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace MobDialog
{
    /// <summary>
    /// 角色头像UI类
    /// </summary>
    public class CharacterHead:MonoBehaviour
    {
        public Image image;
        public TextMeshProUGUI text;
        public void SetCharacter(Character character)
        {
            image.sprite = character.head;
            text.text= character.name;
        }
    }
}
