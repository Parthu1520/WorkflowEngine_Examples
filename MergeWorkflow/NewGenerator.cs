
using OptimaJet.Workflow.Core.Generator;
using OptimaJet.Workflow.Core.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Linq;
using System.Diagnostics;

namespace MergeWorkflow
{
    public class NewGenerator : IWorkflowGenerator<XElement>
    {
        static int count = 0;
        private static void CreateAndAddTransitions(ProcessDefinition pd, ActivityDefinition firstActivity,
            ActivityDefinition finalActivity, bool isSubprocess)
        {
            string TransistionName = firstActivity.Name + "To" + finalActivity.Name;

            List<ConditionDefinition> conditonList = null;
            conditonList = new List<ConditionDefinition>();
            TransitionDefinition Transition = TransitionDefinition.Create(TransistionName, TransitionClassifier.NotSpecified,
                                                                          ConcatenationType.And, ConcatenationType.And, ConcatenationType.And, firstActivity,
                                                                          finalActivity, TriggerDefinition.Auto, null);
            Transition.Trigger.Type = TriggerType.Auto;
            if (isSubprocess)
            {
                Transition = Transition.SetSubprocessSettings(TransistionName, Guid.NewGuid().ToString(), false, false, SubprocessInOutDefinition.Start,
                                                     SubprocessStartupType.AnotherThread, SubprocessStartupParameterCopyStrategy.CopyAll,
                                                     SubprocessFinalizeParameterMergeStrategy.OverwriteSpecified, null);
                Transition.IsFork = true;
            }

            pd.Transitions.Add(Transition);
        }

        private static void CreateCheckTransition(ProcessDefinition pd, ActivityDefinition firstActivity,
            ActivityDefinition finalActivity)
        {
            string TransistionName = firstActivity.Name + "To" + finalActivity.Name;
            string ObjectParameter = "{\"Mode\" : \"AllSubprocesses\"}";
            var conditionList = new List<ConditionDefinition>();
            ConditionDefinition Condition = ConditionDefinition.CreateActionCondition(ActionDefinitionReference.Create("CheckAllSubprocessesCompleted", "0", ObjectParameter.ToString()), false, null);
            Condition.Type = ConditionType.Action;
            conditionList.Add(Condition);
            TransitionDefinition Transition = TransitionDefinition.Create(TransistionName, TransitionClassifier.NotSpecified,
                                                                          ConcatenationType.And, ConcatenationType.And, ConcatenationType.And, firstActivity,
                                                                          finalActivity, TriggerDefinition.Auto, conditionList);
            pd.Transitions.Add(Transition);
        }

        private static void CreateMergeProcessTransition(ProcessDefinition pd, ActivityDefinition firstActivity,
            ActivityDefinition finalActivity, bool isMerge = false)
        {
            string TransistionName = firstActivity.Name + "To" + finalActivity.Name;

            TransitionDefinition Transition = TransitionDefinition.Create(TransistionName, TransitionClassifier.NotSpecified,
                                                                          ConcatenationType.And, ConcatenationType.And, ConcatenationType.And, firstActivity,
                                                                          finalActivity, TriggerDefinition.Auto, null);

            Transition.IsFork = true;
            Transition.SubprocessInOutDefinition = SubprocessInOutDefinition.Finalize;
            if (isMerge)
            {
                Transition.MergeViaSetState = true;
            }
            pd.Transitions.Add(Transition);
        }

        private static void CreateAndAddActivities(ProcessDefinition pd, String stageName, bool IsInitial, bool IsFinal)
        {
            ActivityDefinition newActivity = ActivityDefinition.Create(stageName, stageName, IsInitial, IsFinal, true, true);
            newActivity.AddAction(ActionDefinitionReference.Create(stageName, "1", null));
            pd.Activities.Add(newActivity);
        }

        public XElement Generate(string schemeCode, Guid schemeId, IDictionary<string, object> parameters)
        {
           // var code = "var activity = processInstance.GetParameter<string>(\"CurrentActivity\");" +"\n"+
           //     " Console.WriteLine(\"CurrentActivity: \" + activity);";
           //var codeAction = CodeActionDefinition.Create("PingConsole",
           //        ""
           //        , code, "false", "Action", "false", null);
           var pd = ProcessDefinition.Create(schemeCode + "SimpleProcess", false, new List<ActorDefinition>(),
                new List<ParameterDefinition>()
                {
                    ParameterDefinition.Create("SubProcessName",typeof(string),ParameterPurpose.Temporary,"ImageProcessing"),
                    ParameterDefinition.Create("Count",typeof(int),ParameterPurpose.Temporary,"0")
                },
                new List<CommandDefinition>()
                {
                    CommandDefinition.Create("CheckAllSubprocessesCompleted") , CommandDefinition.Create("Merge"),CommandDefinition.Create("CreateSubProcess")
                },
                new List<TimerDefinition>(),
                new List<ActivityDefinition>(), new List<TransitionDefinition>(), new List<LocalizeDefinition>(),
                new List<CodeActionDefinition>()
                {
                   // codeAction
                },
                DesignerSettings.Empty, new List<string>());

           // pd.CodeActionsCommonUsings = "System.IO;OptimaJet.Workflow;OptimaJet.WorkflowServer.Services.PostgreSql;FxResources.System.Net.Http;OptimaJet.WorkflowServer.Integration;OptimaJet.WorkflowServer.Model.MongoDB;System.Net;OptimaJet.WorkflowServer.Persistence;OptimaJet.WorkflowServer.Initializing;System;OptimaJet.WorkflowServer.Identity.IdentityDataProvider;OptimaJet.WorkflowServer.Model.PostgreSQL;System.Collections.Generic;System.Net.Http.HPack;OptimaJet.WorkflowServer.Forms;System.Threading;OptimaJet.WorkflowServer.Integration.Base.Interfaces;OptimaJet.Logging;OptimaJet.WorkflowServer;OptimaJet.WorkflowServer.Services.MySql;System.Collections;OptimaJet.WorkflowServer.Utils;System.Linq;OptimaJet.WorkflowServer.Forms.Model;OptimaJet.WorkflowServer.Model.Oracle;System.Net.Security;System.Runtime.CompilerServices;OptimaJet.WorkflowServer.Services.Cors;OptimaJet.WorkflowServer.Callback;Microsoft.Extensions.Internal;OptimaJet.Logging.Database;OptimaJet.WorkflowServer.Identity.Native;OptimaJet.WorkflowServer.RuleProviders;OptimaJet.Workflow.Core.Runtime;OptimaJet.WorkflowServer.BackgoundTasks;System.Net.Http;OptimaJet.WorkflowServer.Identity;OptimaJet.WorkflowServer.Services.MsSql;System.Net.Http.QPack;Microsoft.CSharp;OptimaJet.WorkflowServer.Model.Users;OptimaJet.WorkflowServer.Services.MongoDB;OptimaJet.WorkflowServer.Integration.Base.Classes;System.Threading.Tasks;Microsoft.CodeAnalysis;OptimaJet.WorkflowServer.Logging;OptimaJet.WorkflowServer.Services;System.Dynamic;System.Net.Http.Headers;Microsoft.CSharp.RuntimeBinder;System.Text;OptimaJet.WorkflowServer.Services.Oracle;OptimaJet.WorkflowServer.Model.MSSQL;OptimaJet.WorkflowServer.Hubs;OptimaJet.Workflow.Core.Model;OptimaJet.WorkflowServer.Identity.Sync;OptimaJet.WorkflowServer.Model;OptimaJet.WorkflowServer.Model.MySQL;OptimaJet.WorkflowServer.License;";

            object stageInfoList;
            parameters.TryGetValue("StagesInfo", out stageInfoList);
            bool isInitial = true, isFinal = false;

            foreach (var stageInfo in (List<StageInfo>)stageInfoList)
            {
                CreateAndAddActivities(pd, stageInfo.Stage, isInitial, isFinal);
                isInitial = false;
            }
            int NoOfActivities = pd.Activities.Count;
            List<ActivityDefinition> ActivitiesList = pd.Activities;

            CreateAndAddTransitions(pd, pd.Activities.First(x => x.Name == "Start"), pd.Activities.First(x => x.Name == "ResourceCheck"), false);
            CreateAndAddTransitions(pd, pd.Activities.First(x => x.Name == "ResourceCheck"), pd.Activities.First(x => x.Name == "MergeBuild"), false);
            CreateAndAddTransitions(pd, pd.Activities.First(x => x.Name == "ResourceCheck"), pd.Activities.First(x => x.Name == "BuildMDU"), true);
            CreateAndAddTransitions(pd, pd.Activities.First(x => x.Name == "ResourceCheck"), pd.Activities.First(x => x.Name == "BuildSeriesStructure"), true);
            CreateMergeProcessTransition(pd, pd.Activities.First(x => x.Name == "BuildMDU"), pd.Activities.First(x => x.Name == "MergeBuild"));
            CreateMergeProcessTransition(pd, pd.Activities.First(x => x.Name == "BuildSeriesStructure"), pd.Activities.First(x => x.Name == "MergeBuild"));
            CreateCheckTransition(pd, pd.Activities.First(x => x.Name == "MergeBuild"), pd.Activities.First(x => x.Name == "BuildMDUAndSSStatus"));
            CreateAndAddTransitions(pd, pd.Activities.First(x => x.Name == "BuildMDUAndSSStatus"), pd.Activities.First(x => x.Name == "GetReady"), false);
            CreateAndAddTransitions(pd, pd.Activities.First(x => x.Name == "GetReady"), pd.Activities.First(x => x.Name == "PulmoGetReady"), true);
            CreateAndAddTransitions(pd, pd.Activities.First(x => x.Name == "GetReady"), pd.Activities.First(x => x.Name == "MergeReady"), false);
            CreateMergeProcessTransition(pd, pd.Activities.First(x => x.Name == "PulmoGetReady"), pd.Activities.First(x => x.Name == "MergeReady"), true);
            CreateAndAddTransitions(pd, pd.Activities.First(x => x.Name == "GetReady"), pd.Activities.First(x => x.Name == "CIRSGetReady"), true);
            CreateMergeProcessTransition(pd, pd.Activities.First(x => x.Name == "CIRSGetReady"), pd.Activities.First(x => x.Name == "MergeReady"), true);
            CreateAndAddTransitions(pd, pd.Activities.First(x => x.Name == "GetReady"), pd.Activities.First(x => x.Name == "GantryReady"), true);
            CreateMergeProcessTransition(pd, pd.Activities.First(x => x.Name == "GantryReady"), pd.Activities.First(x => x.Name == "MergeReady"), true);
            CreateAndAddTransitions(pd, pd.Activities.First(x => x.Name == "GetReady"), pd.Activities.First(x => x.Name == "ResultEngineGetReady"), true);
            CreateMergeProcessTransition(pd, pd.Activities.First(x => x.Name == "ResultEngineGetReady"), pd.Activities.First(x => x.Name == "MergeReady"), true);
            CreateAndAddTransitions(pd, pd.Activities.First(x => x.Name == "GetReady"), pd.Activities.First(x => x.Name == "InjectorGetReady"), true);
            CreateMergeProcessTransition(pd, pd.Activities.First(x => x.Name == "InjectorGetReady"), pd.Activities.First(x => x.Name == "MergeReady"), true);
            CreateAndAddTransitions(pd, pd.Activities.First(x => x.Name == "GetReady"), pd.Activities.First(x => x.Name == "CardiacGetReady"), true);
            CreateMergeProcessTransition(pd, pd.Activities.First(x => x.Name == "CardiacGetReady"), pd.Activities.First(x => x.Name == "MergeReady"), true);
            CreateCheckTransition(pd, pd.Activities.First(x => x.Name == "MergeReady"), pd.Activities.First(x => x.Name == "GetReadyStatus"));

            var result = WorkflowInit.Runtime.Builder.SaveProcessSchemeAsync(schemeCode, pd).Result;
            var processdefinition = XElement.Parse(pd.Serialize());
            return processdefinition;
        }

        public Task<XElement> GenerateAsync(string schemeCode, Guid schemeId, IDictionary<string, object> parameters)
        {
            return Task.Run(() => Generate(schemeCode, schemeId, parameters));
        }
    }
}
