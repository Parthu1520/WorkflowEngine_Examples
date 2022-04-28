using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using OptimaJet.Workflow.Core.BPMN;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;
using Task = System.Threading.Tasks.Task;
using System.Diagnostics;

namespace MergeWorkflow
{
    public class ProcessingStagesActionProvider : IWorkflowActionProvider
    {
        private readonly Dictionary<string, Action<ProcessInstance, WorkflowRuntime, string>> _actions = new Dictionary<string, Action<ProcessInstance, WorkflowRuntime, string>>();

        private readonly Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task>> _asyncActions = new Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task>>();

        private readonly Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, bool>> _conditions = new Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, bool>>();

        private readonly Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task<bool>>> _asyncConditions = new Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task<bool>>>();

        public ProcessingStagesActionProvider()
        {
            //_actions.Add("ResourceCheck", ResourceCheck);
            //_actions.Add("BuildMDU", BuildMDU);
            //_actions.Add("BuildSeriesStructure", BuildSeriesStructure);
            //_actions.Add("BuildMDUAndSSStatus", BuildMDUAndSSStatus);
            //_actions.Add("GetReady", GetReady);
            //_actions.Add("PulmoGetReady", PulmoGetReady);
            //_actions.Add("GantryReady", GantryReady);
            //_actions.Add("CIRSGetReady", CIRSGetReady);
            //_actions.Add("ResultEngineGetReady", ResultEngineGetReady);
            //_actions.Add("InjectorGetReady", InjectorGetReady);
            //_actions.Add("CardiacGetReady", CardiacGetReady);
            //_actions.Add("GetReadyStatus", GetReadyStatus);
            //_actions.Add("Start", Strat);
            //_actions.Add("MergeBuild", MergeBuild);
            //_actions.Add("MergeReady", MergeReady);
            //_actions.Add("PingConsole", PingConsole);

            _asyncActions.Add("ResourceCheck", PingConsole1);
            _asyncActions.Add("BuildMDU", PingConsole1);
            _asyncActions.Add("BuildSeriesStructure", PingConsole1);
            _asyncActions.Add("BuildMDUAndSSStatus", PingConsole1);
            _asyncActions.Add("GetReady", PingConsole1);
            _asyncActions.Add("PulmoGetReady", PingConsole1);
            _asyncActions.Add("GantryReady", PingConsole1);
            _asyncActions.Add("CIRSGetReady", PingConsole1);
            _asyncActions.Add("ResultEngineGetReady", PingConsole1);
            _asyncActions.Add("InjectorGetReady", PingConsole1);
            _asyncActions.Add("CardiacGetReady", PingConsole1);
            _asyncActions.Add("GetReadyStatus", PingConsole1);
            _asyncActions.Add("Start", PingConsole1);
            _asyncActions.Add("MergeBuild", PingConsole1);
            _asyncActions.Add("MergeReady", PingConsole1);
            _asyncActions.Add("PingConsole", PingConsole1);
           _conditions.Add("StopCondition", StopCondition);

            _actions.Add("ResourceCheck", PingConsole);
            _actions.Add("BuildMDU", PingConsole);
            _actions.Add("BuildSeriesStructure", PingConsole);
            _actions.Add("BuildMDUAndSSStatus", PingConsole);
            _actions.Add("GetReady", PingConsole);
           _actions.Add("PulmoGetReady", PingConsole);
           _actions.Add("GantryReady", PingConsole);
           _actions.Add("CIRSGetReady", PingConsole);
           _actions.Add("ResultEngineGetReady", PingConsole);
           _actions.Add("InjectorGetReady", PingConsole);
           _actions.Add("CardiacGetReady", PingConsole);
           _actions.Add("GetReadyStatus", PingConsole);
          _actions.Add("Start", PingConsole);
           _actions.Add("MergeBuild", PingConsole);
           _actions.Add("MergeReady", PingConsole);
            _actions.Add("PingConsole", PingConsole);
        }

        private Task PingConsole1(ProcessInstance arg1, WorkflowRuntime arg2, string arg3, CancellationToken arg4)
        {
            return Task.Run(() => {
                var commands = arg2.GetAvailableCommands(arg1.ProcessId, "");
                var activity = arg1.GetParameter<string>("CurrentActivity");
                Console.WriteLine("CurrentActivity: " + arg1.CurrentActivity.Name + ", Command count: " + commands.Count());
            });
        }

       

        private void PingConsole(ProcessInstance arg1, WorkflowRuntime arg2, string arg3)
        {
            var activity = arg1.GetParameter<string>("CurrentActivity"); Console.WriteLine("CurrentActivity: " + activity);
        }

        public static IWorkflowActionProvider GetActionProvider()
        {
            return new ProcessingStagesActionProvider();

        }

        private void MergeReady(ProcessInstance arg1, WorkflowRuntime arg2, string arg3)
        {
        }

        private void MergeBuild(ProcessInstance arg1, WorkflowRuntime arg2, string arg3)
        {
        }

        private void ImageReceiver(ProcessInstance processInstance, WorkflowRuntime runtime, string arg3)
        {
            var commands = runtime.GetAvailableCommands(processInstance.ProcessId, String.Empty);
        }

        private void Strat(ProcessInstance arg1, WorkflowRuntime runtime, string arg3)
        {
        }

        private bool StopCondition(ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter)
        {
            return false;
        }

        #region IWorkflowAction provider
        public void ExecuteAction(string name, ProcessInstance processInstance, WorkflowRuntime runtime,
                string actionParameter)
      {
            if (_actions.ContainsKey(name))
                _actions[name].Invoke(processInstance, runtime, actionParameter);
            else
                throw new NotImplementedException($"Action with name {name} isn't implemented");
        }

        public async Task ExecuteActionAsync(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)
        {

            if (_asyncActions.ContainsKey(name))
                await _asyncActions[name].Invoke(processInstance, runtime, actionParameter, token);
            else
                throw new NotImplementedException($"Async Action with name {name} isn't implemented");
        }

        public bool ExecuteCondition(string name, ProcessInstance processInstance, WorkflowRuntime runtime,
            string actionParameter)
        {
            if (_conditions.ContainsKey(name))
                return _conditions[name].Invoke(processInstance, runtime, actionParameter);

            throw new NotImplementedException($"Condition with name {name} isn't implemented");
        }

        public async Task<bool> ExecuteConditionAsync(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)
        {

            if (_asyncConditions.ContainsKey(name))
                return await _asyncConditions[name].Invoke(processInstance, runtime, actionParameter, token);

            throw new NotImplementedException($"Async Condition with name {name} isn't implemented");
        }

        public bool IsActionAsync(string name)
        {
            return _asyncActions.ContainsKey(name);
        }

        public bool IsConditionAsync(string name)
        {
            return _asyncConditions.ContainsKey(name);
        }

        public List<string> GetActions()
        {
            return _actions.Keys.Union(_asyncActions.Keys).ToList();
        }

        public List<string> GetConditions()
        {
            return _conditions.Keys.Union(_asyncConditions.Keys).ToList();
        }

        public bool IsActionAsync(string name, string schemeCode)
        {
            return _asyncActions.ContainsKey(name);
        }

        public bool IsConditionAsync(string name, string schemeCode)
        {
            return _asyncConditions.ContainsKey(name);
        }

        public List<string> GetActions(string schemeCode, NamesSearchType namesSearchType)
        {
            return _actions.Keys.Union(_asyncActions.Keys).ToList();
        }

        public List<string> GetConditions(string schemeCode, NamesSearchType namesSearchType)
        {
            return _conditions.Keys.Union(_asyncConditions.Keys).ToList();
        }

        public bool BuildSeriesStructureCondition(ProcessInstance processInstance,
           WorkflowRuntime workflowRuntime, string parameter)
        {
            return true;
        }

        public bool BuildMDUCondition(ProcessInstance processInstance, WorkflowRuntime workflowRuntime, string parameter)
        {
            return false;
        }


        public void BuildMDUAndSSStatus(ProcessInstance processInstance, WorkflowRuntime workflowRuntime, string parameter)
        {
           
        }

        public void GetReady(ProcessInstance processInstance, WorkflowRuntime workflowRuntime, string parameter)
        {
           

        }
        public void PulmoGetReady(ProcessInstance processInstance, WorkflowRuntime workflowRuntime, string parameter)
        {
           
        }

        public void GantryReady(ProcessInstance processInstance, WorkflowRuntime workflowRuntime, string parameter)
        {
         
        }
        public void CIRSGetReady(ProcessInstance processInstance, WorkflowRuntime workflowRuntime, string parameter)
        {
           
        }

        public void ResultEngineGetReady(ProcessInstance processInstance, WorkflowRuntime workflowRuntime, string parameter)
        {
            
        }

        public void InjectorGetReady(ProcessInstance processInstance, WorkflowRuntime workflowRuntime, string parameter)
        {
          
        }
        public void CardiacGetReady(ProcessInstance processInstance, WorkflowRuntime workflowRuntime, string parameter)
        {
            
        }
        public void GetReadyStatus(ProcessInstance processInstance, WorkflowRuntime workflowRuntime, string parameter)
        {
          
        }
        public void BuildMDU(ProcessInstance processInstance, WorkflowRuntime workflowRuntime, string parameter)
        {
            
        }

        public void BuildSeriesStructure(ProcessInstance processInstance, WorkflowRuntime workflowRuntime, string parameter)
        {
          
        }

        public void ResourceCheck(ProcessInstance processInstance, WorkflowRuntime workflowRuntime, string parameter)
        {
            var commands = workflowRuntime.GetAvailableCommands(processInstance.ProcessId, String.Empty);
            Debugger.Launch();
        }
        #endregion
    }
}

