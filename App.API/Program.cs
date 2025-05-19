using App.Repositories.Extensions;
using App.Services;
using App.Services.Extensions;
using App.Services.Queues.Consumers;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.Filters.Add<FluentValidationFilter>();
    // Turn off default nullable reference types control
    // options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// For Swagger
builder.Services.AddSwaggerGen();

// CORS AYARLARI EKLENDİ 

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
    {
        builder.WithOrigins("http://localhost:5173") // Frontend'in adresi
            .AllowAnyMethod() // GET, POST, PUT, DELETE her şeye izin ver
            .AllowAnyHeader() // Authorization, Content-Type gibi tüm header'ları kabul et
            .AllowCredentials(); // Eğer JWT veya Cookie tabanlı kimlik doğrulama varsa bunu aç
    });
});


builder.Services
    .AddRepositories(builder.Configuration)
    .AddServices(builder.Configuration);

// for RabbitMQ
builder.Services.AddHostedService<RabbitMqConsumer>();
builder.Services.AddHostedService<ExcelExportConsumer>();

var app = builder.Build();

app.UseExceptionHandler(x => {});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // For Swagger
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles(); // wwwroot için

// 🔥 uploads klasörünü dışarıya aç
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")),
    RequestPath = "/uploads"
});

// CORS DEVREYE ALINDI 
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
