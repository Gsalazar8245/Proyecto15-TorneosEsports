using TorneosEsports.Web.Services.Interfaces;

namespace TorneosEsports.Web.Hubs;

/// <summary>
/// Implementación de INotificadorLlaves con SignalR: reutiliza el IBracketNotificador
/// para avisar a los navegadores que están viendo el bracket del torneo.
/// </summary>
public class NotificadorLlavesSignalR : INotificadorLlaves
{
    private readonly IBracketNotificador _bracket;

    public NotificadorLlavesSignalR(IBracketNotificador bracket) => _bracket = bracket;

    public Task LlavesActualizadasAsync(int torneoId, CancellationToken cancellationToken = default) =>
        _bracket.NotificarAsync(torneoId);
}
