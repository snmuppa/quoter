using System;

namespace Model
{
    public interface IQuote
    {
        Guid? Id { get; set; }
        string Symbol { get; set; }
        double Price { get; set; } // Sai Notes: In general this being a monetary value, would recommend using a decimal instead for handling higher precision.
        uint AvailableVolume { get; set; }
        DateTime ExpirationDate { get; set; }
    }
}
