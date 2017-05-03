using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

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

        [JsonProperty(PropertyName = "Exchange_Recipient_Exclusions")]
        public List<C28ExchangeRecipientExclusion> rcpts_exclusions { get; private set; }

        [JsonProperty(PropertyName = "same_domain_action")]
        public String same_domain_action { get; private set; }
        
        public bool isEmailExcluded(string email) {
            email = email.ToLower();
            List<Regex> senderExclusionRegexes = this.exclusions.Select(e => new Regex(e.sender_address)).ToList();
            return senderExclusionRegexes.Any(e => e.IsMatch(email));
        }

        public bool isRecipientExcluded(string rcpt) {
            rcpt = rcpt.ToLower();
            List<Regex> rcptExclusionRegexes = this.rcpts_exclusions.Select(de => new Regex(de.rcpt_pattern)).ToList();
            return rcptExclusionRegexes.Any(re => re.IsMatch(rcpt));
        }
    }
}
