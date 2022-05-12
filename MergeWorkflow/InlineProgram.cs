using OptimaJet.Workflow.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MergeWorkflow
{
    class InlineProgram
    {
        static string schemeCode = "SimpleMPRPipelineNew";

        static Guid? processId = null;
        static void Main(string[] args)
        {
            CreateInstanceParameters();
            Console.ReadLine();
        }

        private static void CreateInstanceParameters()
        {
            processId = Guid.NewGuid();
            try
            {
                var createInstanceParameters = new CreateInstanceParams(schemeCode, processId.Value)
                { };
                WorkflowInit.Runtime.CreateInstanceAsync(createInstanceParameters);
                Console.WriteLine("CreateInstance - OK.", processId);
            }
            catch (Exception ex)
            {
                Console.WriteLine("CreateInstance - Exception: {0}", ex.ToString());
                processId = null;
            }
        }       
    }
}