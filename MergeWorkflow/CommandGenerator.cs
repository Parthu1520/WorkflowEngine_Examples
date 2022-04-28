using OptimaJet.Workflow.Core.Generator;
using OptimaJet.Workflow.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MergeWorkflow
{
    public class CommandGenerator : IWorkflowGenerator<XElement>
    {
        private static void CreateAndAddTransitions(ProcessDefinition pd, ActivityDefinition firstActivity,
            ActivityDefinition finalActivity, bool isSubprocess, bool isMerge = false, bool isSubProcessFinal = false)
        {
            string TransistionName = firstActivity.Name + "To" + finalActivity.Name;
            TransitionDefinition Transition = TransitionDefinition.Create(TransistionName,
                                                                          TransitionClassifier.NotSpecified,
                                                                          ConcatenationType.And,
                                                                          ConcatenationType.And,
                                                                          ConcatenationType.And,
                                                                          firstActivity,
                                                                          finalActivity,
                                                                          TriggerDefinition.Auto,
                                                                          null);
            pd.Transitions.Add(Transition);
        }

        private static void CreateCommandTransitions(ProcessDefinition pd, ActivityDefinition firstActivity,
            ActivityDefinition finalActivity, bool isSubprocess, bool isMerge = false, bool isSubProcessFinal = false)
        {
            string TransistionName = firstActivity.Name + "To" + finalActivity.Name;
            var conditionList = new List<ConditionDefinition>();
            //ConditionDefinition Condition = ConditionDefinition.CreateActionCondition(ActionDefinitionReference.Create("CheckAllSubprocessesCompleted", "0",
            //    ""), false, null);
            //Condition.Type = ConditionType.Action;
            //conditionList.Add(Condition);
            TransitionDefinition Transition = TransitionDefinition.Create(TransistionName,
                                                                          TransitionClassifier.NotSpecified,
                                                                          ConcatenationType.And,
                                                                          ConcatenationType.And,
                                                                          ConcatenationType.And,
                                                                          firstActivity,
                                                                          finalActivity,
                                                                          TriggerDefinition.Auto,
                                                                          conditionList);
            // Transition.Trigger.Type = TriggerType.Command;
            Transition.Trigger.Command = pd.Commands.First();
            Transition.IsFork = true;
            Transition = Transition.SetSubprocessSettings(TransistionName,
                                                     Guid.NewGuid().ToString(),
                                                     false,
                                                     false,
                                                     SubprocessInOutDefinition.Start,
                                                     SubprocessStartupType.AnotherThread,
                                                     SubprocessStartupParameterCopyStrategy.CopyAll,
                                                     SubprocessFinalizeParameterMergeStrategy.OverwriteAllNulls,
                                                     null);
            //Transition.Conditions = conditionList;

            pd.Transitions.Add(Transition);

        }



        private static void CreateAndAddActivities(ProcessDefinition pd, String stageName, bool IsInitial, bool IsFinal)
        {

            ActivityDefinition newActivity = ActivityDefinition.Create(stageName, stageName, IsInitial, IsFinal, true, true);
            newActivity.AddAction(ActionDefinitionReference.Create(stageName, "0", null));
            pd.Activities.Add(newActivity);
        }


        public XElement Generate(string schemeCode, Guid schemeId, IDictionary<string, object> parameters)
        {
            var pd = ProcessDefinition.Create(schemeCode + "SimpleProcess", false, new List<ActorDefinition>(),
                new List<ParameterDefinition>(), new List<CommandDefinition>() { CommandDefinition.Create("CheckAllSubprocessesCompleted") }, new List<TimerDefinition>(),
                new List<ActivityDefinition>(), new List<TransitionDefinition>(), new List<LocalizeDefinition>(),
                new List<CodeActionDefinition>()
                {

                }, DesignerSettings.Empty, new List<string>());

            object stageInfoList;
            parameters.TryGetValue("StagesInfo", out stageInfoList);

            bool isInitial = true, isFinal = false;

            foreach (var stageInfo in (List<StageInfo>)stageInfoList)
            {
                CreateAndAddActivities(pd, stageInfo.Stage, isInitial, isFinal);
                isInitial = false;
                isFinal = false;
            }
            int NoOfActivities = pd.Activities.Count;
            List<ActivityDefinition> ActivitiesList = pd.Activities;

            CreateAndAddTransitions(pd, pd.Activities.First(x => x.Name == "Start"), pd.Activities.First(x => x.Name == "ResourceCheck"), false);
            CreateCommandTransitions(pd, pd.Activities.First(x => x.Name == "ResourceCheck"), pd.Activities.First(x => x.Name == "BuildMDU"), true);
            CreateAndAddTransitions(pd, pd.Activities.First(x => x.Name == "BuildMDU"), pd.Activities.First(x => x.Name == "BuildSeriesStructure"), false);
            CreateAndAddTransitions(pd, pd.Activities.First(x => x.Name == "BuildSeriesStructure"), pd.Activities.First(x => x.Name == "MergeBuild"), false);


            (bool success, List<string> errors, string failedstep) = WorkflowInit.Runtime.Builder
                .SaveProcessSchemeAsync(schemeCode, pd).Result;

            var pd1 = XElement.Parse(pd.Serialize());
            return pd1;

        }
        public Task<XElement> GenerateAsync(string schemeCode, Guid schemeId, IDictionary<string, object> parameters)
        {
            return Task.Run(() => Generate(schemeCode, schemeId, parameters));
        }
    }
}
