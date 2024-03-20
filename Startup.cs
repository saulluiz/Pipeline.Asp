using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Pipeline.Asp
{
    public class Startup
    {
        private IConfiguration configuration;
        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }



        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ContadorOptions>(options =>
            {
                options.Quantidade = 5;
            });
            services.Configure<CronometroOptions>(
                configuration.GetSection(nameof(CronometroOptions)));
            
        }

    public async void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<ContadorOptions> options)
    {
        app.UseTempoExecucao();
        app.MapWhen(
context => context.Request.Query.ContainsKey("caminhoC"),
appC =>
{
    appC.Use(async (context, next) =>
    {
        await context.Response.WriteAsync("\nProcessado pela ramificação C");
        await next();
    });
});

        app.Map("/caminhoB", appB =>
        {
            appB.Run(async context =>
            {
                await context.Response.WriteAsync("\n Processado pela Ramificação B");
            });
        });

        app.Use(async (context, next) =>
        {
            //codigos acrescentados apos o next representam a volta do pipeline
            await context.Response.WriteAsync("==");
            await next();//nessa linha, o codigo ja passa para proxima funcao,ate que chegue na funcao terminal ("App.run")
            await context.Response.WriteAsync("==");
        });

        app.Use(async (context, next) =>
        {
            var contadorOptions = options.Value;
            await context.Response.WriteAsync(new String('>', contadorOptions.Quantidade));
            await next();

            await context.Response.WriteAsync(new String('<', contadorOptions.Quantidade));
        });

        app.Use(async (context, next) =>
        {
            await context.Response.WriteAsync("[[");
            await next();
            await context.Response.WriteAsync("]]");
        });


        app.Run(async context =>
        {

            await context.Response.WriteAsync("Middleware Terminal");//Quando executa o run, ele volta executando os membros anteriores ao next, 
                                                                     //no sentido oposto ao que escreveu
                                                                     // ================> Indo
                                                                     //<================= Voltando
        });
    }
}
}
