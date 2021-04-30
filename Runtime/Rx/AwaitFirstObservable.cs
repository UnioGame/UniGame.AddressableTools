using System;
using UniModules.UniGame.Core.Runtime.DataFlow.Interfaces;
using UniRx;
using UniRx.Operators;

namespace UniModules.UniGame.CoreModules.UniGame.AddressableTools.Runtime.Rx
{
    public class AwaitFirstObservable<T> : OperatorObservableBase<T>
    {
        readonly IObservable<T> _source;
        private readonly ILifeTime _lifeTime;
        readonly bool _useDefault;
        readonly Func<T, bool> _predicate;

        public AwaitFirstObservable(IObservable<T> source,ILifeTime lifeTime, bool useDefault)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this._source = source;
            _lifeTime = lifeTime;
            this._useDefault = useDefault;
        }

        public AwaitFirstObservable(IObservable<T> source,ILifeTime lifeTime, Func<T, bool> predicate, bool useDefault)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            _source = source;
            _lifeTime = lifeTime;
            _predicate = predicate;
            _useDefault = useDefault;
        }

        protected override IDisposable SubscribeCore(IObserver<T> observer, IDisposable cancel)
        {
            if (_predicate == null)
            {
                return _source.Subscribe(new AwaitFirst(this, observer, cancel));
            }
            else
            {
                return _source.Subscribe(new AwaitFirst(this, observer, cancel));
            }
        }

        class AwaitFirst : OperatorObserverBase<T, T>
        {
            readonly AwaitFirstObservable<T> parent;
            bool notPublished;

            public AwaitFirst(AwaitFirstObservable<T> parent, IObserver<T> observer, IDisposable cancel) : base(observer, cancel)
            {
                this.parent = parent;
                this.notPublished = true;
            }

            public override void OnNext(T value)
            {
                if (notPublished)
                {
                    notPublished = false;
                    observer.OnNext(value);
                    try { observer.OnCompleted(); }
                    finally { Dispose(); }
                    return;
                }
            }

            public override void OnError(Exception error)
            {
                try { observer.OnError(error); }
                finally { Dispose(); }
            }

            public override void OnCompleted()
            {
                if (parent._useDefault)
                {
                    if (notPublished)
                    {
                        observer.OnNext(default(T));
                    }
                    try { observer.OnCompleted(); }
                    finally { Dispose(); }
                }
                else
                {
                    if (notPublished)
                    {
                        try { observer.OnError(new InvalidOperationException("sequence is empty")); }
                        finally { Dispose(); }
                    }
                    else
                    {
                        try { observer.OnCompleted(); }
                        finally { Dispose(); }
                    }
                }
            }
        }

        // with predicate
        class AwaitFirst_ : OperatorObserverBase<T, T>
        {
            readonly AwaitFirstObservable<T> parent;
            bool notPublished;

            public AwaitFirst_(AwaitFirstObservable<T> parent, IObserver<T> observer, IDisposable cancel) : base(observer, cancel)
            {
                this.parent = parent;
                this.notPublished = true;
            }

            public override void OnNext(T value)
            {
                if (notPublished)
                {
                    bool isPassed;
                    try
                    {
                        isPassed = parent._predicate(value);
                    }
                    catch (Exception ex)
                    {
                        try { observer.OnError(ex); }
                        finally { Dispose(); }
                        return;
                    }

                    if (isPassed)
                    {
                        notPublished = false;
                        observer.OnNext(value);
                        try { observer.OnCompleted(); }
                        finally { Dispose(); }
                    }
                }
            }

            public override void OnError(Exception error)
            {
                try { observer.OnError(error); }
                finally { Dispose(); }
            }

            public override void OnCompleted()
            {
                if (parent._useDefault)
                {
                    if (notPublished)
                    {
                        observer.OnNext(default(T));
                    }
                    try { observer.OnCompleted(); }
                    finally { Dispose(); }
                }
                else
                {
                    if (notPublished)
                    {
                        try { observer.OnError(new InvalidOperationException("sequence is empty")); }
                        finally { Dispose(); }
                    }
                    else
                    {
                        try { observer.OnCompleted(); }
                        finally { Dispose(); }
                    }
                }
            }
        }
    }
}
