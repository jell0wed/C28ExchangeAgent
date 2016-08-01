using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprintMarketing.C28.ExchangeAgent.API
{
    class C28APIException : Exception
    {
        public C28APIException(String message, Exception src = null) : base(message, src)
        {
        }
    }
}
