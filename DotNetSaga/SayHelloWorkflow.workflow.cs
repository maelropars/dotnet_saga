namespace DotNetSaga;

using Temporalio.Workflows;

[Workflow]
public class SayHelloWorkflow
{
    // A "ref" is needed to access methods on this class in a type-safe way from
    // the client without instantiating the class
    public static readonly SayHelloWorkflow Ref = WorkflowRefs.Create<SayHelloWorkflow>();

    [WorkflowRun]
    public async Task<string> RunAsync(string name)
    {
        // This workflow just runs a simple activity to completion.
        // StartActivityAsync could be used to just start and there are many
        // other things that you can do inside a workflow.
        return await Workflow.ExecuteActivityAsync(
            // If the activity is static, we don't need Ref
            Activities.Ref.SayHello,
            name,
            new() { ScheduleToCloseTimeout = TimeSpan.FromMinutes(5) });
    }
}