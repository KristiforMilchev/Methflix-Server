using System.Text;
using MonoTorrent.Client;
using MonoTorrent.Trackers;

namespace Infrastructure.Interfaces;

public interface ITorrentNotifier
{
    public void OnManagerOnPeerConnected(object? o, PeerConnectedEventArgs e);
    public void OnManagerOnPieceHashed(object? o, PieceHashedEventArgs e);
    public void OnManagerOnConnectionAttemptFailed(object? o, ConnectionAttemptFailedEventArgs e);
    public void OnManagerOnTorrentStateChanged(object? o, TorrentStateChangedEventArgs e);
    public void OnTrackerManagerOnAnnounceComplete(object? sender, AnnounceResponseEventArgs e, TorrentManager manager);
    public void AppendSeparator(StringBuilder sb);
    public void AppendFormat(StringBuilder sb, string str, params object[]? formatting);
    public void ExportListener();
}
