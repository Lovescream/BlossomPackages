using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Blossom.Core.Resource {
    public static class Res {

        public static bool IsInitialized { get; private set; }

        public static void Initialize() {
            if (IsInitialized) return;

            Load<Sprite>("Sprites");
            Load<GameObject>("Prefabs");
            Load<Material>("Materials");
            Load<AudioClip>("AudioClips");
            Load<Mesh>("Meshes");
            
            IsInitialized = true;
        }
        
        /// <summary>
        /// 지정 경로에서 타입 <typeparamref name="T"/> 리소스를 모두 로드하여 캐시에 저장.
        /// <para/>
        /// 이미 로드된 타입이라도 다시 호출하면 해당 타입 캐시를 덮어씀.
        /// </summary>
        /// <typeparam name="T">UnityEngine.Object 파생 타입 (Sprite, GameObject, Material, ...)</typeparam>
        /// <param name="path">Resources 폴더 기준 상대 경로</param>
        /// <param name="keyPolicy">캐시 키 생성 정책</param>
        /// <param name="keySelector">커스텀 키 지정 함수</param>
        public static void Load<T>(
            string path,
            ResourceKeyPolicy keyPolicy = ResourceKeyPolicy.Name,
            Func<T, string> keySelector = null) where T : Object {
            ResourceManager.Load(path, keyPolicy, keySelector);
        }

        /// <summary>
        /// 캐시에 해당 키가 존재하는지 확인.
        /// </summary>
        /// <param name="key">리소스 키</param>
        /// <typeparam name="T">UnityEngine.Object 파생 타입</typeparam>
        /// <returns>존재하면 true, 그렇지 않으면 false</returns>
        public static bool Contains<T>(string key) where T : Object => ResourceManager.Contains<T>(key);

        /// <summary>
        /// 캐시에서 리소스를 가져옴.
        /// <para/>
        /// 실패 시 null을 반환하며, 에러 로그를 남김.
        /// </summary>
        /// <param name="key">리소스 키</param>
        /// <typeparam name="T">UnityEngine.Object 파생 타입</typeparam>
        /// <returns>찾은 리소스를 반환, 없으면 null</returns>
        public static T Get<T>(string key) where T : Object => ResourceManager.Get<T>(key);

        /// <summary>
        /// 캐시에서 리소스 가져오기를 시도함.
        /// </summary>
        /// <param name="key">리소스 키</param>
        /// <param name="value">찾은 리소스</param>
        /// <typeparam name="T">UnityEngine.Object 파생 타입</typeparam>
        /// <returns>성공 여부</returns>
        public static bool TryGet<T>(string key, out T value) where T : Object =>
            ResourceManager.TryGet<T>(key, out value);

        /// <summary>
        /// 캐시에 로드된 타입 <typeparamref name="T"/>의 모든 리소스 반환.
        /// <para/>
        /// 해당 타입이 아직 로드되지 않았다면 빈 배열 반환.
        /// </summary>
        /// <typeparam name="T">UnityEngine.Object 파생 타입</typeparam>
        /// <returns>로드된 리소스 목록(읽기 전용)</returns>
        public static IReadOnlyList<T> GetAll<T>() where T : Object => ResourceManager.GetAll<T>();

        /// <summary>
        /// 특정 타입 <typeparamref name="T"/>의 캐시를 제거.
        /// </summary>
        /// <typeparam name="T">UnityEngine.Object 파생 타입</typeparam>
        public static void Clear<T>() where T : Object => ResourceManager.Clear<T>();
        
        /// <summary>
        /// 모든 타입의 캐시를 제거.
        /// </summary>
        public static void ClearAll() => ResourceManager.ClearAll();

    }
}