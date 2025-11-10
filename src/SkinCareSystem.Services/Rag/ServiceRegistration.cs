using System;
using Microsoft.Extensions.DependencyInjection;

namespace SkinCareSystem.Services.Rag;

public static class ServiceRegistration
{
    public static IServiceCollection AddRagServices(this IServiceCollection services)
    {
        services.AddHttpClient("openai", client =>
        {
            client.BaseAddress ??= new Uri("https://api.openai.com/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddScoped<IEmbeddingClient, OpenAIEmbeddingClient>();
        services.AddScoped<ILlmClient, OpenAILlmClient>();
        services.AddScoped<IRagSearchService, RagSearchService>();
        services.AddScoped<IConsultationService, ConsultationService>();

        return services;
    }
}
