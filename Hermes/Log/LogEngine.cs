using System;
using System.Collections.Generic;
using System.Text;

namespace Hermes.Log
{
    public sealed class LogEngine : IDisposable
    {
        private static readonly Lazy<LogEngine> lazy = new Lazy<LogEngine>(() => new LogEngine());

        private string configXML = "<?xml version=\"1.0\" encoding=\"utf-8\" ?> \r\n" +
                                   "<nlog xmlns = \"http://www.nlog-project.org/schemas/NLog.xsd\" \r\n " +
                                         " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" \r\n" +
                                         " xsi:schemaLocation=\"http://www.nlog-project.org/schemas/NLog.xsd NLog.xsd\"   \r\n" +
                                         "autoReload=\"true\" \r\n" +
                                         "throwExceptions=\"false\" \r\n" +
                                         "throwConfigExceptions =\"true\" \r\n" +
                                         "internalLogLevel =\"Off\" \r\n" +
                                         "internalLogFile =\"c:\\temp\\nlog-internal.log\">  \r\n" +
                                    " <variable name = \"layou\" value=\"${longdate} | ${level:uppercase=true} | ${logger} | ${callsite:className=True:methodName=True} | ${message} | ${exception:format=tostring}\"/> \r\n" +
                                   " <targets async = \"true\" > \r\n" +
                                   " <target name=\"logfile\" xsi:type=\"File\" fileName=\"${basedir}/Log/${date:format=yyyy-MM-dd}_${level}.log\" layout=\"${layou}\" /> \r\n " +
                                   " <target name = \"logconsole\" layout=\"${layou}\" xsi:type=\"Console\" /> \r\n" +
                                   " </targets> \r\n" +
                                   "<rules> \r\n " +
                                   "<logger name = \"*\" minlevel=\"Info\" writeTo=\"logconsole\" /> \r\n " +
                                   "<logger name = \"*\" minlevel=\"Debug\" writeTo=\"logfile\" /> \r\n " +
                                   "</rules> \r\n" +
                                   "</nlog>";

        public static LogEngine Instance { get { return lazy.Value; } }

        private NLog.Logger lEngine;
        public NLog.Logger Engine { get => this.lEngine; }

        public void EnableLoggingLevel( NLog.LogLevel level)
        {
            foreach ( var rule in NLog.LogManager.Configuration.LoggingRules)
            {
                rule.EnableLoggingForLevel(level);
            }
        }
        public void DisableLoggingLevel(NLog.LogLevel level)
        {
            foreach (var rule in NLog.LogManager.Configuration.LoggingRules)
            {
                rule.DisableLoggingForLevel(level);
            }
        }

        public LogEngine()
        {
            //string fileConfig = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "NLog.config");
            //if (System.IO.File.Exists(fileConfig))
            //{
            //    System.IO.File.Delete(fileConfig);
            //}
            //System.IO.File.WriteAllText(fileConfig, this.configXML);
            this.lEngine = NLog.LogManager.GetCurrentClassLogger();
            //foreach (var item in NLog.LogLevel.AllLevels)
            //{
            //    this.EnableLoggingLevel(item);
            //}
        }
        public void Dispose()
        {
            NLog.LogManager.Shutdown();
        }
    }
}
