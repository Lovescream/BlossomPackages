namespace Blossom.Analytics {
    using System;
    using System.Collections.Generic;
    
    [Serializable]
    public sealed class AnalyticsEventData {

        #region Properties

        public string EventName { get; }
        public string ParamName { get; }
        public string ParamValue { get; }
        public IReadOnlyDictionary<string, object> Attributes => _attributes;

        #endregion

        #region Fields

        private readonly Dictionary<string, object> _attributes;

        #endregion

        #region Constructor

        public AnalyticsEventData(string eventName, string paramName = null, string paramValue = null,
            IReadOnlyDictionary<string, object> attributes = null) {
            EventName = eventName ?? string.Empty;
            ParamName = paramName ?? string.Empty;
            ParamValue = paramValue ?? string.Empty;
            _attributes = attributes != null
                ? new(attributes)
                : new();
        }

        #endregion

        #region Builder

        public sealed class Builder {

            public string EventName { get; private set; }
            public string ParamName { get; private set; }
            public string ParamValue { get; private set; }
            private readonly Dictionary<string, object> _attributes = new();

            public Builder(string eventName) {
                EventName = eventName ?? string.Empty;
            }

            public Builder SetParamName(string paramName) {
                ParamName = paramName ?? string.Empty;
                return this;
            }

            public Builder SetParamValue(string paramValue) {
                ParamValue = paramValue ?? string.Empty;
                return this;
            }

            public Builder AddAttribute(string key, object value) {
                if (string.IsNullOrWhiteSpace(key)) return this;
                _attributes[key] = value;
                return this;
            }

            public Builder SetAttributes(IReadOnlyDictionary<string, object> attributes) {
                _attributes.Clear();
                if (attributes == null) return this;

                foreach (KeyValuePair<string, object> pair in attributes) {
                    if (string.IsNullOrWhiteSpace(pair.Key)) continue;
                    _attributes[pair.Key] = pair.Value;
                }

                return this;
            }

            public AnalyticsEventData Build() {
                return new(EventName, ParamName, ParamValue, _attributes);
            }

        }

        #endregion

    }
}