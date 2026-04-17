namespace Blossom.Monetization.IAP {
    using UnityEngine.Purchasing;

    /// <summary>
    /// 표시용 IAP 상품 정보.
    /// <para/>
    /// 가격, 통화 코드, 구매 여부 등 자주 사용하는 데이터를 담는다.
    /// </summary>
    public class IAPProductInfo {

        /// <summary>
        /// 상품 타입.
        /// </summary>
        public ProductType ProductType { get; }

        /// <summary>
        /// Unity IAP Product 객체.
        /// </summary>
        public Product Product { get; }

        /// <summary>
        /// receipt를 기준으로 구매된 상태인지 여부.
        /// </summary>
        public bool IsPurchased { get; }

        /// <summary>
        /// 구독 상태 여부.
        /// </summary>
        public bool IsSubscribed { get; }

        /// <summary>
        /// 로컬라이즈된 가격 값.
        /// </summary>
        public decimal Price { get; }

        /// <summary>
        /// ISO 통화 코드.
        /// </summary>
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
