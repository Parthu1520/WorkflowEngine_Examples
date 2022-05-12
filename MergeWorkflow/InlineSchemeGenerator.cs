using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Generator;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.DbPersistence;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MergeWorkflow
{
    class InlineSchemeGenerator : IWorkflowGenerator<XElement>
    {
        private static void CreateAndAddTransitions(ProcessDefinition pd, ActivityDefinition firstActivity, ActivityDefinition finalActivity)
        {
            string TransistionName = firstActivity.Name + "To" + finalActivity.Name;
            //ConditionDefinition Condition =
            //    ConditionDefinition.CreateActionCondition(ActionDefinitionReference.Create("StopCondition", "0", null),
            //        false, null);
            //List<ConditionDefinition> conditonList = new List<ConditionDefinition>();
            //conditonList.Add(Condition);
            TransitionDefinition Transition = TransitionDefinition.Create(TransistionName, TransitionClassifier.NotSpecified, ConcatenationType.And, ConcatenationType.And, ConcatenationType.And, firstActivity, finalActivity, TriggerDefinition.Auto, null);


            pd.Transitions.Add(Transition);

        }

        private static void CreateAndAddActivities(ProcessDefinition pd, String stageName, bool IsInitial, bool IsFinal)
        {
            ActivityDefinition newActivity = ActivityDefinition.Create(stageName, stageName, IsInitial, IsFinal, true, true);
            newActivity.AddAction(ActionDefinitionReference.Create(stageName, "0", null));
            pd.Activities.Add(newActivity);
        }

        private static void CreateInlineActivity(ProcessDefinition pd, String stageName, bool IsInitial, bool IsFinal)
        {
            ActivityDefinition newActivity = ActivityDefinition.Create(stageName, stageName, IsInitial, IsFinal, true, true);
            newActivity.ActivityType = ActivityType.Inline;
            newActivity.SchemeCode = "ProduceMPRSegment";
            newActivity.AddAction(ActionDefinitionReference.Create(stageName, "0", null));
            pd.Activities.Add(newActivity);
        }


        public XElement Generate(string schemeCode, Guid schemeId, IDictionary<string, object> parameters)
        {
            if (parameters.Count == 0)
            {
                string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    var scheme = WorkflowScheme.SelectByKeyAsync(connection, schemeCode);
                    if (scheme == null || string.IsNullOrEmpty(scheme.Result.Scheme))
                        throw SchemeNotFoundException.Create(schemeCode, SchemeLocation.WorkflowProcessScheme);

                    return XElement.Parse(scheme.Result.Scheme);
                }
            }
            var processDefinition = ProcessDefinition.Create(schemeCode + "SimpleProcess", false, new List<ActorDefinition>(),
                new List<ParameterDefinition>(), new List<CommandDefinition>(), new List<TimerDefinition>(),
                new List<ActivityDefinition>(), new List<TransitionDefinition>(), new List<LocalizeDefinition>(),
                new List<CodeActionDefinition>(), DesignerSettings.Empty, new List<string>());

            object stageInfoList;
            parameters.TryGetValue("StagesInfo", out stageInfoList);
            bool isInitial = true, isFinal = false;
            processDefinition.CanBeInlined = true;
           
            foreach (var stageInfo in (List<StageInfo>)stageInfoList)
            {
                if (stageInfo.Stage.StartsWith("OutputProcess"))
                {

                    isFinal = true;
                }

                if (stageInfo.Stage.StartsWith("Inline"))
                {
                    CreateInlineActivity(processDefinition, stageInfo.Stage, isInitial, isFinal);
                    isInitial = false;
                }
                else
                {
                    CreateAndAddActivities(processDefinition, stageInfo.Stage, isInitial, isFinal);
                    isInitial = false;
                }
                
            }
            int NoOfActivities = processDefinition.Activities.Count;
            List<ActivityDefinition> ActivitiesList = processDefinition.Activities;
            for (int i = 0; i < NoOfActivities - 1; i++)
            {
                CreateAndAddTransitions(processDefinition, ActivitiesList[i], ActivitiesList[i + 1]);
            }
            (bool success, List<string> errors, string failedstep) = WorkflowInit.Runtime.Builder
                .SaveProcessSchemeAsync(schemeCode, processDefinition).Result;
            var processDef = XElement.Parse(processDefinition.Serialize());
            return processDef;

        }
        public Task<XElement> GenerateAsync(string schemeCode, Guid schemeId, IDictionary<string, object> parameters)
        {           
            return Task.Run(() => Generate(schemeCode, schemeId, parameters));
        }


    }
}