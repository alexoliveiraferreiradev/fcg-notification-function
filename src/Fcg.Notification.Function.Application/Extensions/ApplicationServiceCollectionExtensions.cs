using Fcg.Notification.Function.Application.Behaviors;
using Fcg.Notification.Function.Application.UseCase.ApprovedPaymentEmail;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Fcg.Notification.Function.Application.Extensions
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddAplicationServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(typeof(SendPaymentApprovedEmailCommand).Assembly);
            });
           
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(IdempotencyBehavior<,>));
            return services;
        }
    }
}
