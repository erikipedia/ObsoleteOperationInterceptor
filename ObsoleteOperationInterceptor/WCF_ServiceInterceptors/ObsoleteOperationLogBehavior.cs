using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace ObsoleteOperationInterceptor.WCF_ServiceInterceptors
{
    public sealed class ObsoleteOperationLogBehavior : IServiceBehavior
    {
        #region IServiceBehavior Implementation

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        { }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (var cd in serviceHostBase.ChannelDispatchers.OfType<ChannelDispatcher>())
            {
                foreach (var endpointDispatcher in cd.Endpoints)
                {
                    // find matching ContractDescription for this endpoint
                    var endpoint = serviceDescription.Endpoints.FirstOrDefault(se => se.Address.Uri == endpointDispatcher.EndpointAddress.Uri
                        && se.Contract.Name == endpointDispatcher.ContractName
                        && se.Contract.Namespace == endpointDispatcher.ContractNamespace);

                    if (endpoint?.Contract == null) continue;

                    var contract = endpoint.Contract;
                    var contractType = contract.ContractType;
                    var serviceType = serviceDescription.ServiceType;

                    // map interface methods to implementation methods
                    var hasMap = contractType != null && serviceType != null && serviceType.GetInterfaces().Contains(contractType);
                    var map = hasMap ? serviceType.GetInterfaceMap(contractType) : default(InterfaceMapping?);

                    foreach (var opDesc in contract.Operations)
                    {
                        var dispatchOp = endpointDispatcher.DispatchRuntime.Operations.FirstOrDefault(o => o.Name == opDesc.Name);
                        if (dispatchOp == null) continue;

                        // prefer sync/task methods, fall back to APM if needed
                        var ifaceMethod = opDesc.SyncMethod ?? opDesc.TaskMethod ?? opDesc.BeginMethod ?? opDesc.EndMethod;
                        ObsoleteAttribute obsolete = null;

                        if (ifaceMethod != null)
                        {
                            // check for [Obsolete] attribute on the operation contract
                            obsolete = (ObsoleteAttribute)Attribute.GetCustomAttribute(ifaceMethod, typeof(ObsoleteAttribute), true);

                            // also check the service implementation method in case the attribute is placed there
                            if (obsolete == null && map.HasValue)
                            {
                                var im = map.Value;
                                for (int i = 0; i < im.InterfaceMethods.Length; i++)
                                {
                                    if (im.InterfaceMethods[i] == ifaceMethod)
                                    {
                                        var implMethod = im.TargetMethods[i];
                                        obsolete = (ObsoleteAttribute)Attribute.GetCustomAttribute(implMethod, typeof(ObsoleteAttribute), true);
                                        break;
                                    }
                                }
                            }
                        }

                        if (obsolete != null) dispatchOp.ParameterInspectors.Add(new ObsoleteOperationLogInspector(opDesc.Name, obsolete));
                    }
                }
            }
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        { } 

        #endregion
    }
}