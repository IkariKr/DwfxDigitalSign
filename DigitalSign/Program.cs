using DigitalSign.Api;


namespace DigitalSign;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        
        // 添加 CORS 服务并配置策略
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigins", policy =>
            {
                // 允许所有来源
                policy.AllowAnyOrigin()
                    // 允许所有请求头
                    .AllowAnyHeader()
                    // 允许所有 HTTP 方法
                    .AllowAnyMethod();
            });
        });
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        //app.UseHttpsRedirection();
        
        app = app.Watermark().DigitalSign()
            .Vertifysign().Signinfo();
        
        app.Run(); 
    }
}