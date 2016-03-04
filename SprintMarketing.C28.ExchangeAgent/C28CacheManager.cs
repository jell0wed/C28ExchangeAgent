using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SprintMarketing.C28.ExchangeAgent.Agents.SprintMarketing;
using SprintMarketing.C28.ExchangeAgent.API.Models;

namespace SprintMarketing.C28.ExchangeAgent

{
    class C28CacheManager
    {
        private String cacheLocation;
        private int rotationMin;

        public C28CacheManager(String location, int rotationMin) {
            this.cacheLocation = location;
            this.rotationMin = rotationMin;

            C28Logger.Debug(C28Logger.C28LoggerType.CACHE, 
                String.Format("Initialized cache manager at {0} rotating every {1} mins", this.cacheLocation, this.rotationMin));
        }
        public bool isValid() {
            if (!this.isCacheFilePresent()) {
                return false;
            }

            return DateTime.Now.Subtract(this.getLastUpdated()).TotalMinutes <= this.rotationMin;
        }

        public DateTime getLastUpdated() {
            try
            {
                if (!this.isCacheFilePresent()) { return DateTime.MinValue; }

                return System.IO.File.GetLastWriteTime(this.cacheLocation);
            }
            catch (System.IO.IOException e) {
                C28Logger.Error(C28Logger.C28LoggerType.AGENT, "Unexpected IOException while getting cache last updated date. This will try to invalidate the cache.", e);
                this.invalidateCache();
            }

            return DateTime.MinValue;
        }

        public bool isCacheFilePresent() {
            return System.IO.File.Exists(this.cacheLocation);
        }

        public void invalidateCache()
        {
            try
            {
                if (this.isCacheFilePresent())
                {
                    C28Logger.Debug(C28Logger.C28LoggerType.CACHE, "Invalidating the cache (by deletion).");
                    System.IO.File.Delete(this.cacheLocation);
                }
            }
            catch (Exception e) {
                C28Logger.Fatal(C28Logger.C28LoggerType.CACHE, "Something went wrong while invalidating the cache.", e);
                throw e;
            }
        }

        public void updateCache(C28ExchangeData cachedData) {
            try {
                System.IO.File.WriteAllText(this.cacheLocation, JsonConvert.SerializeObject(cachedData));
                C28Logger.Debug(C28Logger.C28LoggerType.CACHE, String.Format("Successfully updated cached data with {0} domain entries.", cachedData.getCount()));
            } catch (System.IO.IOException e) {
                C28Logger.Error(C28Logger.C28LoggerType.CACHE, "Unexpected IOException while updating cache. This will try to invalidate the cache.", e);
                this.invalidateCache();
            }
        }

        public C28ExchangeData retrieveFromCache() {
            C28Logger.Debug(C28Logger.C28LoggerType.CACHE, "Retrieving data from cache");
            return JsonConvert.DeserializeObject<C28ExchangeData>(System.IO.File.ReadAllText(this.cacheLocation));
        }
    }
}
