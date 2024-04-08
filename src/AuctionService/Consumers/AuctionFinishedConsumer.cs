using AuctionService.Data;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
{
    private readonly AuctionDbContext _db;

    public AuctionFinishedConsumer(AuctionDbContext db)
    {
        _db = db;
    }
    public async Task Consume(ConsumeContext<AuctionFinished> context)
    {
        Console.WriteLine("--> Consuming auction finished.");
        var auction = await _db.Auctions.FindAsync(context.Message.AuctionId);

        if (context.Message.ItemSold)
        {
            auction.Winner = context.Message.Winner;
            auction.SoldAmount = context.Message.Amount;
        }
        auction.Status = auction.SoldAmount > auction.ReservePrice ? Entites.Status.Finished : Entites.Status.ReserveNotMet;

        await _db.SaveChangesAsync();
    }
}
