var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.Configure<StyleApp.Services.PhotoProviderOptions>(builder.Configuration.GetSection("PhotoApi"));
builder.Services.AddSingleton<StyleApp.Services.IStyleStateStore, StyleApp.Services.JsonStyleStateStore>();
builder.Services.AddTransient<StyleApp.Services.IStylePhotoProvider, StyleApp.Services.PinterestPhotoProvider>();
builder.Services.AddTransient<StyleApp.Services.IStylePhotoService, StyleApp.Services.ConfiguredStylePhotoService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
