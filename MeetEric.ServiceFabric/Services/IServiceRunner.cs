namespace MeetEric.Services
{
    using System;
    using System.Fabric;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Services.Runtime;

    public interface IServiceRunner
    {
        string ServiceName { get; }

        Task RegisterService();

        void BlockProcessFromExit();
    }
}
