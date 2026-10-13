using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
//从其他项目拿的，这里没用
[Serializable]
public class ProcessManager
{
    public interface IProcess
    {
        public void Enter();
        public void Exit();
    }
    
    public class Process:IProcess
    {
        public Action enter;
        public Action exit;
        public Process(Action enter, Action exit)
        {
            this.enter = enter;
            this.exit = exit;
        }
        public Process() { }
        public void Enter()
        {
            var action=enter;
            enter = null;
            action?.Invoke();
        }
        public void Exit()
        {
            exit?.Invoke();
        }
    }
    public List<IProcess> processes = new List<IProcess>();
    public void Add(Action enter,Action over)
    {
        processes.Add(new Process(enter,over));
        processes[0]?.Enter();
    }
    public void Add(Action enter, Action over,int index)
    {
        if (processes.Count<=index)
        {
            processes.Add(new Process(enter, over));
            processes[0]?.Enter();
            return;
        }
        processes.Insert(index,new Process(enter,over));
        processes[0]?.Enter();
    }
    public void Add(Process[] processes,int index=int.MaxValue)
    {
        if (this.processes.Count <= index)
        {
            this.processes.AddRange(processes);
            this.processes[0]?.Enter();
            return;
        }
        this.processes.InsertRange(index, processes);
        this.processes[0]?.Enter();
    }
    public void Add(Process process, int index = int.MaxValue)
    {
        if (processes.Count <= index)
        {
            processes.Add(process);
            processes[0]?.Enter();
            return;
        }
        processes.Insert(index, process);
        processes[0]?.Enter();
    }
    public void Next()
    {
        if (processes.Count == 0)
        {
            return;
        }
        processes[0]?.Exit();
        processes.RemoveAt(0);
        if(processes.Count==0)
        {
            return;
        }
        processes[0].Enter();
    }
}
