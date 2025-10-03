﻿using MuhasibPro.Contracts.CommonServices;
using MuhasibPro.Infrastructure.ViewModels;

namespace MuhasibPro.Services.CommonServices
{
    public class MessageService : IMessageService
    {
        private object _sync = new Object();
        private List<Subscriber> _subscribers = new List<Subscriber>();

        // ContextService integration için eklenen bölüm
        private readonly Dictionary<int, IContextService> _contextServices = new();
        private readonly object _contextLock = new object();

        public void RegisterContext(int contextId, IContextService contextService)
        {
            lock (_contextLock)
            {
                _contextServices[contextId] = contextService;
            }
        }

        public void UnregisterContext(int contextId)
        {
            lock (_contextLock)
            {
                _contextServices.Remove(contextId);
            }
        }

        public void Subscribe<TSender>(object target, Action<TSender, string, object> action) where TSender : class
        {
            Subscribe<TSender, Object>(target, action);
        }

        public void Subscribe<TSender, TArgs>(object target, Action<TSender, string, TArgs> action) where TSender : class
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            lock (_sync)
            {
                var subscriber = _subscribers.Where(r => r.Target == target).FirstOrDefault();
                if (subscriber == null)
                {
                    subscriber = new Subscriber(target);
                    _subscribers.Add(subscriber);
                }
                subscriber.AddSubscription<TSender, TArgs>(action);
            }
        }

        public void Unsubscribe<TSender>(object target) where TSender : class
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            lock (_sync)
            {
                var subscriber = _subscribers.Where(r => r.Target == target).FirstOrDefault();
                if (subscriber != null)
                {
                    subscriber.RemoveSubscription<TSender>();
                    if (subscriber.IsEmpty)
                    {
                        _subscribers.Remove(subscriber);
                    }
                }
            }
        }

        public void Unsubscribe<TSender, TArgs>(object target) where TSender : class
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            lock (_sync)
            {
                var subscriber = _subscribers.Where(r => r.Target == target).FirstOrDefault();
                if (subscriber != null)
                {
                    subscriber.RemoveSubscription<TSender, TArgs>();
                    if (subscriber.IsEmpty)
                    {
                        _subscribers.Remove(subscriber);
                    }
                }
            }
        }

        public void Unsubscribe(object target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            lock (_sync)
            {
                var subscriber = _subscribers.Where(r => r.Target == target).FirstOrDefault();
                if (subscriber != null)
                {
                    _subscribers.Remove(subscriber);
                }
            }
        }

        // Async Send metodu - ContextService ile UI thread'de çalışır
        public async Task SendAsync<TSender, TArgs>(TSender sender, string message, TArgs args) where TSender : class
        {
            if (sender == null)
                throw new ArgumentNullException(nameof(sender));

            var tasks = new List<Task>();
            var subscribers = GetSubscribersSnapshot();

            foreach (var subscriber in subscribers)
            {
                // Avoid sending message to self
                if (subscriber.Target != sender && subscriber.Target != null)
                {
                    // ViewModel ise context'ini al ve UI thread'de çalıştır
                    if (subscriber.Target is ViewModelBase viewModel)
                    {
                        var contextService = viewModel.ContextService;
                        var task = contextService.RunAsync(() =>
                        {
                            subscriber.TryInvoke(sender, message, args);
                        });
                        tasks.Add(task);
                    }
                    else
                    {
                        // Normal invoke
                        subscriber.TryInvoke(sender, message, args);
                    }
                }
            }

            if (tasks.Any())
            {
                await Task.WhenAll(tasks);
            }
        }

        // Orijinal Send metodu - geriye uyumluluk için
        public void Send<TSender, TArgs>(TSender sender, string message, TArgs args) where TSender : class
        {
            if (sender == null)
                throw new ArgumentNullException(nameof(sender));

            foreach (var subscriber in GetSubscribersSnapshot())
            {
                // Avoid sending message to self
                if (subscriber.Target != sender)
                {
                    subscriber.TryInvoke(sender, message, args);
                }
            }
        }

        private Subscriber[] GetSubscribersSnapshot()
        {
            lock (_sync)
            {
                return _subscribers.ToArray();
            }
        }

        class Subscriber
        {
            private WeakReference _reference = null;
            private Dictionary<Type, Subscriptions> _subscriptions;

            public Subscriber(object target)
            {
                _reference = new WeakReference(target);
                _subscriptions = new Dictionary<Type, Subscriptions>();
            }

            public object Target => _reference.Target;

            public bool IsEmpty => _subscriptions.Count == 0;

            public void AddSubscription<TSender, TArgs>(Action<TSender, string, TArgs> action)
            {
                if (!_subscriptions.TryGetValue(typeof(TSender), out Subscriptions subscriptions))
                {
                    subscriptions = new Subscriptions();
                    _subscriptions.Add(typeof(TSender), subscriptions);
                }
                subscriptions.AddSubscription(action);
            }

            public void RemoveSubscription<TSender>()
            {
                _subscriptions.Remove(typeof(TSender));
            }

            public void RemoveSubscription<TSender, TArgs>()
            {
                if (_subscriptions.TryGetValue(typeof(TSender), out Subscriptions subscriptions))
                {
                    subscriptions.RemoveSubscription<TArgs>();
                    if (subscriptions.IsEmpty)
                    {
                        _subscriptions.Remove(typeof(TSender));
                    }
                }
            }

            public void TryInvoke<TArgs>(object sender, string message, TArgs args)
            {
                var target = _reference.Target;
                if (_reference.IsAlive)
                {
                    var senderType = sender.GetType();
                    foreach (var keyValue in _subscriptions.Where(r => r.Key.IsAssignableFrom(senderType)))
                    {
                        var subscriptions = keyValue.Value;
                        subscriptions.TryInvoke(sender, message, args);
                    }
                }
            }
        }

        class Subscriptions
        {
            private Dictionary<Type, Delegate> _subscriptions = null;

            public Subscriptions()
            {
                _subscriptions = new Dictionary<Type, Delegate>();
            }

            public bool IsEmpty => _subscriptions.Count == 0;

            public void AddSubscription<TSender, TArgs>(Action<TSender, string, TArgs> action)
            {
                _subscriptions.Add(typeof(TArgs), action);
            }

            public void RemoveSubscription<TArgs>()
            {
                _subscriptions.Remove(typeof(TArgs));
            }

            public void TryInvoke<TArgs>(object sender, string message, TArgs args)
            {
                var argsType = typeof(TArgs);
                foreach (var keyValue in _subscriptions.Where(r => r.Key.IsAssignableFrom(argsType)))
                {
                    var action = keyValue.Value;
                    action?.DynamicInvoke(sender, message, args);
                }
            }
        }
    }
}
