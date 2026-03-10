namespace Blossom.Persistence.Akiv.Internal {
    internal interface IAkivData {
        string Key { get; set; }
        void Flush();
        void Clear();
        void OnChanged();
    }
}