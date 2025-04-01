using App.Services.Categories;
using App.Services.ExceptionHandlers;
using App.Services.Products;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using App.Services.Filters;
using App.Services.Queues;
using App.Services.Queues.Publishers;
using App.Services.Reports;

namespace App.Services.Extensions
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Turn off the default ModelState validation filter
            services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped(typeof(NotFoundFilter<,>));

            // async validation için kapatılması gerekir
            services.AddFluentValidationAutoValidation();

            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // for automapper
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // for ExceptionHandler
            services.AddExceptionHandler<CriticalExceptionHandler>();
            services.AddExceptionHandler<GlobalExceptionHandler>();

            // for RabbitMQ
            services.AddSingleton<IRabbitMQPublisher, RabbitMQPublisher>();

            services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMqSettings"));


            // for excell 
            services.AddScoped<ICategoryExcelExporter, CategoryExcelExporter>();


            return services;
        }
    }
}
