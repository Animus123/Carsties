using System;
using System.Net.Http.Json;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.IntegrationTests.Fixtures;
using AuctionService.IntegrationTests.Util;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace AuctionService.IntegrationTests;

[Collection("SharedCollection")]
public class AuctionControllerTests : IAsyncLifetime
{
    private readonly CustomWebAppFactory _factory;
    private readonly HttpClient _httpClient;

    private const string GT_ID = "afbee524-5972-4075-8800-7d1f9d7b0a0c";

    public AuctionControllerTests(CustomWebAppFactory factory)
    {
        this._factory = factory;
        this._httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task GetAuctions_ShouldReturn3Auctions()
    {
        // Arrange

        // Act
        var response = await _httpClient.GetFromJsonAsync<List<AuctionDto>>("/api/auctions");

        // Assert
        Assert.Equal(3, response.Count);
    }

    [Fact]
    public async Task GetAuctionById_WithValidId_ShouldReturnAuction()
    {
        // Arrange

        // Act
        var response = await _httpClient.GetFromJsonAsync<AuctionDto>($"/api/auctions/{GT_ID}");

        // Assert
        Assert.Equal("GT", response.Model);
    }

    [Fact]
    public async Task GetAuctionById_WithInvalidId_ShouldReturn404()
    {
        // Arrange

        // Act
        var response = await _httpClient.GetAsync($"/api/auctions/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAuctionById_WithInvalidGuid_ShouldReturn400()
    {
        // Arrange

        // Act
        var response = await _httpClient.GetAsync($"/api/auctions/notaguid");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateAuction_WithNoAuth_ShouldReturn401()
    {
        // Arrange
        var auction = new CreateAuctionDto { Make = "test" };

        // Act
        var response = await _httpClient.PostAsJsonAsync($"/api/auctions", auction);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateAuction_WithAuth_ShouldReturn201()
    {
        // Arrange
        var auction = GetAuctionForCreate();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

        // Act
        var response = await _httpClient.PostAsJsonAsync($"/api/auctions", auction);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdAuction = await response.Content.ReadFromJsonAsync<AuctionDto>();
        Assert.Equal("bob", createdAuction.Seller);
    }

    [Fact]
    public async Task CreateAuction_WithInvalidCreateAuctionDto_ShouldReturn400()
    {
        // arrange
        var auction = GetAuctionForCreate();
        auction.Make = null;
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

        // Act
        var response = await _httpClient.PostAsJsonAsync($"/api/auctions", auction);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAuction_WithValidUpdateDtoAndUser_ShouldReturn200()
    {
        // arrange
        UpdateAuctionDto updateAuctionDto = new UpdateAuctionDto
        {
            Make = "testModified",
            Model = "testModelModified",
            Year = 2000,
            Color = "test",
            Mileage = 10,
        };
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

        // Act
        var response = await _httpClient.PutAsJsonAsync($"/api/auctions/{GT_ID}", updateAuctionDto);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAuction_WithValidUpdateDtoAndInvalidUser_ShouldReturn403()
    {
        // arrange
        UpdateAuctionDto updateAuctionDto = new UpdateAuctionDto
        {
            Make = "testModified",
            Model = "testModelModified",
            Year = 2000,
            Color = "test",
            Mileage = 10,
        };
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("alice"));

        // Act
        var response = await _httpClient.PutAsJsonAsync($"/api/auctions/{GT_ID}", updateAuctionDto);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }


    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
        DbHelper.ReinitDbForTests(db);
        return Task.CompletedTask;
    }

    private static CreateAuctionDto GetAuctionForCreate()
    {
        return new CreateAuctionDto
        {
            Make = "test",
            Model = "testModel",
            Year = 2000,
            Color = "test",
            Mileage = 10,
            ImageUrl = "test",
            ReservePrice = 10,
            AuctionEnd = DateTime.UtcNow.AddDays(1)
        };
    }
}
