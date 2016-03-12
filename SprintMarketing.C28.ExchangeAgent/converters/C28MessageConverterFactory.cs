using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Exchange.Data.Transport.Email;
using Microsoft.Exchange.Data.Transport.Smtp;

namespace SprintMarketing.C28.ExchangeAgent.converters
{
    public static class C28MessageConverterFactory
    {
        public static C28MessageConverter getConverterForEmailMessage(BodyFormat format)
        {
            if (format == BodyFormat.Rtf)
            {
                return new C28RtfMessageConverter();
            }
            
            return new C28TextBasedMessageConverter();
        }
    }
}
