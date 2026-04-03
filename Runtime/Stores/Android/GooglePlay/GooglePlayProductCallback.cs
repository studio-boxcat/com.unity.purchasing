#nullable enable

namespace UnityEngine.Purchasing
{
    class GooglePlayProductCallback
    {
        GooglePlayConfiguration? m_GooglePlayConfiguration;

        public void SetStoreConfiguration(GooglePlayConfiguration configuration)
        {
            m_GooglePlayConfiguration = configuration;
        }

        public void NotifyQueryProductDetailsFailed(int retryCount)
        {
            m_GooglePlayConfiguration?.NotifyQueryProductDetailsFailed(retryCount);
        }
    }
}
