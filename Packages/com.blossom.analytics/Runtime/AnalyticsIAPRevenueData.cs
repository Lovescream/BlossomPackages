using System;
using System.Collections.Generic;
using UnityEngine.Purchasing;

namespace Blossom.Analytics {
    
    [Serializable]
    public sealed class AnalyticsIAPRevenueData {

        #region Properties

        public string EventName { get; }
        public string StoreKey { get; }
        public Product Product { get; }
        public string Revenue { get; }
        public string ProductId { get; }
        public Dictionary<string, object> Attributes { get; }

        #endregion
        
        #region Constructor

        public AnalyticsIAPRevenueData(string eventName, string storeKey, Product product, string revenue, Dictionary<string, object> attributes = null) {
            EventName = eventName ?? string.Empty;
            StoreKey = storeKey ?? string.Empty;
            Product = product;
            Revenue = revenue ?? string.Empty;
            ProductId = Product?.definition.id ?? string.Empty;
            Attributes = attributes ?? new();
        }

        #endregion

        #region Builder

        public sealed class Builder {

            #region Properties

            public string EventName { get; private set; }
            public string StoreKey { get; private set; }
            public Product Product { get; private set; }
            public string Revenue { get; private set; }
            public Dictionary<string, object> Attributes { get; private set; }

            #endregion

            #region Constructor

            public Builder(string eventName, string storeKey, Product product) {
                EventName = eventName ?? string.Empty;
                StoreKey = storeKey ?? string.Empty;
                Product = product;
                Attributes = new();
            }

            #endregion

            #region Set

            public Builder SetRevenue(string revenue) {
                Revenue = revenue ?? string.Empty;
                return this;
            }
            
            public Builder AddAttribute(string key, object value) {
                if (string.IsNullOrWhiteSpace(key)) return this;
                Attributes[key] = value;
                return this;
            }

            public Builder SetAttributes(Dictionary<string, object> attributes) {
                Attributes = attributes;
                return this;
            }

            #endregion

            #region Build

            public AnalyticsIAPRevenueData Build() {
                return new(EventName, StoreKey, Product, Revenue, Attributes);
            }

            #endregion

        }

        #endregion

    }
}