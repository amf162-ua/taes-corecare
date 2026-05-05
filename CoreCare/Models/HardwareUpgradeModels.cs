using System.Collections.Generic;

namespace CoreCare.Models
{
    public enum UpgradeProfile
    {
        General,
        Gaming,
        HeavyWork
    }

    public sealed class ComponentScore
    {
        public string Component { get; set; } = string.Empty;
        public UpgradeProfile Profile { get; set; }
        public double Score { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public sealed class UpgradeRecommendation
    {
        public string Component { get; set; } = string.Empty;
        public string CurrentComponent { get; set; } = string.Empty;
        public UpgradeProfile Profile { get; set; }
        public double CurrentScore { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string SuggestedUpgrade { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string PriceRange { get; set; } = string.Empty;
        public string CompatibilityNote { get; set; } = string.Empty;
        public List<PurchaseLink> PurchaseLinks { get; set; } = new();
    }

    public sealed class PurchaseLink
    {
        public string Store { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
