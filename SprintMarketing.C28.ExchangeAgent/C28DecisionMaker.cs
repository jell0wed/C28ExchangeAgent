using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Microsoft.Exchange.Data.Transport;
using Microsoft.Exchange.Data.Transport.Email;
using SprintMarketing.C28.ExchangeAgent.API.Models;
using Microsoft.Exchange.Data.Mime;

namespace SprintMarketing.C28.ExchangeAgent
{
    class C28DecisionMaker
    {
        public C28ExchangeData exchangeData { get; }

        public C28DecisionMaker(C28ExchangeData dat)
        {
            this.exchangeData = dat;
        }

        public bool isOutbound(MailItem msg) {
            Header directionality = msg.Message.MimeDocument.RootPart.Headers.FindFirst("X-MS-Exchange-Organization-MessageDirectionality");
            if (directionality == null)
            {
                return false;
            }
            else {
                return directionality.Value.ToLower() == "originating";
            }
        }

        public bool shouldBeHandledByC28(MailItem msg)
        {
            RoutingAddress fromAddr = msg.FromAddress;
            if (!this.exchangeData.hasDomain(msg.FromAddress.DomainPart))
            {
                C28Logger.Debug(C28Logger.C28LoggerType.AGENT, String.Format("Sender '{0}' does not belong to any listed domain. Ignoring.", fromAddr.ToString()));
                return false;
            }

            C28ExchangeDomain domain = this.exchangeData.getDomain(fromAddr.DomainPart);
            if (domain == null)
            {
                return false;
            } else if (domain.isEmailExcluded(fromAddr.ToString()))
            {
                C28Logger.Debug(C28Logger.C28LoggerType.AGENT, String.Format("Sender '{0}' is set to be excluded. Ignoring.", fromAddr.ToString()));
                return false;
            }

            return true;
        }
        
    }
}
