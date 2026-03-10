using System;
using UnityEngine;

namespace Blossom.Core.UI {
    public class UIPanel : UICanvas {

        /// <summary>
        /// 동일 타입 중복 열기 허용 여부.
        /// </summary>
        public virtual bool AllowDuplicate => false;
        
        /// <summary>
        /// 닫힐 때 호출되고, 닫히면 초기화됨.
        /// </summary>
        public event Action OnClosed;

        protected override void OnInitialize() {
            base.OnInitialize();

            _rect = this.gameObject.FindChild<RectTransform>("Panel");
        }

        public virtual void Close() {
            OnClosed?.Invoke();
            OnClosed = null;
            UI.ClosePanel(this);
        }
        
    }
}