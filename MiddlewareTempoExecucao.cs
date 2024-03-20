using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

public class MiddlewareTempoExecucao
{
    private readonly RequestDelegate _next;

    private readonly CronometroOptions _options;
    public MiddlewareTempoExecucao(RequestDelegate next, IOptions<CronometroOptions> options)
    {
        _next = next;
        _options = options.Value;
    }
    public async Task Invoke(HttpContext context)
    {
        context.Response.ContentType = "text/plain; charset=utf-8";
        var sw = Stopwatch.StartNew();//inicia o cronometro
        await _next(context);
        sw.Stop();

        double tempo = 0;
        switch (_options.UnidadeMedida)
        {
            case UnidadesTempo.Nanosegundo:
                double nanosPorTick = (1000.0 * 1000.0 * 1000.0) / Stopwatch.Frequency;
                tempo = sw.ElapsedTicks * nanosPorTick;
                break;
            case UnidadesTempo.Microsegundo:
                double microsPorTick = (1000.0 * 1000.0) / Stopwatch.Frequency;
                tempo = sw.ElapsedTicks * microsPorTick;
                break;
            case UnidadesTempo.Milisegundo:
                double milisPorTick = (1000.0) / Stopwatch.Frequency;
                tempo = sw.ElapsedTicks * milisPorTick;
                break;
        }

        //como o stop do cronometro esta na volta, isso medira o tempo que o pipeline demora para ir e voltar, ou seja, cumprir sua funcao
        await context.Response.WriteAsync($"\n Tempo de Execucao (ms): {sw.ElapsedMilliseconds}");
        await context.Response.WriteAsync($"\n Tempo de Execucao ({_options.UnidadeMedida.ToString().ToLower()}s): {tempo:F2}");
        //O write Async escreve no corpo da response
    }
}