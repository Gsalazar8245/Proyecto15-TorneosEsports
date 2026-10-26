using TorneosEsports.Web.Models.Entidades;

namespace TorneosEsports.Web.Services.Llaves;

/// <summary>
/// Generador de llaves de eliminación directa: quien pierde queda fuera.
/// </summary>
/// <remarks>
/// Si la cantidad de equipos no es potencia de 2 (por ejemplo 6),
/// la llave se completa hasta 8 y las mejores semillas pasan la primera ronda sin jugar
/// (bye). Gracias al orden de siembra, nunca quedan dos byes en el mismo partido.
/// </remarks>
public class EliminacionDirecta : GeneradorLlavesBase
{
    /// <inheritdoc />
    public override string Formato => "Eliminación directa";

    /// <inheritdoc />
    protected override IReadOnlyList<Partido> ConstruirLlaves(int torneoId, IReadOnlyList<int> sembrados)
    {
        int tamano = TamanoDeLlave(sembrados.Count);
        int rondas = CantidadDeRondas(tamano);

        // 1. Crear todos los partidos vacíos: la ronda r tiene tamano / 2^r partidos.
        var partidos = new List<Partido>();
        for (int ronda = 1; ronda <= rondas; ronda++)
        {
            int partidosEnRonda = tamano >> ronda;
            for (int posicion = 0; posicion < partidosEnRonda; posicion++)
                partidos.Add(new Partido { TorneoId = torneoId, Ronda = ronda, Posicion = posicion });
        }

        // 2. Llenar la primera ronda según el orden de siembra.
        var orden = OrdenDeSiembra(tamano);
        var primeraRonda = partidos.Where(p => p.Ronda == 1).OrderBy(p => p.Posicion).ToList();
        for (int i = 0; i < primeraRonda.Count; i++)
        {
            var partido = primeraRonda[i];
            partido.EquipoAId = EquipoDeLaSemilla(orden[2 * i], sembrados);
            partido.EquipoBId = EquipoDeLaSemilla(orden[2 * i + 1], sembrados);

            // 3. Bye: si falta un rival, el equipo presente gana sin jugar y avanza.
            if (partido.EquipoAId is null || partido.EquipoBId is null)
            {
                int unico = (partido.EquipoAId ?? partido.EquipoBId)!.Value;
                partido.EsBye = true;
                partido.GanadorId = unico;
                AvanceDeLlaves.AvanzarGanador(partido, unico, partidos);
            }
        }

        return partidos;
    }

    /// <summary>Devuelve el equipo de una semilla, o null si la semilla es un lugar vacío.</summary>
    private static int? EquipoDeLaSemilla(int semilla, IReadOnlyList<int> sembrados) =>
        semilla <= sembrados.Count ? sembrados[semilla - 1] : null;
}
