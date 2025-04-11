#nullable enable

namespace UnityEngine.Purchasing
{
    class GooglePlayProductCallback
    {
        IGooglePlayConfigurationInternal? m_GooglePlayConfigurationInternal;

        public void SetStoreConfiguration(IGooglePlayConfigurationInternal configuration)
        {
            m_GooglePlayConfigurationInternal = configuration;
        }

        public void NotifyQueryProductDetailsFailed(int retryCount)
        {
            m_GooglePlayConfigurationInternal?.NotifyQueryProductDetailsFailed(retryCount);
        }
    }
}
