using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace SprintMarketing.C28.ExchangeAgent.api.models
{
    class C28ExchangeClient
    {
        [JsonProperty(PropertyName = "id")]
        public int id { get; set; }
        
        [JsonProperty(PropertyName = "api_key")]
        public string api_key { get; set; }
        
        [JsonProperty(PropertyName = "same_organization_action")]
        public string same_organization_action { get; set; }
        
        [JsonProperty(PropertyName = "group_id")]
        public int group_id { get; set; }
        
    }
}
