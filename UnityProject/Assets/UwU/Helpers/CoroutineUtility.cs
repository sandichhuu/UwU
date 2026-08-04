using UnityEngine;

namespace UwU.Helpers
{
    public class CoroutineUtility : MonoBehaviour
    {
        private static CoroutineUtility instance;

        public static CoroutineUtility Instance
        {
            get
            {
                if (instance == null)
                {
                    var obj = new GameObject("[CoroutineUtility]");
                    instance = obj.AddComponent<CoroutineUtility>();
                    DontDestroyOnLoad(obj);
                }
                return instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            var _ = Instance;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static Coroutine StartCoroutineStatic(System.Collections.IEnumerator routine)
        {
            return Instance.StartCoroutine(routine);
        }
    }
}