using TorneosEsports.Web.Services.Interfaces;

namespace TorneosEsports.Web.Services.Llaves;

/// <summary>
/// Notificador que no hace nada. Se usa hasta que exista la versión con SignalR.
/// </summary>
/// <remarks>
/// Permite que el servicio de llaves funcione y se pruebe sin tiempo
/// real; cuando se registre el notificador con SignalR, este deja de usarse sin cambiar
/// ninguna otra clase.
/// </remarks>
public class NotificadorLlavesNulo : INotificadorLlaves
{
    /// <inheritdoc />
    public Task LlavesActualizadasAsync(int torneoId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
