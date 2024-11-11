using GHCW_FE.Services;
using System.Transactions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<TicketService>();
builder.Services.AddScoped<TicketDetailService>();
builder.Services.AddScoped<ScheduleService>();
builder.Services.AddScoped<DiscountService>();
builder.Services.AddScoped<ServicesService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<NewsService>();
builder.Services.AddScoped<CategoryService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(1440);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
} 
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
