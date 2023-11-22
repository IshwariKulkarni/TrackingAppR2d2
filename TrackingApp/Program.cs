using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.Models.ExternalConnectors;
using TrackingApp.Entities;
using TrackingApp.Interface;
using TrackingApp.Repository;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.




builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddDbContext<TrackingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("connMSSQL")));
builder.Services.AddTransient<TrackingDbContext>();
builder.Services.AddTransient<ITrackApp, TrackApp>();
builder.Services.AddTransient<IEmailServices, EmailServices>();
builder.Services.AddLogging(builder => builder.AddConsole());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
/*builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TrackingApp", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Insert token",
        Name = "Auth",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[]{}
                    }
    });
});*/

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseCors(cors => cors.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());


app.MapControllers();

app.Run();
