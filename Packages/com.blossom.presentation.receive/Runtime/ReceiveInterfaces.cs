namespace Blossom.Presentation.Receive {
    using System.Collections.Generic;
    using UnityEngine;
    
    /// <summary>
    /// 실제 반영 핸들러.
    /// Currency 증가, Life 증가, Item 증가 등의 책임을 이 곳에서 구현. 
    /// </summary>
    public interface IReceiveRewardHandler {
        void Apply(ReceiveKey key, int amount);
        void ApplyDisplay(ReceiveKey key, int amount);
        void SyncDisplay(ReceiveKey key);
    }

    /// <summary>
    /// 보상이 날아갈 목표 지점을 찾는 인터페이스.
    /// </summary>
    public interface IReceiveTargetResolver {
        bool TryResolve(ReceiveKey key, out IReceiveTarget target);
    }

    /// <summary>
    /// 보상 도착 지점 인터페이스.
    /// </summary>
    public interface IReceiveTarget {
        ReceiveKey Key { get; }
        ReceiveSpace Space { get; }
        Vector3 Position { get; }
        void NotifyReceived(ReceiveKey key, int amount);
    }

    /// <summary>
    /// 총 수량을 여러 개의 연출 단위로 나누는 인터페이스.
    /// </summary>
    public interface IReceiveAmountSplitter {
        List<int> Split(ReceiveKey key, int amount);
    }

    /// <summary>
    /// Key에 대응하는 아이콘을 제공하는 인터페이스.
    /// </summary>
    public interface IReceiveIconProvider {
        Sprite GetIcon(ReceiveKey key);
    }

    public interface IReceiveAmountTextFormatter {
        string Format(ReceiveKey key, int amount);
    }
    
    /// <summary>
    /// 개별 보상 표시 오브젝트.
    /// </summary>
    public interface IReceiveObject {
        Transform Transform { get; }
        ReceiveSpace Space { get; }
        void SetIcon(Sprite sprite);
        void SetAmountText(string text);
        void SetPosition(Vector3 position);
        void Release();
    }

    /// <summary>
    /// 보상 표시 오브젝트 생성 인터페이스. UI / World 표시 방식을 교체 가능하도록 함.
    /// </summary>
    public interface IReceiveSpawner {
        bool Supports(ReceiveSpace space);
        IReceiveObject Spawn(ReceiveSpawnContext context);
    }

    /// <summary>
    /// 보상 이동 연출 인터페이스.
    /// </summary>
    public interface IReceiveEffect {
        void Play(ReceiveEffectContext context);
    }

    /// <summary>
    /// 커스텀 연출 선택 인터페이스. Key나 Request 조건에 따라 다른 연출을 고를 수 있도록 함.
    /// </summary>
    public interface IReceiveEffectSelector {
        IReceiveEffect Select(ReceiveRequest request);
    }
    
}