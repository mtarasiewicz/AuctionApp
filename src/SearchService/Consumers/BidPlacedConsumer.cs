using System.Data;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        Console.WriteLine("--> Consuming bid placed");
        var auciton = await DB.Find<Item>().OneAsync(context.Message.AuctionId);
        if (context.Message.BidStatus.Contains("Accepted")
            && context.Message.Amount > auciton.CurrentHighBid)
        {
            auciton.CurrentHighBid = context.Message.Amount;
            await auciton.SaveAsync();
        }
    }
}
