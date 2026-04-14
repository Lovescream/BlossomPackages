using System;
using UnityEngine;

namespace Blossom.Presentation.Receive {
    [Serializable]
    public sealed class ReceiveSettings {
        public Camera WorldCamera => worldCamera;
        public float DefaultWorldDepth => defaultWorldDepth;
        public bool ShowAmountText => showAmountText;
        public int ShowAmountTextMinAmount => showAmountTextMinAmount;
        public Vector2 ReferenceResolution => referenceResolution;
        
        [SerializeField] private Camera worldCamera;  // World, Screen 변환 시 사용할 카메라. 비워두면 Camera.main 사용.
        [SerializeField] private float defaultWorldDepth = 10f;
        [SerializeField] private bool showAmountText = true;
        [SerializeField] private int showAmountTextMinAmount = 5;
        [SerializeField] private Vector2 referenceResolution = new(1080f, 1920f);
    }
}