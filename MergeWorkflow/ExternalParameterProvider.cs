using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;
using System.Threading.Tasks;

namespace MergeWorkflow
{
    public class ExternalParameterProvider : IWorkflowExternalParametersProvider
    {
        public object GetExternalParameter(string parameterName, ProcessInstance processInstance)
        {
           return processInstance.GetParameter(parameterName);
        }

        public Task<object> GetExternalParameterAsync(string parameterName, ProcessInstance processInstance)
        {
            return null;
        }

        public bool HasExternalParameter(string parameterName, string schemeCode, ProcessInstance processInstance)
        {
            return false;
        }

        public bool IsGetExternalParameterAsync(string parameterName, string schemeCode, ProcessInstance processInstance)
        {
            return false;
        }

        public bool IsSetExternalParameterAsync(string parameterName, string schemeCode, ProcessInstance processInstance)
        {
            return true;
        }

        public void SetExternalParameter(string parameterName, object parameterValue, ProcessInstance processInstance)
        {
            processInstance.SetParameter(parameterName, parameterValue);
        }

        public Task SetExternalParameterAsync(string parameterName, object parameterValue, ProcessInstance processInstance)
        {
            return processInstance.SetParameterAsync(parameterName, parameterValue);
        }
    }
}
