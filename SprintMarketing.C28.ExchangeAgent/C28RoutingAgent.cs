using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Exchange.Data.Transport;
using Microsoft.Exchange.Data.Transport.Routing;
using SprintMarketing.C28.ExchangeAgent.API;
using SprintMarketing.C28.ExchangeAgent.API.Models;

using System.Text;
using System.Globalization;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Configuration.Install;
using System.Linq.Expressions;
using Microsoft.Win32;
using Microsoft.Exchange.Data.Transport.Smtp;
using SprintMarketing.C28.ExchangeAgent.converters;

namespace SprintMarketing.C28.ExchangeAgent {
    public class C28AgentFactory : RoutingAgentFactory
    {
        public override RoutingAgent CreateAgent(SmtpServer server)
        {
            return new C28RoutingAgent();
        }
    }


    public class C28RoutingAgent : RoutingAgent
    {
        public C28RoutingAgent()
        {
            OnResolvedMessage += SprintRoutingAgent_OnResolvedMessage;
        }

        void SprintRoutingAgent_OnResolvedMessage(ResolvedMessageEventSource source, QueuedMessageEventArgs e)
        {
            try
            {
                var context = C28AgentManager.getInstance().getContext();
                if (!context.shouldBeHandledByC28(e.MailItem))
                {
                    C28Logger.Info(C28Logger.C28LoggerType.AGENT,
                        String.Format("Message from '{0}'. Domain is not present, ignoring.",
                            e.MailItem.FromAddress.ToString()));
                    return;
                }

                C28ExchangeDomain domain = context.exchangeData.getDomain(e.MailItem.FromAddress.ToString());
                if (domain == null)
                {
                    return;
                }
                RoutingAddress fromAddr = e.MailItem.FromAddress;
                C28Logger.Debug(C28Logger.C28LoggerType.AGENT,
                    String.Format("Domain '{0}' is set to be overriden to routing domain '{1}'", fromAddr.DomainPart,
                        domain.connector_override));
                try
                {
                    Stream newBodyContent = e.MailItem.Message.Body.GetContentWriteStream();
                    C28MessageConverterFactory.getConverterForEmailMessage(e.MailItem.Message.Body.BodyFormat)
                        .convert(e.MailItem.Message, ref newBodyContent);
                }
                catch (C28ConverterException ee)
                {
                    C28Logger.Error(C28Logger.C28LoggerType.AGENT, "Error while trying to convert message", ee);
                }

                foreach (var recp in e.MailItem.Recipients)
                {
                    if (fromAddr.DomainPart.ToLower() == recp.Address.DomainPart.ToLower() &&
                        domain.same_domain_action == "LocalDelivery")
                    {
                        C28Logger.Debug(C28Logger.C28LoggerType.AGENT,
                            String.Format(
                                "Message from '{0}' to '{1}' was ignored; both are on the same internal domain.",
                                fromAddr.ToString(), recp.Address.ToString()));
                        continue;
                    }
                    if (recp.RecipientCategory == RecipientCategory.InSameOrganization &&
                        context.exchangeData.currentClient.same_organization_action == "LocalDelivery")
                    {
                        C28Logger.Debug(C28Logger.C28LoggerType.AGENT,
                            String.Format("Recipient '{0}' is in the same organization; ignoring.",
                                recp.Address.ToString()));
                        continue;
                    }

                    recp.SetRoutingOverride(new RoutingDomain(domain.connector_override));
                }

                return;
            }
            catch (Exception ee)
            {
                C28Logger.Fatal(C28Logger.C28LoggerType.AGENT, "Unhandled Exception", ee);
            }
        }
    }
}
