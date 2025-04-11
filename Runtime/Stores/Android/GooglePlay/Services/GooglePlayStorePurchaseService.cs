namespace UnityEngine.Purchasing
{
    class GooglePlayStorePurchaseService
    {
        readonly GooglePlayStoreService m_GooglePlayStoreService;
        internal GooglePlayStorePurchaseService(GooglePlayStoreService googlePlayStoreService)
        {
            m_GooglePlayStoreService = googlePlayStoreService;
        }

        public void Purchase(ProductDefinition product)
        {
            m_GooglePlayStoreService.Purchase(product);
        }
    }
}
