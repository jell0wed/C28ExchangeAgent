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
        
        protected Stream getMessageBodyStream(Body body)
        {
            Stream originalBodyStream;
            if (!body.TryGetContentReadStream(out originalBodyStream))
            {
                throw new C28ConverterException("Cannot decode message body.");
            }

            return originalBodyStream;
        }

        protected abstract void convertMessage(ref EmailMessage msg);

        public void convert(ref EmailMessage msg)
        {
            this.convertMessage(ref msg);
        }
    }
}
