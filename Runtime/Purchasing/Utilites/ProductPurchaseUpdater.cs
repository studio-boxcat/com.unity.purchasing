namespace UnityEngine.Purchasing
{
    internal static class ProductPurchaseUpdater
    {
        internal static void UpdateProductReceiptAndTransactionID(Product product, string receipt, string transactionId, AppStore store)
        {
            product.receipt = UnifiedReceiptFormatter.FormatUnifiedReceipt(receipt, transactionId, store);
            product.transactionID = transactionId;
        }
    }
}
