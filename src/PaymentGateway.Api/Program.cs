using PaymentGateway.Api.Api;
using PaymentGateway.Api.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IPaymentsRepository, PaymentsRepository>();
builder.Services.AddHttpClient<BankService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:8080/"); //TODO we should be using HTTPS
});

builder.Services.AddTransient<IBankService>(sp =>
    sp.GetRequiredService<BankService>());
builder.Services.AddTransient<IPaymentsApi, PaymentsApi>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

