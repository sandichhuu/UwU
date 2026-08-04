namespace UwU.Helpers
{
    using System;
    using System.Collections;
    using UnityEngine;
    using UwU.Common;

    public static class CoroutineHelper
    {
        public class CoroutineTask<T> : IEnumerator
        {
            public T Result { get; private set; }
            public bool IsDone { get; private set; }

            public object Current => this.coroutine.Current;

            private readonly IEnumerator coroutine;
            private readonly MonoBehaviour owner;

            public CoroutineTask(MonoBehaviour owner, IEnumerator coroutine)
            {
                this.owner = owner;
                this.coroutine = coroutine;
            }

            public Coroutine Start()
            {
                return this.owner.StartCoroutine(RunWrapper());
            }

            private IEnumerator RunWrapper()
            {
                while (this.coroutine.MoveNext())
                {
                    yield return this.coroutine.Current;

                    if (this.coroutine.Current is T finalResult)
                    {
                        this.Result = finalResult;
                    }
                }

                this.IsDone = true;
            }

            public bool MoveNext()
            {
                if (!this.IsDone && this.owner != null)
                {
                    return !this.IsDone;
                }
                return false;
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }
        }

        public static CoroutineTask<T> Start<T>(IEnumerator coroutine)
        {
            var task = new CoroutineTask<T>(CoroutineUtility.Instance, coroutine);
            task.Start();
            return task;
        }
    }
}