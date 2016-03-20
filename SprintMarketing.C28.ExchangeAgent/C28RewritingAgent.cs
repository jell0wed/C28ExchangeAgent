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
using Microsoft.Exchange.Data.Transport.Email;
using Microsoft.Win32;
using Microsoft.Exchange.Data.Transport.Smtp;
using SprintMarketing.C28.ExchangeAgent.converters;

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
                try
                {
                    Stream newBodyContent = e.MailItem.Message.Body.GetContentWriteStream();
                    EmailMessage msg = e.MailItem.Message;
                    C28MessageConverterFactory.getConverterForEmailMessage(e.MailItem.Message.Body.BodyFormat)
                        .convert(ref msg);
                    e.MailItem.Message.Subject += " -- rewrited";
                }
                catch (C28ConverterException ee)
                {
                    C28Logger.Error(C28Logger.C28LoggerType.AGENT, "Error while trying to convert message", ee);
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
