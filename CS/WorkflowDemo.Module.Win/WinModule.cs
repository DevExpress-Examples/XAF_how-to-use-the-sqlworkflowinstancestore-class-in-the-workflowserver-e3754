using System.ComponentModel;

using DevExpress.ExpressApp;

namespace WorkflowDemo.Module.Win
{
    [ToolboxItemFilter("Xaf.Platform.Win")]
    public sealed partial class WorkflowDemoWindowsFormsModule : ModuleBase
    {
		public WorkflowDemoWindowsFormsModule()
        {
            InitializeComponent();
        }
    }
}
