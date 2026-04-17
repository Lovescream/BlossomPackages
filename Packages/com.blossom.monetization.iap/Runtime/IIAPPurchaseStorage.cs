namespace Blossom.Monetization.IAP {
    using System.Collections.Generic;
    
    /// <summary>
    /// IAP 구매 이력 저장소 인터페이스.
    /// <para/>
    /// 소모성 상품의 구매 기록, 거래 취소 반영, 영구 저장 등을 담당하는 외부 저장소.
    /// </summary>
    public interface IIAPPurchaseStorage {
        
        /// <summary>
        /// 저장소에 보관된 모든 구매 기록을 로드한다.
        /// </summary>
        /// <returns>구매 기록 목록</returns>
        IReadOnlyList<IAPPurchaseInfo> LoadAll();
        
        /// <summary>
        /// 새로운 구매 기록을 저장소에 추가한다.
        /// </summary>
        /// <param name="info">추가할 구매 기록</param>
        void Add(IAPPurchaseInfo info);

        /// <summary>
        /// 거래 취소를 저장소에 반영한다.
        /// <para/>
        /// 가능하면 transactionId를 기준으로 취소 처리하며,
        /// 찾지 못한 경우 productId 기준 fallback 처리할 수 있다.
        /// </summary>
        /// <param name="productId">대상 상품 ID</param>
        /// <param name="transactionId">거래 ID</param>
        /// <param name="cancelDate">취소 시각 (UTC ticks)</param>
        void MarkCanceled(string productId, string transactionId, long cancelDate);
        
        /// <summary>
        /// 저장소의 현재 상태를 영구 저장한다.
        /// </summary>
        void Save();
        
    }
}