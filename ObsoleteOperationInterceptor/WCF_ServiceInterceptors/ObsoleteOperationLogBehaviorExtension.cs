using System;
using System.ServiceModel.Configuration;

namespace ObsoleteOperationInterceptor.WCF_ServiceInterceptors
{
    public class ObsoleteOperationLogBehaviorExtension : BehaviorExtensionElement
    {
        #region BehaviorExtensionElement Overrides

        public override Type BehaviorType => typeof(ObsoleteOperationLogBehavior);

        protected override object CreateBehavior() => new ObsoleteOperationLogBehavior();

        #endregion
    }
}