using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Sample.PdfDataExtraction;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components.Components.Tooltip;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Add local storage support
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<TemplateRepository>();

// Register Fluent UI Blazor components
builder.Services.AddFluentUIComponents();
builder.Services.AddScoped<ITooltipService, TooltipService>();

await builder.Build().RunAsync();