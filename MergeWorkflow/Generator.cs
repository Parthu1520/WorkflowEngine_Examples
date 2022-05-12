using OptimaJet.Workflow.Core.Generator;
using OptimaJet.Workflow.Core.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MergeWorkflow
{
    public class Generator:IWorkflowGenerator<XElement>
    {
        public XElement Generate(string schemeCode, Guid schemeId, IDictionary<string, object> parameters)
        {
            ProcessDefinition processDefinition = WorkflowInit.Runtime.Builder.GetProcessSchemeForDesignerAsync(schemeCode).Result;
            if (processDefinition == null)
            {
                 processDefinition = ProcessDefinition.Create("NewSimpleMPRProcess", false, new List<ActorDefinition>(), new List<ParameterDefinition>(), new List<CommandDefinition>(), new List<TimerDefinition>(), new List<ActivityDefinition>(), new List<TransitionDefinition>(), new List<LocalizeDefinition>(), new List<CodeActionDefinition>(), DesignerSettings.Empty, new List<string>());
                ActivityDefinition StartActivity = ActivityDefinition.Create("Start", "Start", true, false, true, true);
                ActivityDefinition Intermediate = ActivityDefinition.Create("Intermediate", "Intermediate", false, false, true, true);
                ActivityDefinition FinalActivity = ActivityDefinition.Create("Final", "Final", false, true, true, true);
                ActionDefinitionReference StartAction = ActionDefinitionReference.Create("Start", "0", null);
                ActionDefinitionReference FinalAction = ActionDefinitionReference.Create("Final", "0", null);
                ActionDefinitionReference IntermediateAction = ActionDefinitionReference.Create("Intermediate", "0", null);
                StartActivity.AddAction(StartAction);
                FinalActivity.AddAction(FinalAction);
                Intermediate.AddAction(IntermediateAction);
                processDefinition.Activities.Add(StartActivity);
                processDefinition.Activities.Add(FinalActivity);
                processDefinition.Activities.Add(Intermediate);
                (bool success, List<string> errors, string failedstep) = WorkflowInit.Runtime.Builder.SaveProcessSchemeAsync(schemeCode, processDefinition).Result;
            }
            
            var pd= XElement.Parse(processDefinition.Serialize());
            return pd;
        }
        public Task<XElement> GenerateAsync(string schemeCode, Guid schemeId, IDictionary<string, object> parameters)
        {
            return Task.Run(()=>Generate(schemeCode, schemeId, parameters));
        }
    }
}
