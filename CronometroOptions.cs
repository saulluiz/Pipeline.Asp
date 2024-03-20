using System;

public class CronometroOptions
{
    public int qtdeUnidades = Enum.GetValues(typeof(UnidadesTempo)).Length;
    public UnidadesTempo UnidadeMedida{
        get;set;
    }
}