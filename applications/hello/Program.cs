var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var aUniqueIdThatIsSetOnlyOnceAtStartupOfTheApplication = Guid.NewGuid().ToString();

app.MapGet("/", async () =>
{
    await Task.Delay(500);
    return $"{aUniqueIdThatIsSetOnlyOnceAtStartupOfTheApplication}\nHello.";
});

app.MapGet("/{name}", async (string name) =>
{
    await Task.Delay(500);
    return $"{aUniqueIdThatIsSetOnlyOnceAtStartupOfTheApplication}\nHello, {name}.";
});

app.Run();
