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
    class SelfGenerator : IWorkflowGenerator<XElement>
    {
        static int count = 0;
        private static void CreateAndAddTransitions(ProcessDefinition pd, ActivityDefinition firstActivity,
            ActivityDefinition finalActivity,
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
                 new List<ParameterDefinition>(), new List<CommandDefinition>(), new List<TimerDefinition>(),
                 new List<ActivityDefinition>(), new List<TransitionDefinition>(), new List<LocalizeDefinition>(),
                 new List<CodeActionDefinition>(), DesignerSettings.Empty, new List<string>());
            object stageInfoList;
            parameters.TryGetValue("StagesInfo", out stageInfoList);
            bool isInitial = true, isFinal = false;
            pd.CanBeInlined = true;
            foreach (var stageInfo in (List<StageInfo>)stageInfoList)
            {
                if (stageInfo.Stage.StartsWith("Final"))
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
                        "SelfTransition",
                        "0",
                        null),
                    false,
                    null)
            };

            CreateAndAddTransitions(pd, pd.Activities.First(x => x.Name == "Start"), pd.Activities.First(x => x.Name == "Intermediate"));
            CreateAndAddTransitions(pd, pd.Activities.First(x => x.Name == "Intermediate"), pd.Activities.First(x => x.Name == "Intermediate"),conditions);
            CreateAndAddTransitions(pd, pd.Activities.First(x => x.Name == "Intermediate"), pd.Activities.First(x => x.Name == "Final"));

            //for (int i = 0; i < NoOfActivities - 1; i++)
            //{
            //    CreateAndAddTransitions(pd, ActivitiesList[i], ActivitiesList[i + 1]);
            //}

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
