namespace UnityEngine.Purchasing
{
    class UnityActivity
    {
        internal static AndroidJavaObject GetCurrentActivity()
        {
            return AndroidApplication.UnityActivity;
        }
    }
}
