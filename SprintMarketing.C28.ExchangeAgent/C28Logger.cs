using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using log4net;
using log4net.Repository.Hierarchy;
using log4net.Core;
using log4net.Appender;
using log4net.Layout;
using SprintMarketing.C28.ExchangeAgent;

namespace SprintMarketing.C28.ExchangeAgent
{
    public static class C28Logger
    {
        public enum C28LoggerType { AGENT, API, CACHE, CONFIG, UTILS, ETC }

        private static Dictionary<C28LoggerType, ILog> loggers = new Dictionary<C28LoggerType, ILog>() {
            { C28LoggerType.AGENT, LogManager.GetLogger(Enum.GetName(typeof(C28LoggerType), C28LoggerType.AGENT)) },
            { C28LoggerType.API, LogManager.GetLogger(Enum.GetName(typeof(C28LoggerType), C28LoggerType.API)) },
            { C28LoggerType.CACHE, LogManager.GetLogger(Enum.GetName(typeof(C28LoggerType), C28LoggerType.CACHE)) },
            { C28LoggerType.CONFIG, LogManager.GetLogger(Enum.GetName(typeof(C28LoggerType), C28LoggerType.CONFIG)) },
            { C28LoggerType.UTILS, LogManager.GetLogger(Enum.GetName(typeof(C28LoggerType), C28LoggerType.UTILS)) },
            { C28LoggerType.ETC, LogManager.GetLogger(Enum.GetName(typeof(C28LoggerType), C28LoggerType.ETC)) }
        };

        private const String LOG_LAYOUT = "%date [%thread] $-5level (%logger): %message%newline";

        public static void Setup(C28AgentConfig config) {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

            var layout = new PatternLayout();
            layout.ConversionPattern = LOG_LAYOUT;
            layout.ActivateOptions();

            var roller = new RollingFileAppender();
            roller.AppendToFile = true;
            roller.File = config.getAsString(C28ConfigValues.LOG_FILE);
            roller.Layout = layout;
            roller.MaxSizeRollBackups = 0;
            roller.MaximumFileSize = config.getAsString(C28ConfigValues.LOG_MAX_SIZE);
            roller.RollingStyle = RollingFileAppender.RollingMode.Size;
            roller.StaticLogFileName = true;
            roller.ActivateOptions();
            hierarchy.Root.AddAppender(roller);

            MemoryAppender memory = new MemoryAppender();
            memory.ActivateOptions();
            hierarchy.Root.AddAppender(memory);

            hierarchy.Root.Level = getLogLevel(config.getAsString(C28ConfigValues.LOG_LEVEL));
            hierarchy.Configured = true;
        }

        private static Level getLogLevel(String lvl)
        {
            switch (lvl.ToLower())
            {
                case "debug": return Level.Debug;
                case "info": return Level.Info;
                case "warn": return Level.Warn;
                case "error": return Level.Error;
                case "fatal": return Level.Fatal;
                case "all": return Level.All;
                case "off": return Level.Off;
            }

            return Level.All;
        }

        public static void Debug(C28LoggerType type, String msg) {
            loggers[type].Debug(msg);
        }

        public static void Info(C28LoggerType type, String msg)
        {
            loggers[type].Info(msg);
        }

        public static void Warn(C28LoggerType type, String msg, Exception e = null) {
            loggers[type].Warn(msg, e);
        }
        
        public static void Error(C28LoggerType type, String msg, Exception e = null) {
            loggers[type].Error(msg, e);
        }

        public static void Fatal(C28LoggerType type, String msg, Exception e = null) {
            loggers[type].Fatal(msg, e);
        }
    }
}
