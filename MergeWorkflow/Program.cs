using OptimaJet.Workflow.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;

namespace MergeWorkflow
{
    class Program
    {
        private static System.Timers.Timer timer;
        static Guid processId;
        static void Main(string[] args)
        {
            //var input = Console.ReadLine();
            var input = "1";
            Pipeline pipeline = null;
            if (input == "0")
            {
                pipeline = GetPipeline().First();
            }
            else
            {
                pipeline = GetPipeline().Last();
            }
            try
            {
                var schemeCreationParameters = new Dictionary<string, object>() { { "StagesInfo", pipeline.Stages } };
                processId = Guid.NewGuid();
                var createInstanceParameters = new CreateInstanceParams(pipeline.Name, processId)
                {
                    SchemeCreationParameters = schemeCreationParameters,
                    IdentityId = "123",
                    ImpersonatedIdentityId = "456"
                };
                WorkflowInit.Runtime.SetSchemeIsObsolete(pipeline.Name, schemeCreationParameters);
                WorkflowInit.Runtime.WithExternalParametersProvider(new ExternalParameterProvider());
                WorkflowInit.Runtime.CreateInstanceAsync(createInstanceParameters);
                //WorkflowInit.Runtime.CreateInstance(pipeline.Name,processId);

                if (input == "0")
                {

                    timer = new System.Timers.Timer();
                    timer.Interval = 5000;
                    timer.Elapsed += OnTimedEvent;
                    timer.AutoReset = true;
                    timer.Enabled = true;
                }

                Console.ReadLine();

            }
            catch (System.Exception)
            {

                throw;
            }
        }

        //static WorkflowCommand command;
        static int count = 1;
        private static void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            try
            {
                var processInstanceTree = WorkflowInit.Runtime.GetProcessInstanceAndFillProcessParameters(processId);
                Console.WriteLine("HAshcode of ProcesSInstance from Timer ; " + processInstanceTree.GetHashCode());
                // WorkflowInit.Runtime.SetPersistentProcessParameter(processId, "TestParam", "MyValue");
                // WorkflowInit.Runtime.ExternalParametersProvider.SetExternalParameter("TestParam", "MyValue", processInstanceTree);

                //if (command == null)
                //{
                WorkflowCommand command = WorkflowInit.Runtime.GetAvailableCommands(processId, "").First();
                //}
                if (command != null)
                {
                    command.Parameters.Add(new CommandParameter() { ParameterName = "Comment", Value = "Process" + count, IsPersistent = false, TypeName = typeof(string).ToString() });
                    command.Parameters.Add(new CommandParameter() { ParameterName = "Test", Value = count, IsPersistent = false, TypeName = typeof(Int16).ToString() });
                    command.Parameters.Add(new CommandParameter() { ParameterName = "SomeParameterContainingGuid", Value = Guid.NewGuid(), IsPersistent = false, TypeName = typeof(Guid).ToString() });
                    count = count + 1;
                    Console.WriteLine(command.ProcessId.ToString());
                    WorkflowInit.Runtime.ExecuteCommand(command, "", "");

                }
                //command = null;
                count = count + 1;
            }
            catch (Exception ex)
            {
                // throw;
            }
        }
        private static List<Pipeline> GetPipeline()
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
            var pipe1 = new Pipeline
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
                                Id = "GetReady",
                                Stage = "GetReady",

                            },
                            
                            //To show you how CheckAllSubprocessesCompleted works
                            new StageInfo
                            {
                                Id = "GetReadyStatus",
                                Stage = "GetReadyStatus",
                            }
                        }

            };

            var inlinePipe = new Pipeline
            {
                Id = "InlineWorkflow",
                Name = "InlineWorkflow",
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
                                Id = "Final" ,
                                Stage = "Final" ,
                            },

                        }

            };

            var axialProcessingSegment = new Pipeline
            {
                Id = "InlineAxialProcessingSegment",
                Name = "InlineAxialProcessingSegment",
                Stages = new List<StageInfo>
                        {
                            new StageInfo
                            {
                                Id = "F1" ,
                                Stage = "F1" ,
                            }, new StageInfo
                            {
                                Id = "F2" ,
                                Stage = "F2" ,
                            },
                              new StageInfo
                            {
                                Id = "F3" ,
                                Stage = "F3" ,
                            },
new StageInfo
                            {
                                Id = "D1" ,
                                Stage = "D1" ,
                            },
new StageInfo
                            {
                                Id = "D2" ,
                                Stage = "D2" ,
                            }

                        }

            };

            return new List<Pipeline> { pipe, pipe1, inlinePipe, axialProcessingSegment };
        }
    }
}
