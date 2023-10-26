using System.Text;
using Domain.Models;
using FFmpeg.Net.Enums;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
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
            if (e.NewState == TorrentState.Seeding)
            {
            }
            Listener.WriteLine($"OldState: {e.OldState} NewState: {e.NewState}");
        }
    }

    public async void OnTrackerManagerOnAnnounceComplete(object? sender, AnnounceResponseEventArgs e, TorrentManager 
        manager)
    {
        if (manager.Complete)
        {
            Console.WriteLine(manager.Name);
            Notifier.Call(manager.Name, manager);
            Notifier.Call("on_torrent_downloaded", manager);
        }
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
