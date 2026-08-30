using System.Collections.Concurrent;

namespace Application;

// Torrents started through the indexer Import flow carry an intended Category (from
// the site route the listing was browsed under, or one the caller chose explicitly).
// TorrentManager completion only gives us the torrent's name back, so this bridges
// "name at import time" -> "category to file the resulting movie under" for
// API.Common's completion handler to consume.
public static class PendingCategoryAssignments
{
    private static readonly ConcurrentDictionary<string, int> CategoriesByTorrentName = new();

    public static void Set(string torrentName, int categoryId)
    {
        CategoriesByTorrentName[torrentName] = categoryId;
    }

    public static int TakeOrDefault(string torrentName, int defaultCategoryId = 1)
    {
        return CategoriesByTorrentName.TryRemove(torrentName, out var categoryId) ? categoryId : defaultCategoryId;
    }
}
