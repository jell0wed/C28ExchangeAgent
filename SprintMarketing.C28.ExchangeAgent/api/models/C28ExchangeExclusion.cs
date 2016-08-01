using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SprintMarketing.C28.ExchangeAgent.API.Models
{
    public class C28ExchangeExclusion
    {
        [JsonProperty(PropertyName = "sender_address")]
        public String sender_address { get; private set; }
    }
}
