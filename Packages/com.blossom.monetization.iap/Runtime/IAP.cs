namespace Blossom.Monetization.IAP {
    using System;
    using UnityEngine.Purchasing;
    using Internal;
    
    public static class IAP {

        #region Properties

        /// <summary>
        /// IAP 시스템이 완전히 초기화되었는지 여부.
        /// </summary>
        public static bool IsInitialized => IAPSystem.IsInitialized;

        /// <summary>
        /// IAP 시스템이 현재 초기화 진행 중인지 여부.
        /// <para/>
        /// 초기화 중에는 중복 초기화를 방지하기 위해 추가 초기화 요청이 무시될 수 있음.
        /// </summary>
        public static bool IsInitializing => IAPSystem.IsInitializing;

        /// <summary>
        /// 현재 사용 중인 IAP 설정.
        /// </summary>
        public static IAPSettingsSO Settings => IAPSystem.Settings;

        #endregion

        #region Events

        /// <summary>
        /// IAP 초기화가 완료되었을 때 호출되는 이벤트.
        /// </summary>
        public static event Action OnInitialized {
            add => IAPSystem.OnInitialized += value;
            remove => IAPSystem.OnInitialized -= value;
        }

        /// <summary>
        /// 구매 결과가 확정되었을 때 호출되는 이벤트.
        /// <para/>
        /// 성공/실패 여부, 대상 상품, 실패 사유 등을 <see cref="IAPPurchaseEventArgs"/>로 전달한다.
        /// </summary>
        public static event Action<IAPPurchaseEventArgs> OnPurchaseResult {
            add => IAPSystem.OnPurchaseResult += value;
            remove => IAPSystem.OnPurchaseResult -= value;
        }

        /// <summary>
        /// 구매 또는 복원 과정에서 로딩 UI 표시 여부가 변경될 때 호출되는 이벤트.
        /// <para/>
        /// true이면 로딩 표시, false이면 로딩 해제를 의미한다.
        /// </summary>
        public static event Action<bool> OnLoadingRequested {
            add => IAPSystem.OnLoadingRequested += value;
            remove => IAPSystem.OnLoadingRequested -= value;
        }

        /// <summary>
        /// 복원 요청이 성공적으로 완료되었을 때 호출되는 이벤트.
        /// </summary>
        public static event Action OnRestoreCompleted {
            add => IAPSystem.OnRestoreCompleted += value;
            remove => IAPSystem.OnRestoreCompleted -= value;
        }

        /// <summary>
        /// 복원 요청이 실패했을 때 호출되는 이벤트.
        /// </summary>
        public static event Action OnRestoreFailed {
            add => IAPSystem.OnRestoreFailed += value;
            remove => IAPSystem.OnRestoreFailed -= value;
        }

        #endregion

        #region Initialize

        /// <summary>
        /// IAP 시스템을 초기화한다.
        /// <para/>
        /// 설정 에셋은 <see cref="Settings"/>에서 자동 로드되며,
        /// 구매 기록 저장소와 영수증 검증에 필요한 Tangle 데이터를 함께 전달해야 한다.
        /// </summary>
        /// <param name="storage">구매 이력 저장소 구현체</param>
        /// <param name="googleTangle">Google 영수증 검증용 Tangle 데이터</param>
        /// <param name="appleTangle">Apple 영수증 검증용 Tangle 데이터</param>
        public static void Initialize(IIAPPurchaseStorage storage, byte[] googleTangle, byte[] appleTangle) {
            Initialize(null, storage, googleTangle, appleTangle);
        }

        /// <summary>
        /// IAP 시스템을 초기화한다.
        /// <para/>
        /// 명시적으로 전달한 설정이 우선 사용되며, null이면 SettingsLoader를 통해 자동 로드된다.
        /// </summary>
        /// <param name="settings">사용할 IAP 설정 에셋. null이면 자동 로드.</param>
        /// <param name="storage">구매 이력 저장소 구현체</param>
        /// <param name="googleTangle">Google 영수증 검증용 Tangle 데이터</param>
        /// <param name="appleTangle">Apple 영수증 검증용 Tangle 데이터</param>
        public static void Initialize(IAPSettingsSO settings, IIAPPurchaseStorage storage, byte[] googleTangle, byte[] appleTangle) {
            IAPSystem.Initialize(settings, storage, googleTangle, appleTangle);
        }

        #endregion

        #region Purchase

        /// <summary>
        /// 상품 Key를 기준으로 구매를 요청한다.
        /// <para/>
        /// 실제 스토어에 전달되는 값은 설정에 등록된 플랫폼별 Product ID이다.
        /// </summary>
        /// <param name="key">구매할 상품의 Key</param>
        public static void Purchase(string key) => IAPSystem.Purchase(key);

        /// <summary>
        /// 스토어 복원을 요청한다.
        /// <para/>
        /// 주로 iOS 환경에서 사용되며, 결과는 <see cref="OnRestoreCompleted"/> 또는
        /// <see cref="OnRestoreFailed"/>를 통해 전달된다.
        /// </summary>
        public static void Restore() => IAPSystem.Restore();

        #endregion

        #region Definition / Product

        /// <summary>
        /// 상품 Key를 기준으로 등록된 상품 정의를 조회한다.
        /// </summary>
        /// <param name="key">상품 Key</param>
        /// <returns>대응되는 상품 정의. 없으면 null.</returns>
        public static IAPDefinition GetDefinitionByKey(string key) => IAPSystem.GetDefinitionByKey(key);

        /// <summary>
        /// 스토어 Product ID를 기준으로 등록된 상품 정의를 조회한다.
        /// </summary>
        /// <param name="productId">스토어 Product ID</param>
        /// <returns>대응되는 상품 정의. 없으면 null.</returns>
        public static IAPDefinition GetDefinitionById(string productId) => IAPSystem.GetDefinitionById(productId);

        /// <summary>
        /// 상품 Key를 기준으로 Unity IAP의 <see cref="Product"/> 객체를 조회한다.
        /// </summary>
        /// <param name="key">상품 Key</param>
        /// <returns>대응되는 Product 객체. 없으면 null.</returns>
        public static Product GetProductByKey(string key) => IAPSystem.GetProductByKey(key);

        /// <summary>
        /// 상품 Key를 기준으로 표시용 상품 정보를 조회한다.
        /// <para/>
        /// 가격, 통화 코드, receipt 보유 여부 등의 표시용 데이터가 포함된다.
        /// </summary>
        /// <param name="key">상품 Key</param>
        /// <returns>상품 정보. 찾지 못한 경우 기본값 객체 반환.</returns>
        public static IAPProductInfo GetInfo(string key) => IAPSystem.GetInfo(key);

        #endregion

        #region Tag

        /// <summary>
        /// 태그 이름을 기준으로 대응되는 <see cref="IAPTagId"/>를 조회한다.
        /// </summary>
        /// <param name="tagName">조회할 태그 이름</param>
        /// <param name="tagId">조회 성공 시 대응되는 태그 ID</param>
        /// <returns>조회 성공 시 true, 실패 시 false.</returns>
        public static bool TryGetTagId(string tagName, out IAPTagId tagId) => IAPSystem.TryGetTagId(tagName, out tagId);

        #endregion

        #region Purchase State

        /// <summary>
        /// 특정 상품이 현재 구매된 상태인지 확인한다.
        /// <para/>
        /// 소모성 상품은 로컬 구매 기록을 기준으로 확인하며,
        /// 비소모성/구독형 상품은 receipt 및 구독 상태를 기준으로 확인한다.
        /// </summary>
        /// <param name="key">확인할 상품 Key</param>
        /// <param name="requireValidation">
        /// true이면 영수증 검증까지 수행한다.
        /// false이면 receipt 존재 여부만으로 판단한다.
        /// </param>
        /// <returns>구매 상태이면 true, 아니면 false.</returns>
        public static bool IsPurchased(string key, bool requireValidation = true) =>
            IAPSystem.IsPurchased(key, requireValidation);

        /// <summary>
        /// 특정 태그에 속한 상품들 중 하나라도 구매 상태인지 확인한다.
        /// </summary>
        /// <param name="tagId">확인할 태그 ID</param>
        /// <param name="requireValidation">
        /// true이면 영수증 검증까지 수행한다.
        /// false이면 receipt 존재 여부만으로 판단한다.
        /// </param>
        /// <returns>해당 태그의 상품 중 하나라도 구매 상태이면 true.</returns>
        public static bool IsPurchasedByTag(IAPTagId tagId, bool requireValidation = true) =>
            IAPSystem.IsPurchasedByTag(tagId, requireValidation);

        /// <summary>
        /// 특정 태그 이름에 속한 상품들 중 하나라도 구매 상태인지 확인한다.
        /// </summary>
        /// <param name="tagName">확인할 태그 이름</param>
        /// <param name="requireValidation">
        /// true이면 영수증 검증까지 수행한다.
        /// false이면 receipt 존재 여부만으로 판단한다.
        /// </param>
        /// <returns>해당 태그의 상품 중 하나라도 구매 상태이면 true.</returns>
        public static bool IsPurchasedByTag(string tagName, bool requireValidation = true) =>
            IAPSystem.IsPurchasedByTag(tagName, requireValidation);

        /// <summary>
        /// 특정 상품이 현재 구독 상태인지 확인한다.
        /// <para/>
        /// 구독형 상품에만 의미가 있으며, 구독형이 아닌 상품은 false를 반환한다.
        /// </summary>
        /// <param name="key">상품 Key</param>
        /// <returns>현재 구독 중이면 true, 아니면 false.</returns>
        public static bool IsSubscription(string key) => IAPSystem.IsSubscription(key);

        #endregion

        #region Price

        /// <summary>
        /// 상품의 로컬 가격 문자열을 반환한다.
        /// <para/>
        /// 예: "1.99", "5,900"
        /// </summary>
        /// <param name="key">상품 Key</param>
        /// <param name="removeCurrencyCode">
        /// true이면 통화 기호/통화 심볼을 제거한 문자열을 반환한다.
        /// false이면 스토어 가격 문자열 그대로 사용한다.
        /// </param>
        /// <returns>가격 문자열. 상품을 찾지 못하면 빈 문자열.</returns>
        public static string GetLocalPrice(string key, bool removeCurrencyCode = true) =>
            IAPSystem.GetLocalPrice(key, removeCurrencyCode);

        #endregion

    }
}