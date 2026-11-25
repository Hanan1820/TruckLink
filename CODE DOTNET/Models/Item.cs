
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Text.RegularExpressions;

namespace TruckLink.Models;


[Table("items")]
public class Item
{
    [Key, Column("item_id")] public int ItemId { get; set; }
    [Column("booking_id")] public int BookingId { get; set; }
    [Column("category_id")] public int CategoryId { get; set; }
    [Column("item_name")] public string ItemName { get; set; } = "";
    [Column("quantity")] public int Quantity { get; set; } = 1;
    [Column("estimated_length_cm")] public decimal? EstimatedLengthCm { get; set; }
    [Column("estimated_width_cm")] public decimal? EstimatedWidthCm { get; set; }
    [Column("estimated_height_cm")] public decimal? EstimatedHeightCm { get; set; }
    [Column("estimated_volume_m3")] public decimal? EstimatedVolumeM3 { get; set; }
    [Column("estimated_weight_kg")] public decimal? EstimatedWeightKg { get; set; }
    [Column("ai_confidence_score")] public decimal? AiConfidenceScore { get; set; }
    [Column("measured_at")] public DateTime? MeasuredAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;                              
    public virtual ItemCategory Category { get; set; } = null!;                        
    public virtual ICollection<ItemPhoto> Photos { get; set; } = new List<ItemPhoto>(); 

    
    public bool IsMeasured() => MeasuredAt != null;

    
    
    
    
    public string IllustrationView(bool small = false)
    {
        var name = (ItemName ?? "").ToLowerInvariant();
        string drawing;
        if (Regex.IsMatch(name, "sofa|couch|majlis")) drawing = "sofa";
        else if (Regex.IsMatch(name, "fridge|refrigerator|freezer")) drawing = "refrigerator";
        else if (Regex.IsMatch(name, "table|dining|chair")) drawing = "dining-set";
        else if (Regex.IsMatch(name, "box|carton|bag")) drawing = "boxes";
        else drawing = new Dictionary<int, string> { [1] = "sofa", [2] = "refrigerator" }.GetValueOrDefault(CategoryId, "boxes");

        
        var pascal = string.Concat(drawing.Split('-').Select(p => char.ToUpperInvariant(p[0]) + p[1..]));
        return "Svg/Item" + pascal + (small ? "Sm" : "");
    }

    
    [NotMapped]
    public string? DimensionsLabel
    {
        get
        {
            if (!IsMeasured()) return null;
            static string F(decimal? v) => ((decimal)(v ?? 0)).ToString("0.##", CultureInfo.InvariantCulture);
            return $"{F(EstimatedLengthCm)} × {F(EstimatedWidthCm)} × {F(EstimatedHeightCm)} cm";
        }
    }
}
