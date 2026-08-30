using Application;
using NUnit.Framework;

namespace Tests.Application;

public class PendingCategoryAssignmentsTests
{
    [Test]
    public void TakeOrDefault_NoAssignmentSet_ReturnsDefault()
    {
        var result = PendingCategoryAssignments.TakeOrDefault("unset-torrent-name");

        Assert.That(result, Is.EqualTo(1));
    }

    [Test]
    public void TakeOrDefault_AssignmentSet_ReturnsAssignedCategory()
    {
        PendingCategoryAssignments.Set("some-torrent", 7);

        var result = PendingCategoryAssignments.TakeOrDefault("some-torrent");

        Assert.That(result, Is.EqualTo(7));
    }

    [Test]
    public void TakeOrDefault_ConsumesAssignment_SecondCallReturnsDefault()
    {
        PendingCategoryAssignments.Set("one-shot-torrent", 3);

        PendingCategoryAssignments.TakeOrDefault("one-shot-torrent");
        var secondResult = PendingCategoryAssignments.TakeOrDefault("one-shot-torrent");

        Assert.That(secondResult, Is.EqualTo(1));
    }
}
