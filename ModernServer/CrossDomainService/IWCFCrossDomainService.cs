using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Web;
using System.ServiceModel.Channels;

namespace ModernServer.CrossDomainService
{
    [ServiceContract]
    public interface IWCFCrossDomainService
    {
        [WebGet(UriTemplate = "clientaccesspolicy.xml")]
        Message GetPolicyFile();
    }
}
