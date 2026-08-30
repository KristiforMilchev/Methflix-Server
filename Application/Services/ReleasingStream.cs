namespace Application.Services;

// Releases a per-torrent stream lock once the caller disposes the stream, so the
// next request for the same torrent can open a new StreamProvider stream.
internal sealed class ReleasingStream : Stream
{
    private readonly Stream _inner;
    private readonly SemaphoreSlim _lock;
    private bool _released;

    public ReleasingStream(Stream inner, SemaphoreSlim @lock)
    {
        _inner = inner;
        _lock = @lock;
    }

    public override bool CanRead => _inner.CanRead;
    public override bool CanSeek => _inner.CanSeek;
    public override bool CanWrite => false;
    public override long Length => _inner.Length;

    public override long Position
    {
        get => _inner.Position;
        set => _inner.Position = value;
    }

    public override void Flush() => _inner.Flush();
    public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => _inner.ReadAsync(buffer, offset, count, cancellationToken);
    public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _inner.Dispose();
            Release();
        }

        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        await _inner.DisposeAsync();
        Release();
        await base.DisposeAsync();
    }

    private void Release()
    {
        if (_released) return;
        _released = true;
        _lock.Release();
    }
}
