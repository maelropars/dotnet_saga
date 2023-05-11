namespace DotNetSaga;

using Temporalio.Workflows;

[Workflow]
public class BookWorkflow
{
    public static readonly BookWorkflow Ref = WorkflowRefs.Create<BookWorkflow>();

    [WorkflowRun]
    public async Task<string> RunAsync(BookVacationInput input)
    {
        // This workflow just runs a simple activity to completion.
        // StartActivityAsync could be used to just start and there are many
        // other things that you can do inside a workflow.
        /*
        return await Workflow.ExecuteActivityAsync(
            // If the activity is static, we don't need Ref
            Activities.Ref.SayHello,
            name,
            new() { ScheduleToCloseTimeout = TimeSpan.FromMinutes(5) });
    }*/

        List<string> compensations = new List<string>();

        try
        {
            compensations.Add("undo_book_car");

            string output = await Workflow.ExecuteActivityAsync(Activities.Ref.BookCar, input, new() { ScheduleToCloseTimeout = TimeSpan.FromSeconds(10) });

            compensations.Add("undo_book_hotel");

            output += " " + await Workflow.ExecuteActivityAsync(Activities.Ref.BookHotel, input, new() { ScheduleToCloseTimeout = TimeSpan.FromSeconds(10) });

            // Sleep to simulate flight booking taking longer, allowing for worker restart while workflow running
            //await Task.Delay(T);
            await Workflow.DelayAsync(TimeSpan.FromSeconds(15));

            compensations.Add("undo_book_flight");

            output += " " + await Workflow.ExecuteActivityAsync(Activities.Ref.BookFlight, input, new()
            {
                ScheduleToCloseTimeout = TimeSpan.FromSeconds(10),
            });

            return output;
        }
        catch (Exception)
        {
            foreach (string compensation in compensations)
            {
                await ExecuteCompensationActivity(compensation, input);
            }

            return "Voyage cancelled";
        }
    }

    private async Task ExecuteCompensationActivity(string compensation, BookVacationInput input)
    {
        switch (compensation)
        {
            case "undo_book_car":
                await Workflow.ExecuteActivityAsync(Activities.Ref.UndoBookCar, input, new() { ScheduleToCloseTimeout = TimeSpan.FromSeconds(10) });
                break;
            case "undo_book_hotel":
                await Workflow.ExecuteActivityAsync(Activities.Ref.UndoBookHotel, input, new() { ScheduleToCloseTimeout = TimeSpan.FromSeconds(10) });
                break;
            case "undo_book_flight":
                await Workflow.ExecuteActivityAsync(Activities.Ref.UndoBookFlight, input, new() { ScheduleToCloseTimeout = TimeSpan.FromSeconds(10), RetryPolicy = new Temporalio.RetryPolicy { InitialInterval = TimeSpan.FromSeconds(1), MaximumInterval = TimeSpan.FromSeconds(1), MaximumAttempts = input.Attempts } });
                break;
        }
    }
}