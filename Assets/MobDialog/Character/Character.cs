using UnityEngine;
namespace MobDialog
{
#if UNITY_EDITOR
    [System.Serializable]
#endif
    /// <summary>
    /// 角色数据类，包含角色的信息，因为实际存储不用序列化，所以只在编辑器下使用Serializable属性方便编辑器显示
    /// </summary>
    public class Character
    {
        public string name, camp;
        public Color nameColor, campColor;
        public Sprite head;
        public Sprite[] portrait;
        public Rect[] rect;
        public Vector2 express;
    }
}
