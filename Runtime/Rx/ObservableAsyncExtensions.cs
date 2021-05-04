using System;
using Cysharp.Threading.Tasks;
using UniModules.UniCore.Runtime.ObjectPool.Runtime;
using UniModules.UniCore.Runtime.ObjectPool.Runtime.Extensions;
using UniModules.UniGame.Core.Runtime.DataFlow.Interfaces;

public static class ObservableAsyncExtensions 
{

    public static async UniTask<TValue> AwaitFirstAsync<TValue>(this IObservable<TValue> value, ILifeTime lifeTime,
        Func<TValue, bool> predicate = null)
    {
        var firstAwaiter = ClassPool.Spawn<AwaitFirstAsyncOperation<TValue>>();
        var result = await firstAwaiter.AwaitFirstAsync(value, lifeTime, predicate);
        firstAwaiter.Despawn();
        return result;
    }
    
}
