using OptimaJet.Workflow.Core.Runtime;
using System;
using System.Collections.Generic;

namespace MergeWorkflow
{
    class Program
    {
        static void Main(string[] args)
        {
            var pipeline = GetPipeline();
            try
            {
                var schemeCreationParameters = new Dictionary<string, object>() { { "StagesInfo", pipeline.Stages } };
                var processId = Guid.NewGuid();
                var createInstanceParameters = new CreateInstanceParams(pipeline.Name, processId)
                {
                    SchemeCreationParameters = schemeCreationParameters,
                    IdentityId = "123",
                    ImpersonatedIdentityId = "456"
                };
                WorkflowInit.Runtime.SetSchemeIsObsolete(pipeline.Name, schemeCreationParameters);
                WorkflowInit.Runtime.CreateInstanceAsync(createInstanceParameters);
                Console.ReadLine();
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        private static Pipeline GetPipeline()
        {
            var pipe = new Pipeline
            {
                Id = "MergeWorkflow",
                Name = "MergeWorkflow",
                Stages = new List<StageInfo>
                        {
                            new StageInfo
                            {
                                Id = "Start" ,
                                Stage = "Start" ,
                            }, new StageInfo
                            {
                                Id = "ResourceCheck" ,
                                Stage = "ResourceCheck" ,
                            },
                             new StageInfo
                            {
                                Id = "MergeBuild" ,
                                Stage = "MergeBuild" ,
                            }, new StageInfo
                            {
                                Id = "MergeReady" ,
                                Stage = "MergeReady" ,
                            },
                            new StageInfo
                            {
                                Id = "BuildMDU",
                                Stage = "BuildMDU",
                            },
                            new StageInfo
                            {
                                Id = "BuildSeriesStructure",
                                Stage = "BuildSeriesStructure",
                            },
                            new StageInfo
                            {
                                Id = "BuildMDUAndSSStatus",
                                Stage = "BuildMDUAndSSStatus",
                            },
                            new StageInfo
                            {
                                Id = "GetReady",
                                Stage = "GetReady",
                            },
                            new StageInfo
                            {
                                Id = "PulmoGetReady",
                                Stage = "PulmoGetReady",
                            },
                            new StageInfo
                            {
                                Id = "GantryReady",
                                Stage = "GantryReady",
                            },
                            new StageInfo
                            {
                                Id = "CIRSGetReady",
                                Stage = "CIRSGetReady",
                            },
                            new StageInfo
                            {
                                Id = "ResultEngineGetReady",
                                Stage = "ResultEngineGetReady",
                            },
                            new StageInfo
                            {
                                Id = "InjectorGetReady",
                                Stage = "InjectorGetReady",
                            },
                            new StageInfo
                            {
                                Id = "CardiacGetReady",
                                Stage = "CardiacGetReady",
                            },
                            new StageInfo
                            {
                                Id = "GetReadyStatus",
                                Stage = "GetReadyStatus",
                            }
                        }

            };

            return pipe;
        }
    }
}
