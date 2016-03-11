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
using Microsoft.Win32;
using Microsoft.Exchange.Data.Transport.Smtp;

namespace SprintMarketing.C28.ExchangeAgent {
    public class C28AgentFactory : RoutingAgentFactory
    {
        public override RoutingAgent CreateAgent(SmtpServer server)
        {
            return new C28RoutingAgent();
        }
    }


    public class C28RoutingAgent : RoutingAgent
    {
        public static C28AgentConfig currentConfig = C28AgentConfig.defaultConfig();
        public List<string> IgnoreDomains { get; set; }
        private C28ExchangeData exchangeDomains;

        public C28RoutingAgent()
        {
            try
            {
                String configPath = Path.Combine(Environment.GetEnvironmentVariable("C28AgentInstallDir"), "config.json");
                C28RoutingAgent.currentConfig = C28AgentConfig.createConfig(configPath);
                C28Logger.Setup(C28RoutingAgent.currentConfig);
                C28Logger.Info(C28Logger.C28LoggerType.AGENT, "C-28 Transport Agent has been initialized");
                if (currentConfig.getAsBoolean(C28ConfigValues.FETCH_EAGER))
                {
                    C28Logger.Warn(C28Logger.C28LoggerType.AGENT,
                        @"WARNING: Agent is currently set to act in EAGER mode. Fetched data from the API wont be cached. Thus the WebAPI will be queried upon every incoming email. To change this setting, please refer to the config.json configuration file.");
                }

                String cachePath = Path.Combine(Environment.GetEnvironmentVariable("C28AgentInstallDir"), currentConfig.getAsString(C28ConfigValues.FETCH_CACHE_FILE));
                C28CacheManager c28CacheManager = new C28CacheManager(cachePath, currentConfig.getAsInteger(C28ConfigValues.FETCH_INTERVAL_MIN));
                IC28WebApi api = new C28APIHttpImpl(currentConfig.getAsString(C28ConfigValues.FETCH_URL), currentConfig.getAsString(C28ConfigValues.FETCH_API_KEY));
                try
                {
                    if (!c28CacheManager.isValid() || currentConfig.getAsBoolean(C28ConfigValues.FETCH_EAGER))
                    {
                        C28Logger.Info(C28Logger.C28LoggerType.AGENT, "Cache is invalid. Data from the API will be used.");
                        this.exchangeDomains = api.getExchangeData();
                        c28CacheManager.updateCache(this.exchangeDomains);
                    }
                    else {
                        C28Logger.Info(C28Logger.C28LoggerType.AGENT, "Cache is still valid. Cached data will be used.");
                        this.exchangeDomains = c28CacheManager.retrieveFromCache();
                    }
                }
                catch (Exception e)
                {
                    C28Logger.Error(C28Logger.C28LoggerType.AGENT, "Unexpected exception while fetching data. Cache will be invalidated and the API queried once more.", e);
                    c28CacheManager.invalidateCache();
                    this.exchangeDomains = api.getExchangeData();
                    c28CacheManager.updateCache(this.exchangeDomains);
                }

                OnResolvedMessage += SprintRoutingAgent_OnResolvedMessage;
            }
            catch (Exception e) {
                C28Logger.Fatal(C28Logger.C28LoggerType.AGENT, "Unknown exception : " + e.Message, e);
            }
        }

        void SprintRoutingAgent_OnResolvedMessage(ResolvedMessageEventSource source, QueuedMessageEventArgs e)
        {
            RoutingAddress fromAddr = e.MailItem.FromAddress;
            if (!this.exchangeDomains.hasDomain(fromAddr.DomainPart)) {
                C28Logger.Debug(C28Logger.C28LoggerType.AGENT, String.Format("Sender '{0}' does not belong to any listed domain. Ignoring.", fromAddr.ToString()));
                return;
            }

            C28ExchangeDomain domain = this.exchangeDomains.getDomain(fromAddr.DomainPart);
            if (domain.isEmailExcluded(fromAddr.ToString())) {
                C28Logger.Debug(C28Logger.C28LoggerType.AGENT, String.Format("Sender '{0}' is set to be excluded. Ignoring.", fromAddr.ToString()));
                return;
            }

            C28Logger.Debug(C28Logger.C28LoggerType.AGENT, String.Format("Domain '{0}' is set to be overriden to routing domain '{1}'", fromAddr.DomainPart, domain.connector_override));
            foreach (var recp in e.MailItem.Recipients) {
                recp.SetRoutingOverride(new RoutingDomain(domain.connector_override));
            }

            return;
        }
        
    }
}
