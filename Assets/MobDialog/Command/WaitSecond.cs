using System.Collections;
using UnityEngine;
namespace MobDialog
{
    public class WaitSecond : Command,ISkipable
    {
        public bool skipable;
        public float time;
        Coroutine coroutine;
        public override void Execute()
        {
            coroutine= DialogManager.Instance.StartCoroutine(enumerator());
        }

        public override void Exit()
        {
            
        }


        public void Skip()
        {
            if (skipable)
            {
                DialogManager.Instance.StopCoroutine(coroutine);
                Next();
            }
        }
        IEnumerator enumerator()
        {
            yield return new WaitForSeconds(time);
            Next();
        }
    }
}