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
    class C28TextBasedMessageConverter : C28MessageConverter
    {
        protected override void convertMessage(ref EmailMessage msg)
        {
            if (!(msg.Body.BodyFormat == BodyFormat.Html || msg.Body.BodyFormat == BodyFormat.Text))
            {
                throw new C28ConverterException("Tried to convert a non-RTF EmailMessage with the RTF EmailMessage Converter");
            }

            // TODO : do something with text based emails?
        }
    }
}
