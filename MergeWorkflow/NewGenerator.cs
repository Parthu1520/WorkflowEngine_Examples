
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
            ActivityDefinition finalActivity, bool isSubprocess = false, bool isMerge = false, bool isSubProcessFinal = false,
            List<ConditionDefinition> conditions = null)
        {
            string TransistionName = firstActivity.Name + "To" + finalActivity.Name;
            TransitionDefinition Transition = TransitionDefinition.Create(
                TransistionName,
                TransitionClassifier.NotSpecified,
                ConcatenationType.And,
                ConcatenationType.And,
                ConcatenationType.And,
                firstActivity,
                finalActivity,
                TriggerDefinition.Auto,
                null);

            if (conditions != null)
            {
                Transition.Conditions = conditions;
            }

            if (isSubprocess)
            {
                Transition = Transition.SetSubprocessSettings(
                    TransistionName,
                    Guid.NewGuid().ToString(),
                    false,
                    false,
                    SubprocessInOutDefinition.Start,
                    SubprocessStartupType.AnotherThread,
                    SubprocessStartupParameterCopyStrategy.CopyAll,
                    SubprocessFinalizeParameterMergeStrategy.OverwriteSpecified,
                    null);

                Transition.IsFork = true;
            }

            if (isSubProcessFinal)
            {
                Transition = Transition.SetSubprocessSettings(TransistionName,
                    Guid.NewGuid().ToString(),
                    false,
                    false,
                    SubprocessInOutDefinition.Finalize,
                    SubprocessStartupType.AnotherThread,
                    SubprocessStartupParameterCopyStrategy.CopyAll,
                    SubprocessFinalizeParameterMergeStrategy.OverwriteAllNulls,
                    null);

                Transition.IsFork = true;
            }

            pd.Transitions.Add(Transition);
        }

        private static void CreateCommandTransitions(ProcessDefinition pd, ActivityDefinition firstActivity,
            ActivityDefinition finalActivity, bool isSubprocess, bool isMerge = false, bool isSubProcessFinal = false)
        {
            string TransistionName = firstActivity.Name + "To" + finalActivity.Name;
            var conditionList = new List<ConditionDefinition>();
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
                 new List<ParameterDefinition>(),new List<CommandDefinition>(),new List<TimerDefinition>(),
                 new List<ActivityDefinition>(), new List<TransitionDefinition>(), new List<LocalizeDefinition>(),
                 new List<CodeActionDefinition>(),DesignerSettings.Empty, new List<string>());
            object stageInfoList;
            parameters.TryGetValue("StagesInfo", out stageInfoList);
            bool isInitial = true, isFinal = false;
            pd.CanBeInlined = true;
            foreach (var stageInfo in (List<StageInfo>)stageInfoList)
            {
                if (stageInfo.Stage.StartsWith("D2"))
                {

                    isFinal = true;
                }
                CreateAndAddActivities(pd, stageInfo.Stage, isInitial, isFinal);
                isInitial = false;
            }
            int NoOfActivities = pd.Activities.Count;
            List<ActivityDefinition> ActivitiesList = pd.Activities;

           var conditions = new List<ConditionDefinition>
            {
                ConditionDefinition.CreateActionCondition(
                    ActionDefinitionReference.Create(
                        "CheckAllSubprocessesCompleted",
                        "0",
                        "{\"Mode\":\"AllSubprocessesAndParent\"}"),
                    false,
                    null)
            };

            //CreateAndAddTransitions(pd, pd.Activities.First(x => x.Name == "Start"), pd.Activities.First(x => x.Name == "ResourceCheck"));
            //CreateAndAddTransitions(pd, pd.Activities.First(x => x.Name == "ResourceCheck"), pd.Activities.First(x => x.Name == "BuildMDU"));
            //CreateAndAddTransitions(pd, pd.Activities.First(x => x.Name == "BuildMDU"), pd.Activities.First(x => x.Name == "MergeBuild"));

            for (int i = 0; i < NoOfActivities - 1; i++)
            {
                CreateAndAddTransitions(pd, ActivitiesList[i], ActivitiesList[i + 1]);
            }

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
