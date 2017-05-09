using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SprintMarketing.C28.ExchangeAgent.API;
using SprintMarketing.C28.ExchangeAgent.API.Models;

namespace SprintMarketing.C28.ExchangeAgent
{
    class C28AgentManager
    {
        private static C28AgentManager instance = null;

        public static C28AgentManager getInstance()
        {
            if (instance == null)
            {
                instance = new C28AgentManager();
            }

            return instance;
        }

        private C28AgentConfig currentConfig;
        private C28CacheManager cache;
        private IC28WebApi api;

        private C28AgentManager()
        {
            try
            {
                String configPath = Path.Combine(Environment.GetEnvironmentVariable("C28AgentInstallDir"), "config.json");
                this.currentConfig = C28AgentConfig.createConfig(configPath);
                C28Logger.Setup(this.currentConfig);
                C28Logger.Info(C28Logger.C28LoggerType.AGENT, "C-28 Transport Agent has been initialized");
                if (currentConfig.getAsBoolean(C28ConfigValues.FETCH_EAGER))
                {
                    C28Logger.Warn(C28Logger.C28LoggerType.AGENT,
                        @"WARNING: Agent is currently set to act in EAGER mode. Fetched data from the API wont be cached. Thus the WebAPI will be queried upon every incoming email. To change this setting, please refer to the config.json configuration file.");
                }

                String cachePath = Path.Combine(currentConfig.getAsString(C28ConfigValues.DATA_BASE_PATH),
                    currentConfig.getAsString(C28ConfigValues.FETCH_CACHE_FILE));
                this.cache = new C28CacheManager(cachePath,
                    currentConfig.getAsInteger(C28ConfigValues.FETCH_INTERVAL_MIN));
                this.api = new C28APIHttpImpl(currentConfig.getAsString(C28ConfigValues.FETCH_URL),
                    currentConfig.getAsString(C28ConfigValues.FETCH_API_KEY));
                
            }
            catch (Exception e)
            {
                C28Logger.Fatal(C28Logger.C28LoggerType.AGENT, "Unknown exception : " + e.Message, e);
                throw e;
            }
        }

        public C28DecisionMaker getContext()
        {
            C28ExchangeData exchangeData = null;

            try
            {
                if (!this.cache.isValid() || currentConfig.getAsBoolean(C28ConfigValues.FETCH_EAGER))
                {
                    C28Logger.Info(C28Logger.C28LoggerType.AGENT, "Cache is invalid. Data from the API will be used.");
                    exchangeData = api.getExchangeData();
                    this.cache.updateCache(exchangeData);
                    this.api.postHeartbeat();
                    C28Logger.Info(C28Logger.C28LoggerType.API, "Posted Heartbeat to exchange server.");
                }
                else
                {
                    C28Logger.Info(C28Logger.C28LoggerType.AGENT, "Cache is still valid. Cached data will be used.");
                    exchangeData = this.cache.retrieveFromCache();
                }
            }
            catch (Exception e)
            {
                C28Logger.Error(C28Logger.C28LoggerType.AGENT,
                    "Unexpected exception while fetching data. Cache will be invalidated and the API queried once more.",
                    e);
                this.cache.invalidateCache();
                exchangeData = api.getExchangeData();
                this.cache.updateCache(exchangeData);
            }

            if (exchangeData == null)
            {
                throw new C28AgentException("Unable to fetch the exchange data either from cache or remotely. C28 Routing agent will be disabled.");
            }

            return new C28DecisionMaker(exchangeData);
        }
    }
}
