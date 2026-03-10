using System.Collections.Generic;

namespace Blossom.Monetization.IAP {
    public interface IIAPPurchaseStorage {
        /// <summary>
        /// 저장소가 가진 모든 구매 기록을 로드.
        /// </summary>
        IReadOnlyList<IAPPurchaseInfo> LoadAll();
        
        /// <summary>
        /// 새 구매 기록 추가.
        /// </summary>
        void Add(IAPPurchaseInfo info);

        /// <summary>
        /// 거래 취소 반영. 가능하면 transactionId를 기준으로, 없으면 productId 기준 fallback.
        /// </summary>
        void MarkCanceled(string productId, string transactionId, long cancelDate);
        
        /// <summary>
        /// 전체 저장.
        /// </summary>
        void Save();
    }
}