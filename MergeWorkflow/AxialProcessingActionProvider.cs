using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace MergeWorkflow
{
    public class AxialProcessingActionProvider : IWorkflowActionProvider
    {
        private readonly Dictionary<string, Action<ProcessInstance, WorkflowRuntime, string>> _actions = new Dictionary<string, Action<ProcessInstance, WorkflowRuntime, string>>();

        private readonly Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task>> _asyncActions = new Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task>>();

        private readonly Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, bool>> _conditions = new Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, bool>>();

        private readonly Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task<bool>>> _asyncConditions = new Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task<bool>>>();

        public AxialProcessingActionProvider()
        {
            _actions.Add("F1", F1);
            _actions.Add("F2", F2);
            _actions.Add("F3", F3);
            _actions.Add("D1", D1);
            _actions.Add("D2",D2);
            _actions.Add("Start", Final);
            _actions.Add("Final", Final); 
            _actions.Add("Intermediate", Final); 
            _actions.Add("ImagesDilutionProcessingStage", F1);
            _actions.Add("BurnTextProcessingStage", F3);
            _actions.Add("CreateLogoImageProcessingStage", D1);
            _actions.Add("OutputProcessingStage", D2);

            _conditions.Add("SelfTransition", SelfTransition);
            timer = new System.Timers.Timer();
            timer.Interval = 500;
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        bool flag = true;
        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            flag = !flag;
        }

        System.Timers.Timer timer;
        private bool SelfTransition(ProcessInstance arg1, WorkflowRuntime arg2, string arg3)
        {
            return false;
        }

        int c = 0;
        private void Final(ProcessInstance arg1, WorkflowRuntime arg2, string arg3)
        {
            Console.WriteLine("Final");
        }
        private void Intermediate(ProcessInstance arg1, WorkflowRuntime arg2, string arg3)
        {
            Console.WriteLine("Intermediate");
        }
        private void Start(ProcessInstance arg1, WorkflowRuntime arg2, string arg3)
        {
            Console.WriteLine("Start");
        }
        private void F3(ProcessInstance arg1, WorkflowRuntime arg2, string arg3)
        {
            System.Console.WriteLine(string.Format("Applying Filter 3 "));
        }

        private void D1(ProcessInstance arg1, WorkflowRuntime arg2, string arg3)
        {
            System.Console.WriteLine(string.Format("DICOM update 1 "));
        }

        private void D2(ProcessInstance arg1, WorkflowRuntime arg2, string arg3)
        {
            System.Console.WriteLine(string.Format("DICOM update 2"));
        }


        private void F2(ProcessInstance arg1, WorkflowRuntime arg2, string arg3)
        {
            System.Console.WriteLine(string.Format("Appyling Filter two "));

        }

        private void F1(ProcessInstance arg1, WorkflowRuntime arg2, string arg3)
        {
            System.Console.WriteLine(string.Format("Appyling Filter one"));
        }

        public static IWorkflowActionProvider GetActionProvider()
        {
            return new AxialProcessingActionProvider();
        }

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
            //token.ThrowIfCancellationRequested(); // You can use the transferred token at your discretion
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
            //token.ThrowIfCancellationRequested(); // You can use the transferred token at your discretion
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
    }
    }

