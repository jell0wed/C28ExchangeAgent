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
    public abstract class C28MessageConverter
    {
        
        private Stream getMessageBodyStream(Body body)
        {
            Stream originalBodyStream;
            if (!body.TryGetContentReadStream(out originalBodyStream))
            {
                throw new C28ConverterException("Cannot decode message body.");
            }

            return originalBodyStream;
        }

        protected abstract void convertMessage(EmailMessage msg, Stream readStream, ref Stream writeStream);

        public void convert(EmailMessage msg, ref Stream writeStream)
        {
            Stream bodyStream = this.getMessageBodyStream(msg.Body);
            this.convertMessage(msg, bodyStream, ref writeStream);
        }
    }
}
