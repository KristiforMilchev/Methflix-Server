using Application.Repositories;
using Domain.Models;
using Infrastructure.Repositories;
using Moq;
using Npgsql;
using NUnit.Framework;

namespace Tests.Application.Repositories;

public class CategoryRepositoryTests
{
    private Mock<IMovieRepository> _mockMovieRepository;
    private CategoryRepository _categoryRepository;
    private Mock<NpgsqlConnection> _fakeConnection;

    public CategoryRepositoryTests(Mock<IMovieRepository> mockMovieRepository, CategoryRepository categoryRepository, Mock<NpgsqlConnection> fakeConnection)
    {
        _mockMovieRepository = mockMovieRepository;
        _categoryRepository = categoryRepository;
        _fakeConnection = fakeConnection;
    }

    [SetUp]
    public void Setup()
    {
        _mockMovieRepository = new Mock<IMovieRepository>();
        _fakeConnection = new Mock<NpgsqlConnection>();
        _categoryRepository = new CategoryRepository(_fakeConnection.Object, _mockMovieRepository.Object);
    }

    [Test]
    public async Task GetCategory_ValidId_ReturnsCategory()
    {
        var fakeCategory = new Category
        {
            Id = 1,
            Name = "Fake Category",
            CreatedBy = 1,
            CreatedAt = "2023-01-01",
            UpdatedBy = null,
            UpdatedAt = null,
            IsDeleted = null
        };

        var fakeDataReader = new Mock<NpgsqlDataReader>();
        fakeDataReader.Setup(r => r.ReadAsync()).ReturnsAsync(true);
        fakeDataReader.Setup(r => r.GetInt32(It.IsAny<int>())).Returns(fakeCategory.Id);
        fakeDataReader.Setup(r => r.GetString(It.IsAny<int>())).Returns(fakeCategory.Name);
        fakeDataReader.Setup(r => r.IsDBNull(It.IsAny<int>())).Returns(false);

        var fakeCommand = new Mock<NpgsqlCommand>();
        fakeCommand.Setup(c => c.ExecuteReaderAsync(new CancellationToken())).ReturnsAsync(fakeDataReader.Object);

        _fakeConnection.Setup(c => c.CreateCommand()).Returns(fakeCommand.Object);

        var result = await _categoryRepository.GetCategory(fakeCategory.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result?.Id, Is.EqualTo(fakeCategory.Id));
        Assert.That(result?.Name, Is.EqualTo(fakeCategory.Name));
    }

    [Test]
    public async Task GetCategory_InvalidId_ReturnsNull()
    {
        // Arrange
        var fakeDataReader = new Mock<NpgsqlDataReader>();
        fakeDataReader.Setup(r => r.ReadAsync()).ReturnsAsync(false);

        var fakeCommand = new Mock<NpgsqlCommand>();
        fakeCommand.Setup(c => c.ExecuteReaderAsync(new CancellationToken())).ReturnsAsync(fakeDataReader.Object);

        _fakeConnection.Setup(c => c.CreateCommand()).Returns(fakeCommand.Object);

        var result = await _categoryRepository.GetCategory(999);

        Assert.IsNull(result);
    }
}