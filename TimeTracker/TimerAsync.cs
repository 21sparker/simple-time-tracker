using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TimeTracker
{

    public class TimerAsync : IDisposable
    {
        private ISubscriber _subscriber;
        private int _milliseconds;
        private CancellationTokenSource _cts;

        public TimerAsync(int milliseconds = 1000)
        {
            _milliseconds = milliseconds;
        }

        public async void StartTimer()
        {
            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;

            while (true)
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(_milliseconds);
                SendUpdate();
            }
        }

        public void SendUpdate()
        {
            if (_subscriber != null)
                _subscriber.HandleUpdate(_milliseconds);
        }

        public void Subscribe(ISubscriber subscriber)
        {
            if (_subscriber != null)
            {
                throw new System.InvalidOperationException("Only one subscriber permitted at a time.");
            }

            _subscriber = subscriber;
        }

        public void Unsubscribe(ISubscriber subscriber)
        {
            if (_subscriber == subscriber)
            {
                _subscriber = null;
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
        }
    }


    public interface ISubscriber
    {
        void HandleUpdate(object obj);
    }

    public class TimerObject : ISubscriber
    {
        private Action<object> _action;

        public TimerObject(Action<object> action)
        {
            _action = action;
        }

        public void HandleUpdate(object obj)
        {
            _action(obj);
        }
    }



}
