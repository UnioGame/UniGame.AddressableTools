using System;
using UniModules.UniCore.Runtime.Rx.Extensions;
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
            _source = source;
            _lifeTime = lifeTime;
            _useDefault = useDefault;
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
            
            var disposable = _predicate == null ? 
                _source.Subscribe(new AwaitFirst(this, observer, cancel).AddTo(_lifeTime)) : 
                _source.Subscribe(new AwaitFirst_(this, observer, cancel).AddTo(_lifeTime));
            _lifeTime.AddDispose(disposable);
            
            return Disposable.Empty;
        }

        class AwaitFirst : OperatorObserverBase<T, T>
        {
            readonly AwaitFirstObservable<T> parent;
            bool notPublished;

            public AwaitFirst(AwaitFirstObservable<T> parent, IObserver<T> observer, IDisposable cancel) : base(observer, cancel)
            {
                this.parent = parent;
                notPublished = true;
            }

            public override void OnNext(T value)
            {
                if (notPublished)
                {
                    notPublished = false;
                    observer.OnNext(value);
                    observer.OnCompleted();
                }
            }

            public override void OnError(Exception error)
            {
                observer.OnError(error);
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
                        observer.OnError(new InvalidOperationException("sequence is empty")); 
                    }
                    else
                    {
                        observer.OnCompleted();
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
                notPublished = true;
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
                        observer.OnError(ex);
                        return;
                    }

                    if (isPassed)
                    {
                        notPublished = false;
                        observer.OnNext(value);
                        observer.OnCompleted();
                    }
                }
            }

            public override void OnError(Exception error)
            {
                observer.OnError(error);
            }

            public override void OnCompleted()
            {
                if (parent._useDefault)
                {
                    if (notPublished)
                    {
                        observer.OnNext(default(T));
                    }
                    observer.OnCompleted();
                }
                else
                {
                    if (notPublished)
                    {
                        observer.OnError(new InvalidOperationException("sequence is empty"));
                    }
                    else
                    {
                        observer.OnCompleted();
                    }
                }
            }
        }
    }
}
