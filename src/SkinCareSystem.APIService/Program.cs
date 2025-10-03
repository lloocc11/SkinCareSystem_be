
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using SkinCareSystem.Repositories.DBContext;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.InternalServices.IServices;
using SkinCareSystem.Services.InternalServices.Services;


var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<SkinCareSystemDbContext>(options =>
{
    var connectionString = SkinCareSystemDbContext.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString);
});

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserService, UserService>();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c=>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "SkinCareSystem.APIService",
        Version = "v1",
        Description = "Skin Care System API Service"
    });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
    policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SkinCareSystem.APIService v1");
    c.RoutePrefix = string.Empty; // Make Swagger UI available at root
});
app.UseDeveloperExceptionPage();


app.UseHttpsRedirection();

app.UseCors("AllowAllOrigins");

app.UseAuthorization();


app.MapControllers();

await app.RunAsync();
        
    