namespace MeetEric.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Tracing;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [EventSource(Name = "MeetEric-ServiceFabric")]
    internal class ServiceFabricLog<T> : EventSource
    {
        private const int ServiceExceptionId = 1;

        public ServiceFabricLog()
        {
            var name = typeof(T).Name;
            var appName = E("Fabric_ApplicationName");
            ServiceName = $"{appName}/{name}";
            ServiceTypeName = $"{name}Type";
            ApplicationName = appName;
            ApplicationTypeName = E("Fabric_ApplicationId").Split('_').First();
        }

        private string ApplicationName { get; }

        private string ApplicationTypeName { get; }

        private string ServiceName { get; }

        private string ServiceTypeName { get; }

        [NonEvent]
        public void LogException(Exception ex)
        {
            CatastrophicException(
                ex.ToString(),
                ApplicationTypeName,
                ApplicationName,
                ServiceTypeName,
                ServiceName);
        }

        private static string E(string name)
        {
            return Environment.GetEnvironmentVariable(name);
        }

        [Event(ServiceExceptionId, Level = EventLevel.Error, Message = "{0}")]
        private void CatastrophicException(
            string message,
            string applicationTypeName,
            string applicationName,
            string serviceTypeName,
            string serviceName)
        {
            WriteEvent(ServiceExceptionId, message, applicationTypeName, applicationName, serviceTypeName, serviceName);
        }
    }
}
