using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Model;

namespace MergeWorkflow
{
    public class CustomBuildStep : BuildStep
    {
        public override string Name => "CustomBuildStep";

        private static void CreateAndAddTransitions(ProcessDefinition pd, ActivityDefinition firstActivity,
            ActivityDefinition finalActivity, bool isSubprocess, List<ConditionDefinition> conditions = null)
        {
            string TransistionName = firstActivity.Name + "To" + finalActivity.Name;

            TransitionDefinition Transition = TransitionDefinition.Create(TransistionName,
                TransitionClassifier.NotSpecified, ConcatenationType.And, ConcatenationType.And, ConcatenationType.And,
                firstActivity, finalActivity, TriggerDefinition.Auto, null);
            if (conditions != null)
            {
                Transition.Conditions = conditions;
            }

            if (isSubprocess)
            {
                Transition.SetSubprocessSettings(TransistionName, Guid.NewGuid().ToString(),
                    false, false, SubprocessInOutDefinition.Start, SubprocessStartupType.AnotherThread,
                    SubprocessStartupParameterCopyStrategy.CopyAll,
                    SubprocessFinalizeParameterMergeStrategy.OverwriteSpecified, null);
                Transition.IsFork = true;
            }
            pd.Transitions.Add(Transition);

        }
        private BuildStepResult Execute(ProcessDefinition processDefinition, IDictionary<string, object> parameters)
        {
            var builder = Builder;
            var success = true;
            ProcessDefinition modifiedProcessDefinition = processDefinition;
            try
            {
                //get the inlined schemes (segments)
                var produceMPRSegment = processDefinition.Activities.FirstOrDefault(x => x.Name.StartsWith("ProduceMPRSegment"));
                if (produceMPRSegment == null)
                {
                    List<string> inlinedSchemes = builder.GetInlinedSchemeCodesAsync().Result;
                    ActivityDefinition StartActivity = null;
                    //ActivityDefinition AxialProcessingSegment = null;
                    ActivityDefinition FinalActivity = null;
                    ActivityDefinition Intermediate = null;
                    StartActivity = modifiedProcessDefinition.FindActivity("Start");
                    FinalActivity = modifiedProcessDefinition.FindActivity("Final");
                    Intermediate = modifiedProcessDefinition.FindActivity("Intermediate");
                    //AxialProcessingSegment = ActivityDefinition.CreateInlineActivity("AxialProcessing", inlinedSchemes.Find(a => a.StartsWith("ProduceMPRSegment")));
                    //modifiedProcessDefinition.Activities.Add(AxialProcessingSegment);
                    List<TransitionDefinition> translist = modifiedProcessDefinition.Transitions;
                    ConditionDefinition Condition =
              ConditionDefinition.CreateActionCondition(ActionDefinitionReference.Create("SelfTransition", "0", null),
                  false, null);
                    List<ConditionDefinition> conditonList = new List<ConditionDefinition>();
                    conditonList.Add(Condition);
                    if (StartActivity != null && FinalActivity != null)
                    {
                        CreateAndAddTransitions(modifiedProcessDefinition, StartActivity, Intermediate, false);
                        CreateAndAddTransitions(modifiedProcessDefinition, Intermediate, Intermediate, true, conditions: conditonList);
                        CreateAndAddTransitions(modifiedProcessDefinition, Intermediate, FinalActivity, false);
                    }
                }
            }
            catch (Exception ex)
            {
                return BuildStepResult.Fail(ex.Message);
            }

            if (success)
            {
                return BuildStepResult.Success(modifiedProcessDefinition);
            }
            else
            {
                return BuildStepResult.Fail("Some error message");
            }
        }

        public override Task<BuildStepResult> ExecuteAsync(ProcessDefinition processDefinition, IDictionary<string, object> parameters)
        {
            return Task.Run(() => Execute(processDefinition, parameters));
        }
    }
}
