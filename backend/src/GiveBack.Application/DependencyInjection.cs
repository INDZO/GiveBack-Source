using FluentValidation;
using GiveBack.Application.Common.Behaviors;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Common.Policies;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GiveBack.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;
        services.AddAutoMapper(_ => { }, assembly);
        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped<IOpportunityAccessPolicy, OpportunityAccessPolicy>();
        return services;
    }
}
