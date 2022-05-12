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
            ActivityDefinition finalActivity)
        {
            string TransistionName = firstActivity.Name + "To" + finalActivity.Name;
            var Transition = TransitionDefinition.Create(
                TransistionName,
                TransitionClassifier.NotSpecified,
                ConcatenationType.And,
                ConcatenationType.And,
                ConcatenationType.And,
                firstActivity,
                finalActivity,
                TriggerDefinition.Auto,
                null
                );
            pd.Transitions.Add(Transition);
        }
        private static void CreateCommandTransitions(ProcessDefinition pd, ActivityDefinition firstActivity,
            ActivityDefinition finalActivity)
        {
            string TransistionName = firstActivity.Name + "To" + finalActivity.Name;
            var conditionList = new List<ConditionDefinition>() { ConditionDefinition.Always };
            var Transition = TransitionDefinition.Create(
                TransistionName,
                TransitionClassifier.NotSpecified,
                ConcatenationType.And,
                ConcatenationType.And,
                ConcatenationType.And,
                firstActivity,
                finalActivity,
                new TriggerDefinition(TriggerType.Command) { Command = pd.Commands.First() },
                conditionList
                );
            Transition.IsFork = true;
            Transition = Transition.SetSubprocessSettings("@Comment+\"_\"+@Test", "@SomeParameterContainingGuid", false, false, SubprocessInOutDefinition.Start,
                                                     SubprocessStartupType.AnotherThread, SubprocessStartupParameterCopyStrategy.CopyAll,
                                                     SubprocessFinalizeParameterMergeStrategy.OverwriteSpecified, null);
            //Transition.SubprocessName = "@Comment+\"_\"+@Test";
            pd.Transitions.Add(Transition);

        }
        private static void CreateEndWorkflowCommandTransitions(ProcessDefinition pd, ActivityDefinition firstActivity,
           ActivityDefinition finalActivity)
        {
            string TransistionName = firstActivity.Name + "To" + finalActivity.Name;
            var conditionList = new List<ConditionDefinition>() { ConditionDefinition.Always };
            var Transition = TransitionDefinition.Create(
                TransistionName,
                TransitionClassifier.NotSpecified,
                ConcatenationType.And,
                ConcatenationType.And,
                ConcatenationType.And,
                firstActivity,
                finalActivity,
                new TriggerDefinition(TriggerType.Command) { Command = pd.Commands.Last() },
                conditionList
                );
            Transition.IsFork = true;
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
            var pd = ProcessDefinition.Create(
                schemeCode + "SimpleProcess",
                false,
                new List<ActorDefinition>(),
                new List<ParameterDefinition>() { ParameterDefinition.Create("Comment", "String", "Temporary", "123"), ParameterDefinition.Create("Test", "Int16", "Temporary", "123") },
                new List<CommandDefinition>() { CommandDefinition.Create("CreateSubProcess"), CommandDefinition.Create("EndWorkflow") },
                new List<TimerDefinition>(),
                new List<ActivityDefinition>(),
                new List<TransitionDefinition>(),
                new List<LocalizeDefinition>(),
                new List<CodeActionDefinition>(),
                DesignerSettings.Empty,
                new List<string>()
                );

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

            CreateAndAddTransitions(pd, pd.Activities.First(x => x.Name == "Start"), pd.Activities.First(x => x.Name == "ResourceCheck"));
            CreateCommandTransitions(pd, pd.Activities.First(x => x.Name == "ResourceCheck"), pd.Activities.First(x => x.Name == "BuildMDU"));
            CreateAndAddTransitions(pd, pd.Activities.First(x => x.Name == "BuildMDU"), pd.Activities.First(x => x.Name == "BuildSeriesStructure"));
            CreateAndAddTransitions(pd, pd.Activities.First(x => x.Name == "BuildSeriesStructure"), pd.Activities.First(x => x.Name == "MergeBuild"));


            WorkflowInit.Runtime.Builder.SaveProcessSchemeAsync(schemeCode, pd);

            var pd1 = XElement.Parse(pd.Serialize());
            return pd1;

        }
        public Task<XElement> GenerateAsync(string schemeCode, Guid schemeId, IDictionary<string, object> parameters)
        {
            return Task.Run(() => Generate(schemeCode, schemeId, parameters));
        }
    }
}
