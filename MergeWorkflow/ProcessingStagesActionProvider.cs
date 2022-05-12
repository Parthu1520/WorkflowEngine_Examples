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
            _asyncActions.Add("ResourceCheck", PingConsoleAsync);
            _asyncActions.Add("BuildMDU", PingConsoleAsync);
            _asyncActions.Add("BuildSeriesStructure", PingConsoleAsync);
            _asyncActions.Add("BuildMDUAndSSStatus", PingConsoleAsync);
            _asyncActions.Add("GetReady", PingConsoleAsync);
            _asyncActions.Add("PulmoGetReady", PingConsoleAsync);
            _asyncActions.Add("GantryReady", PingConsoleAsync);
            _asyncActions.Add("CIRSGetReady", PingConsoleAsync);
            _asyncActions.Add("ResultEngineGetReady", PingConsoleAsync);
            _asyncActions.Add("InjectorGetReady", PingConsoleAsync);
            _asyncActions.Add("CardiacGetReady", PingConsoleAsync);
            _asyncActions.Add("GetReadyStatus", PingConsoleAsync);
            _asyncActions.Add("Start", PingConsoleAsync);
            _asyncActions.Add("MergeBuild", PingConsoleAsync);
            _asyncActions.Add("MergeReady", PingConsoleAsync);
            _asyncActions.Add("PingConsole", PingConsoleAsync);
            _conditions.Add("StopCondition", StopCondition);


            _actions.Add("F1", F1);
            _actions.Add("F2", F1);
            _actions.Add("F3", F1);
            _actions.Add("D1", F1);
            _actions.Add("D2", F1);

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

        private void F1(ProcessInstance arg1, WorkflowRuntime arg2, string arg3)
        {
            Console.WriteLine("F!");
        }

        private int _counter;
        public static IWorkflowActionProvider GetActionProvider()
        {
            return new ProcessingStagesActionProvider();
        }

        private Task PingConsoleAsync(ProcessInstance arg1, WorkflowRuntime arg2, string arg3, CancellationToken arg4)
        {
            //For debug process
            Console.WriteLine("Execution number: " + _counter);
            Console.WriteLine("Current activity: " + arg1.CurrentActivity.Name);
            Console.WriteLine("Current state: " + arg1.CurrentState);
            Console.WriteLine("Executed activity: " + arg1.ExecutedActivity.Name);
            var param = arg1.GetParameter("TestParam") as OptimaJet.Workflow.Core.Model.ParameterDefinitionWithValue;
            WorkflowInit.Runtime.ExternalParametersProvider.SetExternalParameter("TestParam", "MyValue", arg1);
            if (param != null)
            {

            }
            //while (param == null)
            //{
            // //param = arg1.GetParameter("TestParam") as OptimaJet.Workflow.Core.Model.ParameterDefinitionWithValue;
            //    param = arg2.ExternalParametersProvider.GetExternalParameter("TestParam", arg1) as OptimaJet.Workflow.Core.Model.ParameterDefinitionWithValue;
            //}                //param = arg2.ExternalParametersProvider.GetExternalParameter("TestParam", arg1) as OptimaJet.Workflow.Core.Model.ParameterDefinitionWithValue;
            Console.WriteLine(param==null?"":param.Value);
            Console.WriteLine("From PingConsole :"+ arg1.GetHashCode());
            Console.WriteLine(arg1.ProcessId.ToString());
            Console.WriteLine("Transaction: " + arg1.StartTransitionalProcessActivity);
            Console.WriteLine("Previous: " + arg1.PreviousActivityName);
            if (arg1.IsSubprocess)
            {
                Console.WriteLine(arg1.SubprocessName);
            }
            Console.WriteLine("----------------------------------------");
            _counter++;

            return Task.CompletedTask;
        }

        private void PingConsole(ProcessInstance arg1, WorkflowRuntime arg2, string arg3)
        {
            object param = null;
           
            PingConsoleAsync(arg1, arg2, arg3, new CancellationToken());
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
       
        #endregion
    }
}

