using System;
using UnityEngine;

namespace Blossom.Core.UI {
    /// <summary>
    /// UI 전반에서 사용하는 기본 설정값.
    /// </summary>
    [Serializable]
    public class UISettings {
        /// <summary>
        /// 기본 CnavasScaler 기준 해상도.
        /// </summary>
        public Vector2 referenceResolution = new(1080f, 1920f);
        
        /// <summary>
        /// 패널의 기본 정렬 시작 순서. 
        /// </summary>
        public int initialPanelOrder = 10;
        
        /// <summary>
        /// 팝업의 기본 정렬 시작 순서.
        /// </summary>
        public int initialPopupOrder = 100;
        
        /// <summary>
        /// UI 루트 오브젝트 이름.
        /// </summary>
        public string rootName = "@UI_Root";
    }
}