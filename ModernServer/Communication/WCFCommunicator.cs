using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using CommonLib.Helpers;

namespace ModernServer.Communication
{
    public class WCFCommunicator : IWCFCommunicator
    {
        public void Hello()
        {
            Logger.Log("Client connected.");    
        }
    }
}
