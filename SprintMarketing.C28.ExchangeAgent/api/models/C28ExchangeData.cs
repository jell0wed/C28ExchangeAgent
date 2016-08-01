using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SprintMarketing.C28.ExchangeAgent.api.models;
using SprintMarketing.C28.ExchangeAgent.API.Models;

namespace SprintMarketing.C28.ExchangeAgent.API.Models
{
    class C28ExchangeData
    {
        [JsonProperty(PropertyName = "domains")]
        private List<C28ExchangeDomain> domains = new List<C28ExchangeDomain>();

        [JsonProperty(PropertyName = "client")]
        public C28ExchangeClient currentClient { get; set; }

        public bool hasDomain(String domain) {
            return this.domains.Any(d => d.domain.ToLower() == (domain.ToLower()));
        }

        public C28ExchangeDomain getDomain(String domain) {
            return this.domains.FirstOrDefault(d => d.domain.ToLower() == (domain.ToLower()));
        }

        public int getCount() { return this.domains.Count; }
    }
}
