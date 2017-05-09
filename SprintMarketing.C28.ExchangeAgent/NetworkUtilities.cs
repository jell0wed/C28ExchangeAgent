using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;

namespace SprintMarketing.C28.ExchangeAgent
{
    class NetworkUtilities
    {
        public static List<string> GetMacAddress() {
            return NetworkInterface.GetAllNetworkInterfaces().Select(x => x.GetPhysicalAddress().ToString()).ToList();
        }
    }
}
