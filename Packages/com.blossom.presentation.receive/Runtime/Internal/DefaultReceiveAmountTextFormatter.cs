namespace Blossom.Presentation.Receive.Internal {
    internal sealed class DefaultReceiveAmountTextFormatter : IReceiveAmountTextFormatter {

        private readonly ReceiveSettings _settings;

        internal DefaultReceiveAmountTextFormatter(ReceiveSettings settings) {
            _settings = settings;
        }

        public string Format(ReceiveKey key, int amount) {
            if (_settings == null) return string.Empty;
            if (!_settings.ShowAmountText) return string.Empty;
            if (amount < _settings.ShowAmountTextMinAmount) return string.Empty;

            return $"x{amount}";
        }

    }
}