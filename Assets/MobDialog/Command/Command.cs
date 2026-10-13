using System;
using System.Reflection;
namespace MobDialog
{
    /// <summary>
    /// 命令基类
    /// </summary>
    public abstract class Command
    {
        public abstract void Execute();
        public abstract void Exit();
        internal Action Next;
    }
    /// <summary>
    /// 可跳过接口，继承该接口的命令可以被跳过
    /// </summary>
    public interface ISkipable
    {
        public void Skip();
    }
    /// <summary>
    /// 等待点击接口，继承该接口的命令会在执行时等待用户点击后才会继续执行下一条命令
    /// </summary>
    public interface IWaitClick
    {
        public bool Complete { get; }
    }
    //这些属性用于标记命令的变量的特性
    public class BGAttribute : Attribute
    {
        public BGAttribute()
        {
        }
    }
    public class CharacterAttribute : Attribute
    {
        public bool Show { get; set; }
        public CharacterAttribute()
        {
        }
        public CharacterAttribute(bool show)
        {
            Show = show;
        }
    }
    public class PortraitAttribute : Attribute
    {
        public bool Show { get; set; }
        public PortraitAttribute()
        {
        }

        public PortraitAttribute(bool show)
        {
            Show = show;
        }
    }
    public class AudioAttribute : Attribute
    {
        public bool IsBGM { get; set; }
        public AudioAttribute()
        {
        }

        public AudioAttribute(bool isBGM)
        {
            IsBGM = isBGM;
        }
    }
    /// <summary>
    /// 命令组类，继承该类的命令可以包含多个子命令，为了显示好一点自带一个Say命令
    /// </summary>
    public abstract class GroupCommand:Command
    {
        public Command[] commands={new Say() };
    }
}