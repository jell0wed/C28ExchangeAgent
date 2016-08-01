using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Exchange.Data.TextConverters;
using Microsoft.Exchange.Data.Transport.Email;
using SprintMarketing.C28.ExchangeAgent.API;

namespace SprintMarketing.C28.ExchangeAgent.converters
{
    class C28RtfMessageConverter : C28MessageConverter
    {
        protected override void convertMessage(ref EmailMessage msg)
        {
            if (msg.Body.BodyFormat == BodyFormat.Rtf)
            {
                Stream bodyStream = this.getMessageBodyStream(msg.Body);
                Stream writeStream = msg.Body.GetContentWriteStream();
                try
                {
                    ConverterStream uncompressedRtf = new ConverterStream(bodyStream, new RtfCompressedToRtf(),
                        ConverterStreamAccess.Read);
                    RtfToHtml rtfToHtmlConvert = new RtfToHtml();
                    rtfToHtmlConvert.NormalizeHtml = true;
                    rtfToHtmlConvert.HeaderFooterFormat = HeaderFooterFormat.Html;

                    ConverterReader html = new ConverterReader(uncompressedRtf, rtfToHtmlConvert);
                    rtfToHtmlConvert.Convert(html, msg.Body.GetContentWriteStream());
                    C28Logger.Info(C28Logger.C28LoggerType.REWRITER, "Converted EmailMessage from RTF to HTML format.");
                }
                finally
                {
                    bodyStream.Close();
                    writeStream.Close();
                }
            }
            else
            {
                throw new C28ConverterException("Tried to convert a non-RTF EmailMessage with the RTF EmailMessage Converter");
            }
        }
    }
}
