using System.IO;
using System.Reflection;
using System.Xml;
using log4net;
using log4net.Config;
using log4net.Repository.Hierarchy;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Optima.Utilities 
{
    /// <summary>
    /// Class Log4NetHelper.
    /// </summary>
    public static class Log4NetHelper
    {
        /// <summary>
        /// Configures the log4net.
        /// </summary>
        /// <param name="webHost">The web host.</param>
        /// <returns>IHostBuilder.</returns>
        public static IHostBuilder ConfigureLog4Net(this IHostBuilder webHost)
        {
            var log4NetConfig = new XmlDocument();
            log4NetConfig.Load(File.OpenRead("log4net.config"));

            var loggerRepository = LogManager.CreateRepository(Assembly.GetEntryAssembly(),
                typeof(Hierarchy));

            XmlConfigurator.Configure(loggerRepository, log4NetConfig["log4net"]);

            webHost.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddLog4Net();
            });

            return webHost;
        }
    }
}