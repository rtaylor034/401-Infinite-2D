using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;



public class ControlledTask
{
    public ControlledAwaiter Awaiter { get; private set; }
    public ControlledTask()
    {
        Awaiter = new ControlledAwaiter();
    }

    public void Resolve()
    {
        Awaiter.Resolve();
    }

    public ControlledAwaiter GetAwaiter() => Awaiter;

    public class ControlledAwaiter : INotifyCompletion
    {
        private bool _completed;
        public bool IsCompleted => _completed;
        private Action _continueAction;

        public ControlledAwaiter()
        {
            _completed = false;
        }
        public void Resolve()
        {
            if (_completed) throw new Exception("Awaiter already resolved");
            _completed = true;
            _continueAction();
        }
        public void OnCompleted(Action continuation)
        {
            _continueAction = continuation;
        }

        public void GetResult() { }
    }
}

public class ControlledTask<T>
{
    public ControlledAwaiter<T> Awaiter { get; private set; }
    public ControlledTask()
    {
        Awaiter = new ControlledAwaiter<T>();
    }

    public void Resolve(T result)
    {
        Awaiter.Resolve(result);
    }

    public ControlledAwaiter<T> GetAwaiter() => Awaiter;

    public class ControlledAwaiter<B> : INotifyCompletion
    {
        private bool _completed;
        private B _result;
        public bool IsCompleted => _completed;
        private Action _continueAction;

        public ControlledAwaiter()
        {
            _completed = false;
        }
        public void Resolve(B result)
        {
            _result = result;
            _completed = true;
            _continueAction();
        }
        public void OnCompleted(Action continuation)
        {
            _continueAction = continuation;
        }

        public B GetResult() => _result;
    }
}