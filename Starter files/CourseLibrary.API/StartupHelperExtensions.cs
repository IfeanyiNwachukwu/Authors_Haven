using CourseLibrary.API.DbContexts;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;

namespace CourseLibrary.API;

internal static class StartupHelperExtensions
{
    // Add services to the container
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers(configure =>
        {
            configure.ReturnHttpNotAcceptable = true;

            // configure a cache profile
            configure.CacheProfiles.Add("240SecondsCacheProfile", new() { Duration = 240 });

        })
        .AddNewtonsoftJson(setupAction =>
        {
            setupAction.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        })
        .AddXmlDataContractSerializerFormatters()
        .ConfigureApiBehaviorOptions(setupAction =>
        {
            setupAction.InvalidModelStateResponseFactory = context =>
            {
                // create a validation problem details object
                var problemDetailsFactory = context.HttpContext.RequestServices
                .GetRequiredService<ProblemDetailsFactory>();

                var validationProblemDetails = problemDetailsFactory
                .CreateValidationProblemDetails
                (
                    context.HttpContext,
                    context.ModelState
                );

                //add additional info not added by Default
                validationProblemDetails.Detail = "See the errors field for details";
                validationProblemDetails.Instance = context.HttpContext.Request.Path;

                //report ivalid model state responses as Validation Issues
                validationProblemDetails.Type = "https://courselibrary.com/modelvalidationproblem";
                validationProblemDetails.Status = StatusCodes.Status422UnprocessableEntity;
                validationProblemDetails.Title = "One or more validation errors occured.";

                return new UnprocessableEntityObjectResult(
                    validationProblemDetails
                    )
                {
                    ContentTypes = { "application/problem+json" }
                };


            };
        });

        //configuring custom media tpes

        builder.Services.Configure<MvcOptions>(config =>
        {
            var newtonsoftJsonOutputFormatter = config.OutputFormatters.OfType<NewtonsoftJsonOutputFormatter> ()?.FirstOrDefault();

            if(newtonsoftJsonOutputFormatter != null)
            {
                newtonsoftJsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.ifeanyi.hateoas+json");
            }
        });

        builder.Services.AddTransient<IPropertyMappingService, PropertyMappingService>();

        builder.Services.AddTransient<IPropertyCheckerService, PropertyCheckerService>();

        builder.Services.AddScoped<ICourseLibraryRepository,
            CourseLibraryRepository>();



        builder.Services.AddDbContext<CourseLibraryContext>(options =>
        {
            //options.UseSqlite(@"Data Source=library.db");
            options.UseSqlServer(
                   builder.Configuration.GetConnectionString("DefaultConnection"));
        });

        builder.Services.AddAutoMapper(
            AppDomain.CurrentDomain.GetAssemblies());

        builder.Services.AddResponseCaching();
        // Global Configuration
        builder.Services.AddHttpCacheHeaders(
            (expiratioModelOptions) =>
            {
                expiratioModelOptions.MaxAge = 60;
                expiratioModelOptions.CacheLocation = Marvin.Cache.Headers.CacheLocation.Public;
            },
            (validationModelOptions) => 
            {
                validationModelOptions.MustRevalidate = true;
            
            }
       );
        return builder.Build();
    }




    // Configure the request/response pipeline
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            // code is executed when there is an unhandled exception.
            app.UseExceptionHandler(appBuilder =>
            {
                appBuilder.Run(async context =>
                {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("An Unexpected fault happened. Try again later.");
                });
            });
        }
        app.UseResponseCaching();

        app.UseHttpCacheHeaders();

        app.UseAuthorization();

        app.MapControllers();

        return app;
    }

    public static async Task ResetDatabaseAsync(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            try
            {
                var context = scope.ServiceProvider.GetService<CourseLibraryContext>();
                if (context != null)
                {
                    await context.Database.EnsureDeletedAsync();
                    await context.Database.MigrateAsync();
                }
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger>();
                logger.LogError(ex, "An error occurred while migrating the database.");
            }
        }
    }
}