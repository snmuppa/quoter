using System;

namespace Model
{
    public class Quote : IQuote, IEquatable<Quote>
    {
        public Guid? Id { get; set; }
        public string Symbol { get; set; }
        public double Price { get; set; } // Sai Notes: In general this being a monetary value, would recommend using a decimal instead for handling higher precision.
        public uint AvailableVolume { get; set; }
        public DateTime ExpirationDate { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Quote);
        }

        public bool Equals(Quote other)
        {
            return other != null &&
                   Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return 2108858624 + Id.GetHashCode();
        }
    }
}
