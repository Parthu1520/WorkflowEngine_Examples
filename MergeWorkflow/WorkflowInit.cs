using System;
using System.Configuration;
using System.Reflection;
using System.Xml.Linq;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.DbPersistence;


namespace MergeWorkflow
{

    public class WorkflowInit
    {
        private static readonly Lazy<WorkflowRuntime> LazyRuntime = new Lazy<WorkflowRuntime>(InitWorkflowRuntime);

        public static WorkflowRuntime Runtime
        {
            get { return LazyRuntime.Value; }
        }

        public static string ConnectionString { get; set; }

        private static WorkflowRuntime InitWorkflowRuntime()
        {
            //TODO Uncomment for .NET Framework if you don't set ConnectionString externally.

            ConnectionString =ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

            if (string.IsNullOrEmpty(ConnectionString))
            {
                throw new Exception("Please init ConnectionString before calling the Runtime!");
            }
            //TODO If you have a license key, you have to register it here

            var licenseKey = ConfigurationManager.AppSettings["LicenseKey"].ToString();
            WorkflowRuntime.RegisterLicense(licenseKey);

            //TODO If you are using database different from SQL Server you have to use different persistence provider here.
            var dbProvider = new MSSQLProvider(ConnectionString);

            var builder = new WorkflowBuilder<XElement>(
                new CommandGenerator(),
                new OptimaJet.Workflow.Core.Parser.XmlWorkflowParser(),
                dbProvider
            );

           // builder.AddBuildStep(0, BuildStepPosition.BeforeSystemSteps, new CustomBuildStep());

            var runtime = new WorkflowRuntime()
                .WithBuilder(builder)
                .WithPersistenceProvider(dbProvider)
                .EnableCodeActions()
                .SwitchAutoUpdateSchemeBeforeGetAvailableCommandsOn()
                .AsSingleServer();
            runtime.CancellationTokenHandling = CancellationTokenHandling.Throw;
            //events subscription
            runtime.OnProcessActivityChanged += (sender, args) => { };
            runtime.OnProcessStatusChanged += (sender, args) =>
            {
                // System.Console.WriteLine(string.Format("The Status of the Porcess {0} is {1}",args.ProcessId,args.NewStatus.Name));
            };

            runtime.BPMNApi.CustomDownload += (definition, scheme) =>
            {
                var a = scheme.Serialize();
                return a;
            };
            runtime.OnWorkflowError += (sender, args) =>
            {
                Exception exception = args.Exception;
                ProcessInstance processInstance = args.ProcessInstance;
                TransitionDefinition executedTransition = args.ExecutedTransition;
            };

            #region Single Code Action For all stages combined

            //runtime.EnableCodeActions();
            runtime.RegisterAssemblyForCodeActions(
                 Assembly.GetAssembly(typeof(ProcessingStagesActionProvider)));

            runtime.WithActionProvider(ProcessingStagesActionProvider.GetActionProvider());

            #endregion

            //TODO If you have planned to use Code Actions functionality that required references to external assemblies you have to register them here
            //runtime.RegisterAssemblyForCodeActions(Assembly.GetAssembly(typeof(SomeTypeFromMyAssembly)));

            //starts the WorkflowRuntime
            //TODO If you have planned use Timers the best way to start WorkflowRuntime is somwhere outside of this function in Global.asax for example
            //runtime.Start();

            return runtime;
        }
    }
}
