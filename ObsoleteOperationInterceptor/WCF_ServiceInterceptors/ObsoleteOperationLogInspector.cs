using log4net;
using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;

namespace ObsoleteOperationInterceptor.WCF_ServiceInterceptors
{
    public class ObsoleteOperationLogInspector : IParameterInspector
    {
        #region Private Fields

        private readonly string _operationName;
        private readonly ObsoleteAttribute _obsolete;

        private readonly ILog _log = LogManager.GetLogger(typeof(ObsoleteOperationLogInspector));

        #endregion


        #region Constructor

        public ObsoleteOperationLogInspector(string operationName, ObsoleteAttribute obsolete)
        {
            _operationName = operationName ?? throw new ArgumentNullException(nameof(operationName));
            _obsolete = obsolete ?? throw new ArgumentNullException(nameof(obsolete));

            log4net.Config.XmlConfigurator.Configure();

            var logRepository = LogManager.GetRepository();
            var appenders = logRepository.GetAppenders();

            foreach (var appender in appenders)
            {
                if (appender is log4net.Appender.FileAppender fileAppender)
                {
                    fileAppender.File = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "ObsoleteOperation.log");
                    fileAppender.ActivateOptions();
                }
            }
        }

        #endregion


        #region IParameterInspector Implementation

        public object BeforeCall(string operationName, object[] inputs)
        {
            OperationContext context = OperationContext.Current;
            var caller = context?.ServiceSecurityContext?.PrimaryIdentity?.Name ?? "anonymous";
            var endpoint = context?.IncomingMessageHeaders?.To?.AbsoluteUri ?? "unknown";
            var contract = context?.EndpointDispatcher?.ContractName ?? "unknown";
            var callerIp = "unknown";
            if (context?.IncomingMessageProperties != null && context.IncomingMessageProperties.ContainsKey(RemoteEndpointMessageProperty.Name))
            {
                var prop = (RemoteEndpointMessageProperty)context.IncomingMessageProperties[RemoteEndpointMessageProperty.Name];
                callerIp = prop.Address;
            }

            // log the call
            string message = $"Obsolete operation invoked: Contract=[{contract}], Operation=[{_operationName}], Caller=[{caller}], IP=[{callerIp}], Endpoint=[{endpoint}], ObsoletionDate=[{_obsolete.Message ?? "unknown"}]";
            if (_obsolete.IsError) _log.Error(message);
            else _log.Warn(message);

            // if IsError is set to true, return 400 - this lets us disable the endpoint without removing it
            if (_obsolete.IsError) throw new WebFaultException<string>($"Operation '{_operationName}' was obsolesced on {_obsolete.Message}.", System.Net.HttpStatusCode.BadRequest);

            // otherwise, let the call go through
            return null;
        }

        public void AfterCall(string operationName, object[] outputs, object returnValue, object correlationState)
        { }

        #endregion
    }
}