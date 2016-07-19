using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.IO;
using System.Xml;
using System.ServiceModel.Channels;

namespace ModernServer.CrossDomainService
{
    public class WCFCrossDomainService : IWCFCrossDomainService
    {
        public Message GetPolicyFile()
        {
            using (FileStream stream = File.Open("clientaccesspolicy.xml", FileMode.Open))
            {
                using (XmlReader xmlReader = XmlReader.Create(stream))
                {
                    Message m = Message.CreateMessage(MessageVersion.None, "", xmlReader);
                    using (MessageBuffer buffer = m.CreateBufferedCopy(1000))
                    {
                        return buffer.CreateMessage();
                    }
                }
            }
        }
    }
}
