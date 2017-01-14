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

        public bool shouldBeHandledByC28(MailItem msg)
        {
            try
            {
                Header directionalityHeader = msg.Message.MimeDocument.RootPart.Headers.FindFirst("X-MS-Exchange-Organization-MessageDirectionality");
                C28Logger.Info(C28Logger.C28LoggerType.AGENT, String.Format("Directionality from header is {0}", directionalityHeader.Value));
            }
            catch (ArgumentNullException e) {
                C28Logger.Info(C28Logger.C28LoggerType.AGENT, "Error while getting directionality from header");
            }

            try
            {
                C28Logger.Info(C28Logger.C28LoggerType.AGENT, "deliveryMethod = " + msg.InboundDeliveryMethod.ToString());
            }
            catch (Exception e) {
                C28Logger.Info(C28Logger.C28LoggerType.AGENT, "error while getting delivery method");
            }

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
