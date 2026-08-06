#if UWU_UNITASK_SUPPORT
namespace UwU
{
    using Cysharp.Threading.Tasks;
    using System.Threading;
    using Unity.VisualScripting;
    using static UwU.Helpers.CoroutineHelper;

    public static class CoroutineTaskAsyncExtensions
    {
        public static async UniTask<T> ToUniTask<T>(this CoroutineTask<T> task, CancellationToken cancellationToken = default)
        {
            while (!task.IsDone)
            {
                if (cancellationToken.IsCancellationRequested)
                    return default;

                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            return task.Result;
        }

        public static UniTask<T>.Awaiter GetAwaiter<T>(this CoroutineTask<T> task)
        {
            return task.ToUniTask().GetAwaiter();
        }
    }
}
#endif