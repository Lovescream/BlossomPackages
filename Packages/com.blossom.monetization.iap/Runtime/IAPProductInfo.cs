using UnityEngine.Purchasing;

namespace Blossom.Monetization.IAP {
    public class IAPProductInfo {

        public ProductType ProductType { get; }
        public Product Product { get; }
        public bool IsPurchased { get; }
        public bool IsSubscribed { get; }
        public decimal Price { get; }
        public string ISOCurrencyCode { get; }

        public IAPProductInfo() {
            Price = 0.00m;
            ISOCurrencyCode = "USD";
            IsPurchased = false;
            IsSubscribed = false;
        }

        public IAPProductInfo(Product product) {
            Product = product;
            ProductType = product.definition.type;
            IsPurchased = product.hasReceipt;
            Price = product.metadata.localizedPrice;
            ISOCurrencyCode = product.metadata.isoCurrencyCode;
        }
        
    }
}
