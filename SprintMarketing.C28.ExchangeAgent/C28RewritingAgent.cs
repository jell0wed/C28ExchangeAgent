﻿using System;
using Microsoft.Exchange.Data.Transport;
using SprintMarketing.C28.ExchangeAgent.API.Models;

using Microsoft.Exchange.Data.Transport.Smtp;

namespace SprintMarketing.C28.ExchangeAgent {
    public class C28RewritingFactory : SmtpReceiveAgentFactory
    {
        public override SmtpReceiveAgent CreateAgent(SmtpServer server)
        {
            return new C28RewritingAgent();
        }
    }
    

    public class C28RewritingAgent : SmtpReceiveAgent
    {
        public C28RewritingAgent()
        {
            OnEndOfData += SprintAgent_RewriteEmail;
        }

        void SprintAgent_RewriteEmail(ReceiveMessageEventSource source, EndOfDataEventArgs e)
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

                C28ExchangeDomain domain = context.exchangeData.getDomain(e.MailItem.FromAddress.DomainPart);
                if (domain == null)
                {
                    C28Logger.Info(C28Logger.C28LoggerType.AGENT, String.Format("Domain '{0}' could not be found... Skipping entry", e.MailItem.FromAddress.DomainPart));
                    return;
                }

                RoutingAddress fromAddr = e.MailItem.FromAddress;
                C28Logger.Debug(C28Logger.C28LoggerType.AGENT,
                    String.Format("Domain '{0}' is set to be overriden to routing domain '{1}'", fromAddr.DomainPart,
                        domain.connector_override));
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

                    string encodedEmailAddr = recp.Address.ToString().Replace("@", "__at__") + "@rewrite.c-28proof.com";

                    recp.Address = RoutingAddress.Parse(encodedEmailAddr);
                    C28Logger.Info(C28Logger.C28LoggerType.REWRITER, "Rewrited to " + encodedEmailAddr);

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
