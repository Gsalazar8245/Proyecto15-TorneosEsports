using Microsoft.AspNetCore.SignalR;

namespace TorneosEsports.Web.Hubs;

public class BracketHub : Hub
{
    public static string Grupo(int torneoId) => $"torneo-{torneoId}";

    /// <summary>Cada navegador se une al grupo del torneo que está viendo.</summary>
    public Task UnirseATorneo(int torneoId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, Grupo(torneoId));
}

public interface IBracketNotificador
{
    /// <summary>Avisa a quienes ven este torneo que el bracket cambió.</summary>
    Task NotificarAsync(int torneoId);
}

public class BracketNotificador : IBracketNotificador
{
    private readonly IHubContext<BracketHub> _hub;
    public BracketNotificador(IHubContext<BracketHub> hub) => _hub = hub;

    public Task NotificarAsync(int torneoId) =>
        _hub.Clients.Group(BracketHub.Grupo(torneoId)).SendAsync("BracketActualizado", torneoId);
}