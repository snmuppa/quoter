using System;

namespace Model
{
    /// <summary>
    /// Concrete implementation of <see cref="ITradeResult"/>
    /// </summary>
    public class TradeResult : ITradeResult
    {
        public Guid Id { get; set; }
        public string Symbol { get; set; }
        public double VolumeWeightedAveragePrice { get; set; } 
        public uint VolumeRequested { get; set; }
        public uint VolumeExecuted { get; set; }

        /// <summary>
        /// Overriden ToString method
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Id: " + Id + ", " + "Symbol: " + Symbol + ", " + "Price: " + VolumeWeightedAveragePrice + ", " + "VolumeRequested: " + VolumeRequested + ", " + " VolumeExecuted: " + VolumeExecuted;
        }
    }
}
