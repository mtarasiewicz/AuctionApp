using AuctionService.Entites;

namespace AuctionService.UnitTests;

public class AuctionEntityTests
{
    [Fact]
    public void HasReservePrice_ReservePriceGreaterThanZero_ShouldBeTrue()
    {
        var auction = new Auction
        {
            Id = Guid.NewGuid(),
            ReservePrice = 10
        };
        var result = auction.HasReservePrice();
        Assert.True(result);
    }
    [Fact]
    public void HasReservePrice_ReservePriceIsZero_ShouldBeFalse()
    {
        var auction = new Auction
        {
            Id = Guid.NewGuid(),
        };
        var result = auction.HasReservePrice();
        Assert.False(result);
    }


}
