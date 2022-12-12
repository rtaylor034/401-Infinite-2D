using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;



public class CustomTask
{
    public CustomAwaiter Awaiter { get; private set; }
    public CustomTask()
    {
        Awaiter = new CustomAwaiter();
    }

    public void Resolve()
    {
        Awaiter.Resolve();
    }

    public CustomAwaiter GetAwaiter() => Awaiter;

    public class CustomAwaiter : INotifyCompletion
    {
        private bool _completed;
        public bool IsCompleted => _completed;
        private Action _continueAction;

        public CustomAwaiter()
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

public class CustomTask<T>
{
    public CustomAwaiter<T> Awaiter { get; private set; }
    public CustomTask()
    {
        Awaiter = new CustomAwaiter<T>();
    }

    public void Resolve(T result)
    {
        Awaiter.Resolve(result);
    }

    public CustomAwaiter<T> GetAwaiter() => Awaiter;

    public class CustomAwaiter<B> : INotifyCompletion
    {
        private bool _completed;
        private B _result;
        public bool IsCompleted => _completed;
        private Action _continueAction;

        public CustomAwaiter()
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