namespace Blossom.Core.Resource {
    public enum ResourceKeyPolicy {
        /// <summary>
        /// Object.name을 Key로 사용. (기본값)
        /// </summary>
        Name = 0,
        /// <summary>
        /// Object.name을 소문자료 변환하여 키로 사용.
        /// </summary>
        NameLower = 1,
    }
}