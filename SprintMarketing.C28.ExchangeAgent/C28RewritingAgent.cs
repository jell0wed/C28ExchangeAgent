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
using Microsoft.Exchange.Data.TextConverters;
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
            C28Logger.Info(C28Logger.C28LoggerType.REWRITER, "Starting rewriting");
            try
            {
                var context = C28AgentManager.getInstance().getContext();
                if (!context.shouldBeHandledByC28(e.MailItem))
                {
                    return;
                }
                
                C28ExchangeDomain domain = context.exchangeData.getDomain(e.MailItem.FromAddress.DomainPart);
                if (domain == null)
                {
                    return;
                }

                C28Logger.Info(C28Logger.C28LoggerType.REWRITER, "Email is set to be rewritted. Starting rewriting process.");
                
                try
                {
                    EmailMessage message = e.MailItem.Message;
                    Stream originalBodyContent = null;
                    Stream newBodyContent = null;
                    Body body = message.Body;
                    BodyFormat bodyFormat = message.Body.BodyFormat;

                    if (!body.TryGetContentReadStream(out originalBodyContent))
                    {
                        return;
                    }
                    C28Logger.Info(C28Logger.C28LoggerType.REWRITER, String.Format("Rewriting !! ... BodyFormat = {0}", bodyFormat.ToString()));
                    if (BodyFormat.Rtf == bodyFormat)
                    {

                        C28Logger.Info(C28Logger.C28LoggerType.REWRITER, "Rewriting -- Rtf body detected, trying to decode !! ... ");
                        ConverterStream uncompressedRtf = new ConverterStream(originalBodyContent, new RtfCompressedToRtf(), ConverterStreamAccess.Read);
                        RtfToHtml rtfToHtmlConversion = new RtfToHtml();
                        rtfToHtmlConversion.FilterHtml = true;
                        rtfToHtmlConversion.HeaderFooterFormat = HeaderFooterFormat.Html;
                        ConverterReader html = new ConverterReader(uncompressedRtf, rtfToHtmlConversion);
                        newBodyContent = body.GetContentWriteStream();

                        rtfToHtmlConversion.Convert(html, newBodyContent);

                        originalBodyContent.Close();
                        newBodyContent.Close();

                        e.MailItem.Message.Subject += "-- rewrited";
                    }
                }
                catch (C28ConverterException ee)
                {
                    C28Logger.Error(C28Logger.C28LoggerType.AGENT, "Error while trying to convert message", ee);
                }

                return;
            }
            catch (Exception ee)
            {
                C28Logger.Fatal(C28Logger.C28LoggerType.AGENT, "Unhandled Exception while rewriting", ee);
            }
        }
    }
}
