using System.Numerics;
using TorneosEsports.Web.Models.Entidades;
using TorneosEsports.Web.Services.Interfaces;

namespace TorneosEsports.Web.Services.Llaves;

/// <summary>
/// Base abstracta de los generadores de llaves (patrón método plantilla).
/// </summary>
/// <remarks>
/// <see cref="Generar"/> fija los pasos que todo formato comparte:
/// validar los equipos, ordenar la siembra y construir las llaves. Cada formato concreto
/// solo implementa <see cref="ConstruirLlaves"/>. Para agregar «doble eliminación» se crea
/// otra clase derivada sin modificar esta (principio abierto/cerrado).
/// </remarks>
public abstract class GeneradorLlavesBase : IGeneradorLlaves
{
    /// <inheritdoc />
    public abstract string Formato { get; }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Si hay menos de dos equipos.</exception>
    /// <exception cref="ArgumentException">Si un equipo aparece repetido.</exception>
    public IReadOnlyList<Partido> Generar(int torneoId, IReadOnlyList<int> equiposEnOrden)
    {
        ArgumentNullException.ThrowIfNull(equiposEnOrden);

        if (equiposEnOrden.Count < Torneo.MinimoEquiposParaLlaves)
            throw new InvalidOperationException(
                $"Se necesitan al menos {Torneo.MinimoEquiposParaLlaves} equipos para generar las llaves.");

        if (equiposEnOrden.Distinct().Count() != equiposEnOrden.Count)
            throw new ArgumentException("Hay equipos repetidos en la lista.", nameof(equiposEnOrden));

        var sembrados = OrdenarSiembra(equiposEnOrden);
        return ConstruirLlaves(torneoId, sembrados);
    }

    /// <summary>
    /// Decide el orden de siembra. Por defecto respeta el orden recibido
    /// (el primer equipo inscrito y pagado es la semilla 1).
    /// </summary>
    /// <param name="equipos">Equipos en el orden recibido.</param>
    /// <returns>Equipos en orden de siembra.</returns>
    protected virtual IReadOnlyList<int> OrdenarSiembra(IReadOnlyList<int> equipos) => equipos;

    /// <summary>Construye los partidos del formato concreto.</summary>
    /// <param name="torneoId">Torneo al que pertenecen.</param>
    /// <param name="sembrados">Equipos en orden de siembra.</param>
    /// <returns>Todos los partidos de todas las rondas.</returns>
    protected abstract IReadOnlyList<Partido> ConstruirLlaves(int torneoId, IReadOnlyList<int> sembrados);

    /// <summary>Tamaño de la llave: la menor potencia de 2 que alcanza para todos los equipos.</summary>
    /// <param name="cantidadEquipos">Equipos que juegan.</param>
    /// <returns>Por ejemplo, 6 equipos → 8.</returns>
    public static int TamanoDeLlave(int cantidadEquipos) =>
        (int)BitOperations.RoundUpToPowerOf2((uint)Math.Max(2, cantidadEquipos));

    /// <summary>Cantidad de rondas de una llave.</summary>
    /// <param name="tamano">Tamaño de la llave (potencia de 2).</param>
    /// <returns>Por ejemplo, 8 → 3 rondas (cuartos, semifinal, final).</returns>
    public static int CantidadDeRondas(int tamano) => BitOperations.Log2((uint)tamano);

    /// <summary>
    /// Orden en que se ubican las semillas en la primera ronda, para que las mejores
    /// semillas solo se crucen al final: 4 → [1, 4, 2, 3]; 8 → [1, 8, 4, 5, 2, 7, 3, 6].
    /// </summary>
    /// <param name="tamano">Tamaño de la llave (potencia de 2).</param>
    /// <returns>Lista de semillas; cada par consecutivo es un partido.</returns>
    public static IReadOnlyList<int> OrdenDeSiembra(int tamano)
    {
        var orden = new List<int> { 1 };
        while (orden.Count < tamano)
        {
            int n = orden.Count * 2;
            var siguiente = new List<int>(n);
            foreach (int semilla in orden)
            {
                siguiente.Add(semilla);
                siguiente.Add(n + 1 - semilla); // su rival: la semilla «espejo»
            }
            orden = siguiente;
        }
        return orden;
    }
}
