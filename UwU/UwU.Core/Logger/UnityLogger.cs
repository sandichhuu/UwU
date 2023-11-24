using UnityEngine;

namespace UwU.Core
{
    public class UnityLogger : ILogger
    {
        public void Error(object message)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log(string.Format("<color=red>{0}</color>", message));
#endif
        }

        public void Trace(object message)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log(string.Format("<color=green>{0}</color>", message));
#endif
        }

        public void Warn(object message)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log(string.Format("<color=yellow>{0}</color>", message));
#endif
        }

        [System.Diagnostics.DebuggerHidden]
        public static void Print(string message)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log(string.Format("<color=cyan>{0}</color>", message));
#else
            UnityEngine.Debug.Log(message);
#endif
        }

        [System.Diagnostics.DebuggerHidden]
        public static void Print(bool isOk, string message)
        {
#if UNITY_EDITOR
            var isOkText = isOk ? "<color=#b4ffa1>PASS</color>" : "<color=#FA5B4E>FAIL</color>";
            UnityEngine.Debug.Log(string.Format("[{0}] <color=cyan>{1}</color>", isOkText, message));
#else
            var isOkText = isOk ? "PASS" : "FAIL";
            UnityEngine.Debug.Log(string.Format("[{0}] {1}", isOkText, message));
#endif
        }
    }
}