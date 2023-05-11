using DotNetSaga;
using Temporalio.Client;

// Create a client to localhost on "default" namespace
var client = await TemporalClient.ConnectAsync(new("localhost:7233"));

var input = new BookVacationInput(
    BookUserId: "Jim",
    BookCarId: "Volvo",
    BookHotelId: "CitizenM",
    BookFlightId: "SQ333",
    Attempts: 5
);

// Run workflow
var result = await client.ExecuteWorkflowAsync(
    BookWorkflow.Ref.RunAsync,
    input,
    new(id: "my-workflow-id", taskQueue: "my-booking-queue"));

Console.WriteLine("Workflow result: {0}", result);