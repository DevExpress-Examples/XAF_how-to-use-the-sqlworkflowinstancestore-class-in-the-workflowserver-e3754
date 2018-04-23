using System;
using System.ComponentModel;

using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Model.Core;
using DevExpress.Workflow.Xpo;
using DevExpress.ExpressApp.Workflow.Xpo;
using WorkflowDemo.Module.Objects;

namespace WorkflowDemo.Module {
	public sealed partial class WorkflowDemoModule : ModuleBase {
		public WorkflowDemoModule() {
			InitializeComponent();
            this.AdditionalExportedTypes.Add(typeof(Issue));
            this.AdditionalExportedTypes.Add(typeof(Task));
            this.AdditionalExportedTypes.Add(typeof(DevExpress.Persistent.BaseImpl.User));
            this.AdditionalExportedTypes.Add(typeof(DevExpress.Persistent.BaseImpl.Role));
            this.RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Workflow.WorkflowModule));
            this.RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Security.SecurityModule));
            this.RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Objects.BusinessClassLibraryCustomizationModule));
        }
	}
}
