using System.Text;
using MonoTorrent.Client;
using MonoTorrent.Trackers;

namespace Application.Services;

public class TorrentNotifier : ITorrentNotifier
{
    private Top10Listener Listener { get; } = new (10);

    public  void OnManagerOnPeerConnected(object? o, PeerConnectedEventArgs e)
    {
        lock (Listener)
        {
            Listener.WriteLine($"Connection succeeded: {e.Peer.Uri}");
        }
    }

    public  void OnManagerOnPieceHashed(object? o, PieceHashedEventArgs e)
    {
        lock (Listener)
        {
            Listener.WriteLine($"Piece Hashed: {e.PieceIndex} - {(e.HashPassed ? "Pass" : "Fail")}");
        }
    }

    public  void OnManagerOnConnectionAttemptFailed(object? o, ConnectionAttemptFailedEventArgs e)
    {
        lock (Listener)
        {
            Listener.WriteLine($"Connection failed: {e.Peer.ConnectionUri} - {e.Reason}");
        }
    }

    public  void OnManagerOnTorrentStateChanged(object? o, TorrentStateChangedEventArgs e)
    {
        lock (Listener)
        {
            Listener.WriteLine($"OldState: {e.OldState} NewState: {e.NewState}");
        }
    }

    public void OnTrackerManagerOnAnnounceComplete(object? sender, AnnounceResponseEventArgs e)
    {
        Listener.WriteLine($"{e.Successful}: {e.Tracker}");
    }
    public void AppendSeparator(StringBuilder sb)
    {
        AppendFormat(sb, "");
        AppendFormat(sb, "- - - - - - - - - - - - - - - - - - - - - - - - - - - - - -");
        AppendFormat(sb, "");
    }

    public void AppendFormat(StringBuilder sb, string str, params object[]? formatting)
    {
        if (formatting is { Length: > 0 })
            sb.AppendFormat(str, formatting);
        else
            sb.Append(str);
        sb.AppendLine();
    }

    public void ExportListener() => Listener.ExportTo(Console.Out);
    
}
