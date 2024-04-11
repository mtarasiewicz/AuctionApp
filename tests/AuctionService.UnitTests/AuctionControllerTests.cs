using AuctionService.Controllers;
using AuctionService.DTOs;
using AuctionService.Entites;
using AuctionService.RequestHelpers;
using AutoFixture;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AuctionService.UnitTests;

public class AuctionControllerTests
{
    private readonly Mock<IAuctionRepository> _auctionRepo;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly Fixture _fixture;
    private readonly AuctionsController _auctionsController;
    private readonly IMapper _mapper;
    public AuctionControllerTests()
    {
        _fixture = new Fixture();
        _auctionRepo = new Mock<IAuctionRepository>();
        _publishEndpoint = new Mock<IPublishEndpoint>();

        var mockMapper = new MapperConfiguration(mc =>
        {
            mc.AddMaps(typeof(MappingProfiles).Assembly);

        }).CreateMapper().ConfigurationProvider;
        _mapper = new Mapper(mockMapper);
        _auctionsController = new AuctionsController(_auctionRepo.Object,
                                                     _mapper,
                                                     _publishEndpoint.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = Helpers.GetClaimsPrincipal()
                }
            }
        };
    }
    [Fact]
    public async Task GetAuctions_WithNoParams_ShouldReturns10Auctions()
    {
        var auctions = _fixture.CreateMany<AuctionDto>(10).ToList();
        _auctionRepo.Setup(r => r.GetAuctionsAsync(null)).ReturnsAsync(auctions);
        var result = await _auctionsController.GetAllAuctions(null);
        Assert.Equal(10, result.Value.Count);
        Assert.IsType<ActionResult<List<AuctionDto>>>(result);
    }
    [Fact]
    public async Task GetAuctionsById_WithValidGuid_ShouldReturnsAuction()
    {
        var auction = _fixture.Create<AuctionDto>();
        _auctionRepo.Setup(r => r.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);
        var result = await _auctionsController.GetAuctionById(auction.Id);
        Assert.Equal(auction.Make, result.Value.Make);
        Assert.IsType<ActionResult<AuctionDto>>(result);
    }
    [Fact]
    public async Task GetAuctionsById_WithInValidGuid_ShouldReturnsNotFound()
    {
        _auctionRepo.Setup(r => r.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(value: null);
        var result = await _auctionsController.GetAuctionById(Guid.NewGuid());
        Assert.IsType<NotFoundResult>(result.Result);
    }
    [Fact]
    public async Task CreateAcution_WithValidCreateAuctionDto_ShouldReturnsCreatedAtAction()
    {
        var auction = _fixture.Create<CreateAuctionDto>();
        _auctionRepo.Setup(r => r.AddAuction(It.IsAny<Auction>()));
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        var result = await _auctionsController.CreateAuction(auction);
        var createdResults = result.Result as CreatedAtActionResult;
        Assert.NotNull(createdResults);
        Assert.Equal("GetAuctionById", createdResults.ActionName);
        Assert.IsType<AuctionDto>(createdResults.Value);
    }
    [Fact]
    public async Task CreateAcution_DbCantSaveChanges_ShouldReturnsBadRequest()
    {
        var auction = _fixture.Create<CreateAuctionDto>();
        _auctionRepo.Setup(r => r.AddAuction(It.IsAny<Auction>()));
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(false);

        var result = await _auctionsController.CreateAuction(auction);
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
    [Fact]
    public async Task UpdateAuction_WithUpdateAuctionDto_ShouldReturnsOkResponse()
    {
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();
        auction.Seller = Helpers.Username;
        var updateDto = _fixture.Create<UpdateAuctionDto>();
        _auctionRepo.Setup(r => r.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
        _auctionRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);
        var result = await _auctionsController.UpdateAuction(auction.Id, updateDto);
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_WithInvalidUser_ShouldReturns403Forbid()
    {
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();
        auction.Seller = "unknownUser";
        var updateDto = _fixture.Create<UpdateAuctionDto>();
        _auctionRepo.Setup(r => r.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
        _auctionRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);
        var result = await _auctionsController.UpdateAuction(auction.Id, updateDto);
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_WithInvalidGuid_ShouldReturnsNotFound()
    {
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();
        _auctionRepo.Setup(r => r.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(value: null);
        var updateDto = _fixture.Create<UpdateAuctionDto>();
        var result = await _auctionsController.UpdateAuction(Guid.NewGuid(), updateDto);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithValidUser_ShouldReturnsOkResponse()
    {
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();
        auction.Seller = Helpers.Username;
        _auctionRepo.Setup(r => r.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
        _auctionRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);
        var result = await _auctionsController.DeleteAuction(Guid.NewGuid());
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidGuid_ShouldReturns404Response()
    {
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();
        auction.Seller = Helpers.Username;
        _auctionRepo.Setup(r => r.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(value: null);
        _auctionRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);
        var result = await _auctionsController.DeleteAuction(Guid.NewGuid());
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidUser_ShouldReturns403Response()
    {
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();
        auction.Seller = "unknownUser";
        _auctionRepo.Setup(r => r.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
        _auctionRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);
        var result = await _auctionsController.DeleteAuction(Guid.NewGuid());
        Assert.IsType<ForbidResult>(result);
    }

}