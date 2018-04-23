using System;
using System.Configuration;
using System.Windows.Forms;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Workflow.Server;
using DevExpress.ExpressApp.Workflow.Xpo;
using DevExpress.ExpressApp.Workflow.CommonServices;
using DevExpress.ExpressApp.Workflow.Versioning;
using WorkflowDemo.Module.Win;
using WorkflowDemo.Module;
using DevExpress.ExpressApp.Workflow.Win;
using DevExpress.ExpressApp.Workflow;
using WorkflowDemo.Module.Activities;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Data.Filtering;
using DevExpress.Workflow.Store;
using DevExpress.ExpressApp.MiddleTier;

namespace WorkflowDemo.Win
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
		[STAThread]
        static void Main(string[] arguments)
        {
            
            Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			EditModelPermission.AlwaysGranted = System.Diagnostics.Debugger.IsAttached;
			WorkflowDemoWindowsFormsApplication xafApplication = new WorkflowDemoWindowsFormsApplication();
            WorkflowModule workflowModule = xafApplication.Modules.FindModule<WorkflowModule>();
            workflowModule.WorkflowInstanceType = null;
            workflowModule.WorkflowInstanceKeyType = null;
            
#if EASYTEST
			try {
				DevExpress.ExpressApp.Win.EasyTest.EasyTestRemotingRegistration.Register();
			}
			catch(Exception) { }
#endif

            if(ConfigurationManager.ConnectionStrings["ConnectionString"] != null) {
				xafApplication.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
			}
#if EASYTEST
            if(ConfigurationManager.ConnectionStrings["EasyTestConnectionString"] != null) {
                xafApplication.ConnectionString = ConfigurationManager.ConnectionStrings["EasyTestConnectionString"].ConnectionString;
            }
#endif
            xafApplication.Modules.FindModule<WorkflowWindowsFormsModule>().QueryAvailableActivities += delegate(object sender, ActivitiesInformationEventArgs e) {
                e.ActivitiesInformation.Add(new ActivityInformation(typeof(CreateTask), "Code Activities", "Create Task", ImageLoader.Instance.GetImageInfo("CreateTask").Image));
            };

            xafApplication.LastLogonParametersReading += new EventHandler<LastLogonParametersReadingEventArgs>(xafApplication_LastLogonParametersReading);
            WorkflowServerStarter starter = null;
            xafApplication.LoggedOn += delegate(object sender, LogonEventArgs e) {
                if(starter == null) {
                    starter = new WorkflowServerStarter();
                    starter.OnCustomHandleException += delegate(object sender1, ExceptionEventArgs args1) {
                        MessageBox.Show(args1.Message);
                    };

                    starter.Start(xafApplication.ConnectionString, xafApplication.ApplicationName);
                }
            };

            try
            {
                xafApplication.Setup();
				xafApplication.Start();
            }
            catch (Exception e)
            {
				xafApplication.HandleException(e);
            }

            if(starter != null) {
                starter.Stop();
            }
        }

        static void xafApplication_LastLogonParametersReading(object sender, LastLogonParametersReadingEventArgs e) {
            if(string.IsNullOrEmpty(e.SettingsStorage.LoadOption("", "UserName"))) {
                e.SettingsStorage.SaveOption("", "UserName", "Sam");
            }
        }
    }

    [Serializable]
    public class ExceptionEventArgs : EventArgs {
        public ExceptionEventArgs(string message) {
            this.Message = message;
        }
        public string Message { get; private set; }
    }

    public class WorkflowServerStarter : MarshalByRefObject {
        private class ServerApplication : XafApplication {
            protected override DevExpress.ExpressApp.Layout.LayoutManager CreateLayoutManagerCore(bool simple) {
                throw new NotImplementedException();
            }
            public void Logon() {
                base.Logon(null);
            }
        }
        private static WorkflowServerStarter starter;
        private WorkflowServer server;
        private AppDomain domain;
        public void Start(string connectionString, string applicationName) {
            try {
                domain = AppDomain.CreateDomain("ServerDomain");
                starter = (WorkflowServerStarter)domain.CreateInstanceAndUnwrap(
                    System.Reflection.Assembly.GetEntryAssembly().FullName, typeof(WorkflowServerStarter).FullName);
				starter.OnCustomHandleException_ += new EventHandler<ExceptionEventArgs>(starter_OnCustomHandleException_);
				starter.Start_(connectionString, applicationName);
            }
            catch(Exception e) {
                Tracing.Tracer.LogError(e);
                if(OnCustomHandleException != null) {
                    OnCustomHandleException(null, new ExceptionEventArgs("Exception occurs:\r\n\r\n" + e.Message));
                }
            }
        }

        void starter_OnCustomHandleException_(object sender, ExceptionEventArgs e) {
            if(OnCustomHandleException != null) {
                OnCustomHandleException(null, e);
            }
        }
        public void Stop() {
            starter.Stop_();
            if(domain != null) {
                AppDomain.Unload(domain);
            }
        }
        private void Stop_() {
            server.Stop();
        }
        private void Start_(string connectionString, string applicationName) {
            ServerApplication serverApplication = new ServerApplication();
            serverApplication.ApplicationName = applicationName;
            serverApplication.Modules.Add(new WorkflowDemoModule());
            serverApplication.ConnectionString = connectionString;
            serverApplication.Security = new SecurityComplex<User, Role>(
                new WorkflowServerAuthentication(new BinaryOperator("UserName", "WorkflowService")));
            serverApplication.Setup();
            serverApplication.Logon();

            IObjectSpaceProvider objectSpaceProvider = serverApplication.ObjectSpaceProvider;

            server = new WorkflowServer("http://localhost:46232", objectSpaceProvider, objectSpaceProvider);
            server.WorkflowDefinitionProvider = new WorkflowVersionedDefinitionProvider<XpoWorkflowDefinition, XpoUserActivityVersion>(objectSpaceProvider, null);
            server.StartWorkflowListenerService.DelayPeriod = TimeSpan.FromSeconds(5);
            server.StartWorkflowByRequestService.DelayPeriod = TimeSpan.FromSeconds(5);
            server.RefreshWorkflowDefinitionsService.DelayPeriod = TimeSpan.FromSeconds(30);
            server.CustomizeHost += delegate(object sender, CustomizeHostEventArgs e) {
                e.WorkflowInstanceStoreBehavior = null;
                System.ServiceModel.Activities.Description.SqlWorkflowInstanceStoreBehavior sqlWorkflowInstanceStoreBehavior = new System.ServiceModel.Activities.Description.SqlWorkflowInstanceStoreBehavior("Integrated Security=SSPI;Pooling=false;Data Source=.\\SqlExpress;Initial Catalog=SqlWorkflowInstanceStoreDB");
                sqlWorkflowInstanceStoreBehavior.RunnableInstancesDetectionPeriod = TimeSpan.FromSeconds(2);
                e.Host.Description.Behaviors.Add(sqlWorkflowInstanceStoreBehavior);
                e.WorkflowIdleBehavior.TimeToPersist = TimeSpan.FromSeconds(1);
				//e.WorkflowInstanceStoreBehavior.RunnableInstancesDetectionPeriod = TimeSpan.FromSeconds(2);
			};

            server.CustomHandleException += delegate(object sender, CustomHandleServiceExceptionEventArgs e) {
                Tracing.Tracer.LogError(e.Exception);
                if(OnCustomHandleException_ != null) {
                    OnCustomHandleException_(this, new ExceptionEventArgs("Exception occurs:\r\n\r\n" + e.Exception.Message + "\r\n\r\n'" + e.Service.GetType() + "' service"));
                }
                e.Handled = true;
            };
			server.Start();
		}
        public event EventHandler<ExceptionEventArgs> OnCustomHandleException_;
        public event EventHandler<ExceptionEventArgs> OnCustomHandleException;
    }
}
