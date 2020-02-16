using System;

namespace Model
{
    public interface ITradeResult
    {
        Guid Id { get; set; }
        string Symbol { get; set; }
        double VolumeWeightedAveragePrice { get; set; } // Sai Notes: In general this being a monetary value, would recommend using a decimal instead for handling higher precision.
        uint VolumeRequested { get; set; }
        uint VolumeExecuted { get; set; }
    }
}
