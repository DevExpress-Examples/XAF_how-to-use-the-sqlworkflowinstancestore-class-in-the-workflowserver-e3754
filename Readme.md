<!-- default badges list -->
![](https://img.shields.io/endpoint?url=https://codecentral.devexpress.com/api/v1/VersionRange/128594823/11.2.7%2B)
[![](https://img.shields.io/badge/Open_in_DevExpress_Support_Center-FF7200?style=flat-square&logo=DevExpress&logoColor=white)](https://supportcenter.devexpress.com/ticket/details/E3754)
[![](https://img.shields.io/badge/📖_How_to_use_DevExpress_Examples-e9f6fc?style=flat-square)](https://docs.devexpress.com/GeneralInformation/403183)
<!-- default badges end -->
<!-- default file list -->
*Files to look at*:

* [Program.cs](./CS/WorkflowDemo.Win/Program.cs)
<!-- default file list end -->
# How to use the SqlWorkflowInstanceStore class in the WorkflowServer


<p>The main advantage of WorkflowInstanceStore is that it can work with any DBMS supported by <a href="http://documentation.devexpress.com/#XPO/CustomDocument1998"><u>XPO</u></a> (see <a href="http://documentation.devexpress.com/#XPO/CustomDocument2114"><u>Database Systems Supported by XPO</u></a>). If you only need to work with the MS SQL Server, as it often happens in case of the Azure platform, you can use the standard <a href="http://msdn.microsoft.com/en-us/library/ee383994.aspx"><u>SqlWorkflowInstanceStore</u></a>. To use SqlWorkflowInstanceStore in an XAF Workflow Server application, first create a database, as it is described in the <a href="http://msdn.microsoft.com/en-us/library/ee395773.aspx"><u>How to: Enable SQL Persistence for Workflows and Workflow Services</u></a> article. Then, replace the default instance store with SqlWorkflowInstanceStore in XAF WorkflowServer.</p>


```cs
           server.CustomizeHost += delegate(object sender, CustomizeHostEventArgs e) {
               e.WorkflowInstanceStoreBehavior = null;
               System.ServiceModel.Activities.Description.SqlWorkflowInstanceStoreBehavior sqlWorkflowInstanceStoreBehavior = new System.ServiceModel.Activities.Description.SqlWorkflowInstanceStoreBehavior("Integrated Security=SSPI;Pooling=false;Data Source=.\\SqlExpress;Initial Catalog=SqlWorkflowInstanceStoreDB");
               sqlWorkflowInstanceStoreBehavior.RunnableInstancesDetectionPeriod = TimeSpan.FromSeconds(2);
               e.Host.Description.Behaviors.Add(sqlWorkflowInstanceStoreBehavior);
               //e.WorkflowInstanceStoreBehavior.RunnableInstancesDetectionPeriod = TimeSpan.FromSeconds(2);
               e.WorkflowIdleBehavior.TimeToPersist = TimeSpan.FromSeconds(1);
           };
and exclude WorkflowInstanceStore classes from the persistent classes list:
           WorkflowDemoWindowsFormsApplication xafApplication = new WorkflowDemoWindowsFormsApplication();
           WorkflowModule workflowModule = xafApplication.Modules.FindModule<WorkflowModule>();
           workflowModule.WorkflowInstanceType = null;
           workflowModule.WorkflowInstanceKeyType = null;
```


<p><strong>See Also:<br> </strong><a href="http://msdn.microsoft.com/en-us/library/ee383994.aspx"><u>SqlWorkflowInstanceStore</u></a><br> <a href="http://msdn.microsoft.com/en-us/library/ee395773.aspx"><u>How to: Enable SQL Persistence for Workflows and Workflow Services</u></a><br> <a href="http://documentation.devexpress.com/#Xaf/CustomDocument3343"><u>Workflow Module</u></a><br> <a href="http://documentation.devexpress.com/#XPO/CustomDocument1998"><u>XPO</u></a><br> <a href="http://documentation.devexpress.com/#XPO/CustomDocument2114"><u>Database Systems Supported by XPO<br><a href="https://www.devexpress.com/Support/Center/p/Q364738">Questions about Workflow host deployment to Windows Azure</a><br></u></a></p>

<br/>


