using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SprintMarketing.C28.ExchangeAgent.API.Models
{
    public class C28ExchangeDomain
    {
        [JsonProperty(PropertyName = "domain")]
        public String domain { get; private set; }

        [JsonProperty(PropertyName = "connector_override")]
        public String connector_override { get; private set; }

        [JsonProperty(PropertyName = "Exchange_Exclusions")]
        public List<C28ExchangeExclusion> exclusions { get; private set; }

        public bool isEmailExcluded(String email) {
            return this.exclusions.Any(e => e.sender_address.ToLower() == email.ToLower());
        }
    }
}
