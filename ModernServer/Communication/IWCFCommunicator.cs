using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ModernServer.Communication
{
    [ServiceContract]
    public interface IWCFCommunicator
    {
        [OperationContract]
        void Hello();
    }
}
