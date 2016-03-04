using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SprintMarketing.C28.ExchangeAgent
{
    public enum C28ConfigValues {
        FETCH_API_KEY,
        FETCH_INTERVAL_MIN,
        FETCH_EAGER,
        FETCH_URL,
        FETCH_CACHE_FILE,
        LOG_FILE,
        LOG_LEVEL,
        LOG_MAX_SIZE
    }
    
    public class C28AgentConfig
    {
        private Dictionary<C28ConfigValues, Object> values = new Dictionary<C28ConfigValues, object>()
        {
            { C28ConfigValues.FETCH_API_KEY, "" },
            { C28ConfigValues.FETCH_INTERVAL_MIN, 20 },
            { C28ConfigValues.FETCH_EAGER, false },
            { C28ConfigValues.FETCH_URL, "" },
            { C28ConfigValues.FETCH_CACHE_FILE, "cache.json" },
            { C28ConfigValues.LOG_FILE, "c28.log" },
            { C28ConfigValues.LOG_LEVEL, "debug" },
            { C28ConfigValues.LOG_MAX_SIZE, "20MB" }
        };
        
        public static C28AgentConfig createConfig(String fileName) {
            if (!System.IO.File.Exists(fileName)) {
                throw new Exception("Unable to load configuration file " + fileName);   
            }

            return new C28AgentConfig(System.IO.File.ReadAllText(fileName));
        }

        public static C28AgentConfig defaultConfig() { return new C28AgentConfig(); }

        private C28AgentConfig(String rawJson) {
            var vals = JsonConvert.DeserializeObject<Dictionary<string, object>>(rawJson);
            foreach (KeyValuePair<string, object> entry in vals) {
                try {
                    var confKey = (C28ConfigValues)Enum.Parse(typeof(C28ConfigValues), entry.Key.ToUpper());
                    this.values[confKey] = entry.Value;
                } catch (ArgumentException e) {
                    C28Logger.Error(C28Logger.C28LoggerType.CONFIG, String.Format("Unknown key value '{0}'; ignoring.", entry.Key));
                }
            }
        }

        private C28AgentConfig() {
        }

        public String getAsString(C28ConfigValues key) {
            return this.values[key].ToString();
        }

        public int getAsInteger(C28ConfigValues key) {
            return Int32.Parse(this.getAsString(key));
        }

        public bool getAsBoolean(C28ConfigValues key) {
            return Boolean.Parse(this.getAsString(key));
        }
    }
}
