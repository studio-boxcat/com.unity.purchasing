namespace UnityEngine.Purchasing.Models
{
    class GooglePurchaseStateEnumProvider
    {
        public int Purchased()
        {
            return GooglePurchaseStateEnum.Purchased();
        }

        public int Pending()
        {
            return GooglePurchaseStateEnum.Pending();
        }
    }
}
