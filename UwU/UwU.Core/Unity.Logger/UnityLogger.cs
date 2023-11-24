using UnityEngine;

namespace UwU.Core
{
    public class UnityLogger : ILogger
    {
        public void Error(object message)
        {
#if UNITY_EDITOR
            Debug.LogWarning(message);
#endif
        }

        public void Trace(object message)
        {
#if UNITY_EDITOR
            Debug.Log("<color=cyan>" + message + "</color>");
#endif
        }

        public void Warn(object message)
        {
#if UNITY_EDITOR
            Debug.LogWarning(message);
#endif
        }
    }
}