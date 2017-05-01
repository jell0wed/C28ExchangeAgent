using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SprintMarketing.C28.ExchangeAgent.API.Models
{
    public class C28ExchangeRecipientExclusion
    {
        [JsonProperty(PropertyName = "rcpt_pattern")]
        public String rcpt_pattern { get; private set; }
    }
}
