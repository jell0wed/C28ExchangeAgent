using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SprintMarketing.C28.ExchangeAgent.API.Models;

namespace SprintMarketing.C28.ExchangeAgent.API
{
    interface IC28WebApi
    {
        C28ExchangeData getExchangeData();
    }
}
