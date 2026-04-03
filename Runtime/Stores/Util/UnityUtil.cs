using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.Assertions;

namespace UnityEngine.Purchasing
{
    [HideInInspector]
    [AddComponentMenu("")]
    internal class UnityUtil : MonoBehaviour
    {
        private static UnityUtil s_Instance;
        public static void Init()
        {
            if (s_Instance) return;
            var gameObject = new GameObject("IAPUtil");
            DontDestroyOnLoad(gameObject);
            gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            s_Instance = gameObject.AddComponent<UnityUtil>();
        }

        private static readonly List<Action> s_Callbacks = new List<Action>();

        private static volatile bool s_CallbacksPending;

        public static void RunOnMainThread(Action runnable)
        {
            Assert.IsTrue(s_Instance, "UnityUtil not initialized. Call UnityUtil.Init() before using RunOnMainThread.");

            lock (s_Callbacks)
            {
                s_Callbacks.Add(runnable);
                s_CallbacksPending = true;
            }
        }

        private void Update()
        {
            if (!s_CallbacksPending)
            {
                return;
            }
            // We copy our actions to another array to avoid
            // locking the queue whilst we process them.
            Action[] copy;
            lock (s_Callbacks)
            {
                if (s_Callbacks.Count == 0)
                {
                    return;
                }

                copy = new Action[s_Callbacks.Count];
                s_Callbacks.CopyTo(copy);
                s_Callbacks.Clear();
                s_CallbacksPending = false;
            }

            foreach (var action in copy)
            {
                action();
            }
        }

        private static readonly List<Action<bool>> pauseListeners = new();
        public static void AddPauseListener(Action<bool> runnable)
        {
            Assert.IsTrue(s_Instance, "UnityUtil not initialized. Call UnityUtil.Init() before using AddPauseListener.");

            pauseListeners.Add(runnable);
        }

        public void OnApplicationPause(bool paused)
        {
            foreach (var listener in pauseListeners)
                listener(paused);
        }

        [Conditional("DEBUG")]
        public static void LogWarning(string message) => Debug.unityLogger.LogWarning("Unity IAP", message);
        [Conditional("DEBUG")]
        public static void LogWarning(string format, params object[] args) => Debug.unityLogger.LogFormat(LogType.Warning, format, args);
        public static void LogError(string message) => Debug.unityLogger.LogError("Unity IAP", message);
        public static void LogError(string format, params object[] args) => Debug.unityLogger.LogFormat(LogType.Error, format, args);
        public static void LogException(Exception exception) => Debug.unityLogger.LogException(exception);
    }
}
