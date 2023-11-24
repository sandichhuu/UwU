using Collections.Pooled;
using System.Collections.Generic;

namespace UwU.ObserverSystem
{
    using UwU.Core;

    public class Observer
    {
        private readonly TypeId idProvider;
        private readonly Dictionary<long, IList<ISubscriber>> subscribers;

        public Observer(TypeId idProvider)
        {
            this.idProvider = idProvider;
            this.subscribers = new Dictionary<long, IList<ISubscriber>>(16);
        }

        public void Subscribe<Arg>(ISubscriber subscriber)
        {
            var index = this.idProvider.GetId<Arg>();

            if (this.subscribers.ContainsKey(index))
            {
                this.subscribers[index].Add(subscriber);
            }
            else
            {
                this.subscribers.Add(index, new PooledList<ISubscriber>() { subscriber });
            }
        }

        public void Unsubscribe<Arg>(ISubscriber subscriber)
        {
            var index = this.idProvider.GetId<Arg>();

            if (this.subscribers.ContainsKey(index))
            {
                this.subscribers[index].Remove(subscriber);
            }
        }

        public void Notify<Arg>(Arg arg) where Arg : struct
        {
            var index = this.idProvider.GetId<Arg>();

            if (this.subscribers.ContainsKey(index))
            {
                var notifyTargets = this.subscribers[index];
                for (var i = 0; i < notifyTargets.Count; i++)
                {
                    (notifyTargets[i] as IReactOn<Arg>).OnNotify(arg);
                }
            }
        }

        public void Notify<Arg>() where Arg : struct
        {
            Notify<Arg>(default);
        }
    }
}