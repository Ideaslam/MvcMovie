using MvcMovie.Contracts;
using MvcMovie.Data;
using MvcMovie.Entities;
using MvcMovie.Services;

namespace MvcMovie.Configs;

public class DependencyInjection
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>(); 
        services.AddScoped<IPusher<Product>, Pusher<Product>>(); 
        services.AddScoped<ICurrencyService, CurrencyService>(); 
    
    }
    
}
