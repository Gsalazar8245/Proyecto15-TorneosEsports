namespace TorneosEsports.Web.Services.Interfaces;

/// <summary>
/// Avisa que el bracket de un torneo cambió (se generaron llaves o hay un resultado nuevo).
/// </summary>
/// <remarks>
/// El servicio de llaves solo conoce esta interfaz. La implementación con SignalR (bracket
/// en vivo) va en el módulo de tiempo real. Mientras tanto se usa <see cref="Llaves.NotificadorLlavesNulo"/>, que no hace
/// nada. Es un ejemplo de abierto/cerrado: se agrega tiempo real sin modificar el servicio de llaves.
/// </remarks>
public interface INotificadorLlaves
{
    /// <summary>Notifica a quienes están viendo el bracket del torneo.</summary>
    /// <param name="torneoId">Id del torneo que cambió.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tarea asíncrona.</returns>
    Task LlavesActualizadasAsync(int torneoId, CancellationToken cancellationToken = default);
}
