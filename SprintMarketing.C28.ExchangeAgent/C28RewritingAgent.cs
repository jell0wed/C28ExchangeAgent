using System;
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
                if (!context.isOutbound(e.MailItem)) {
                    return; // dont handle inbound messages
                }

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

                bool allOnSameExchangeDomain = true;
                foreach (var recp in e.MailItem.Recipients) {
                    allOnSameExchangeDomain = allOnSameExchangeDomain &&
                        recp.Address.DomainPart.ToLower() == domain.domain.ToLower();
                }

                foreach (var recp in e.MailItem.Recipients)
                {
                    if (allOnSameExchangeDomain &&
                        domain.same_domain_action == "LocalDelivery")
                    {
                        C28Logger.Debug(C28Logger.C28LoggerType.AGENT,
                            String.Format(
                                "Message from '{0}' to '{1}' was ignored; all recipients are on the same internal domain.",
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
                    if (domain.isRecipientExcluded(recp.Address.ToString())) {
                        C28Logger.Debug(C28Logger.C28LoggerType.AGENT,
                            String.Format("Recipient '{0}' matched an excluded recipient pattern; ignoring.",
                            recp.Address.ToString()));
                        continue;
                    }

                    string encodedEmailAddr = recp.Address.ToString().Replace("@", "__at__") + "@rewrite.c-28proof.com";
                    object recpType = null;
                    if (recp.Properties.TryGetValue("Microsoft.Exchange.Transport.RecipientP2Type", out recpType)) {
                        if ((Int32)recpType == 1) { // to recipients
                            encodedEmailAddr = recp.Address.ToString().Replace("@", "__at__") + "@to.rewrite.c-28proof.com";
                        } else if ((Int32)recpType == 2) { // cc recipients
                            encodedEmailAddr = recp.Address.ToString().Replace("@", "__at__") + "@cc.rewrite.c-28proof.com";
                        } else if ((Int32)recpType == 3) { // bcc recipients
                            encodedEmailAddr = recp.Address.ToString().Replace("@", "__at__") + "@bcc.rewrite.c-28proof.com";
                        }
                    }
                    
                    recp.Address = RoutingAddress.Parse(encodedEmailAddr);
                    C28Logger.Info(C28Logger.C28LoggerType.REWRITER, "Rewrited to " + encodedEmailAddr);
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
