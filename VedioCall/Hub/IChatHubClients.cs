namespace VedioCall;

public interface IChatHubClients
{
    Task ReceiveCall(Guid callerId);
    Task CallAccepted();
    Task CallRejected();
    Task Busy();
    Task CallEnded();
    Task ReceiveIceCandidate(Guid callerId, string candidate);
    Task ReceiveSDP(Guid callerId, string candidate);
    Task UserConnect(List<HubConnectedUser> user);

}
