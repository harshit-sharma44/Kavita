using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs.Filtering.v2;
using API.Entities;
using API.Extensions.QueryExtensions.Filtering;
using API.Helpers.Builders;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace API.Tests.Extensions;

public class SeriesFilterTests : AbstractDbTest
{

    protected override Task ResetDb()
    {
        return Task.CompletedTask;
    }

    #region HasLanguage

    [Fact]
    public async Task HasLanguage_Works()
    {
        var foundSeries = await _context.Series.HasLanguage(true, FilterComparison.Contains, new List<string>() { }).ToListAsync();

    }

    #endregion

    #region HasGenre / HasTags IsEmpty

    /// <summary>
    /// Alpha has a genre and a tag, Beta has a genre, Gamma has neither
    /// </summary>
    private async Task SetupGenreAndTagData()
    {
        var library = await _context.Library.FirstAsync();
        var genre = new GenreBuilder("Action").Build();
        var tag = new TagBuilder("Magic").Build();

        var alphaMetadata = new SeriesMetadataBuilder().Build();
        alphaMetadata.Genres.Add(genre);
        alphaMetadata.Tags.Add(tag);
        var betaMetadata = new SeriesMetadataBuilder().Build();
        betaMetadata.Genres.Add(genre);

        library.Series = new List<Series>()
        {
            new SeriesBuilder("Alpha").WithMetadata(alphaMetadata).Build(),
            new SeriesBuilder("Beta").WithMetadata(betaMetadata).Build(),
            new SeriesBuilder("Gamma").WithMetadata(new SeriesMetadataBuilder().Build()).Build(),
        };
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task HasGenre_IsEmpty_ReturnsOnlySeriesWithoutGenres()
    {
        await SetupGenreAndTagData();

        var names = await _context.Series
            .HasGenre(true, FilterComparison.IsEmpty, new List<int>())
            .Select(s => s.Name)
            .ToListAsync();

        Assert.Equal(new[] {"Gamma"}, names);
    }

    [Fact]
    public async Task HasTags_IsEmpty_ReturnsOnlySeriesWithoutTags()
    {
        await SetupGenreAndTagData();

        var names = await _context.Series
            .HasTags(true, FilterComparison.IsEmpty, new List<int>())
            .Select(s => s.Name)
            .OrderBy(n => n)
            .ToListAsync();

        Assert.Equal(new[] {"Beta", "Gamma"}, names);
    }

    [Fact]
    public async Task HasGenre_Contains_EmptyList_ReturnsEverything()
    {
        await SetupGenreAndTagData();

        var count = await _context.Series
            .HasGenre(true, FilterComparison.Contains, new List<int>())
            .CountAsync();

        Assert.Equal(3, count);
    }

    #endregion
}
