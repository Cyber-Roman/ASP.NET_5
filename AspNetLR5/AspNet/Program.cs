
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDistributedMemoryCache(); // служба кешування

var app = builder.Build();

DateTime currentTime = DateTime.Now;
var ErrorLogsFilePath = Path.Combine(Directory.GetCurrentDirectory(), "lr5/ErrorsLogs.txt");
app.UseDeveloperExceptionPage();

app.Map("/", (context) =>
{
    var htmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "lr5/index.html");

    if (File.Exists(htmlFilePath))
    {
        var htmlContent = File.ReadAllText(htmlFilePath);

        return context.Response.WriteAsync(htmlContent);
    }
    else
    {
        File.WriteAllText(ErrorLogsFilePath, "File not found. Error 404 " + currentTime);
        context.Response.StatusCode = 404;
        return Task.CompletedTask;
    }
});

app.Map("/SubmitForm", (context) =>
{
    if (context.Request.Method == "POST")
    {
        var valueInput = context.Request.Form["valueInput"];
        var dateTimeInput = DateTime.Parse(context.Request.Form["dateTimeInput"]);

        var cookieOptions = new CookieOptions
        {
            Expires = dateTimeInput,
            HttpOnly = false
        };

        File.WriteAllText(ErrorLogsFilePath, "The data was successfully saved in cookies.  " + currentTime);
        context.Response.Cookies.Append("formData", valueInput, cookieOptions);
        return context.Response.WriteAsync("The data was successfully saved in cookies.");
    }
    else
    {
        File.WriteAllText(ErrorLogsFilePath, "Incorrect method of recording. " + currentTime);
        return context.Response.WriteAsync("Incorrect method of recording.");
    }
});

app.Map("/CheckCookie", (context) =>
{
    if (context.Request.Cookies.TryGetValue("formData", out var formData))
    {
        return context.Response.WriteAsync($"Value found in cookies: {formData}");
    }
    else
    {
        File.WriteAllText(ErrorLogsFilePath, "Value not found in cookies. " + currentTime);
        return context.Response.WriteAsync("Value not found in cookies.");
    }
});

app.UseExceptionHandler(app => app.Run(async context =>
{
    File.WriteAllText(ErrorLogsFilePath, "Some exeption has occured. " + currentTime);
    context.Response.StatusCode = 500;
    await context.Response.WriteAsync("Error 500.");
}));

app.MapGet("/allmaps", (IEnumerable<EndpointDataSource> endpointSources) =>
string.Join("\n", endpointSources.SelectMany(source => source.Endpoints)));
app.Run();

public class FormData
{
    public string Value { get; set; }
    public DateTime DateTime { get; set; }
}