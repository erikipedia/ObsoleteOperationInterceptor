using ObsoleteOperationInterceptor.DTO;
using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace ObsoleteOperationInterceptor
{
    [ServiceContract]
    public interface IDataService
    {
        [OperationContract]
        [WebInvoke(UriTemplate = "/Obsolete/{Id}", Method = "GET", ResponseFormat = WebMessageFormat.Json)]
        [Obsolete("2025-09-05", false)]
        CustomObject GetObsolete(string Id);

        [OperationContract]
        [WebInvoke(UriTemplate = "/Active/{Id}", Method = "GET", ResponseFormat = WebMessageFormat.Json)]
        CustomObject GetActive(string Id);
    }
}
