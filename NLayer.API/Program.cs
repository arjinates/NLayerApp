using Autofac;
using Autofac.Extensions.DependencyInjection;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLayer.API.Filters;
using NLayer.API.Middleware;
using NLayer.API.Modules;
using NLayer.Core.Repositories;
using NLayer.Core.Services;
using NLayer.Core.UnitOfWorks;
using NLayer.Repository;
using NLayer.Repository.Repositories;
using NLayer.Repository.UnitOfWork;
using NLayer.Service.Mapping;
using NLayer.Service.Services;
using NLayer.Service.Validation;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
// Configuration
// Add services to the container.

builder.Services.AddControllers(options => options.Filters.Add(new ValidateFilterAttribute()));
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true; 
});                                                

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();

builder.Services.AddScoped(typeof(NotFoundFilter<>));

builder.Services.AddAutoMapper(typeof(MapProfile));

builder.Services.AddFluentValidationAutoValidation().AddValidatorsFromAssemblyContaining<ProductDtoValidator>();

builder.Services.AddDbContext<AppDbContext>(x =>
{

    x.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection"), option =>
    {

        option.MigrationsAssembly(Assembly.GetAssembly(typeof(AppDbContext)).GetName().Name);
        //GeAssembly("NLayerApp.Repository") seklinde de yazabilirdik ama bu statik olurdu
    }); //dinamik typeOf AppDbContext name yaparak dinamiklestirdik
});

builder.Host.UseServiceProviderFactory
    (new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
    containerBuilder.RegisterModule(new RepoServiceModule()));



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCustomException();

app.UseAuthorization();

app.MapControllers();

app.Run();
