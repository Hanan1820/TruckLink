
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TruckLink.Models;

[Table("item_photos")]
public class ItemPhoto
{
    [Key, Column("item_photo_id")] public int ItemPhotoId { get; set; }
    [Column("item_id")] public int ItemId { get; set; }
    [Column("photo_url")] public string PhotoUrl { get; set; } = "";
    [Column("uploaded_at")] public DateTime UploadedAt { get; set; }

    public virtual Item Item { get; set; } = null!;   

    
    
    [NotMapped]
    public string Src => PhotoUrl.StartsWith("http", StringComparison.Ordinal) ? PhotoUrl : "~/" + PhotoUrl.TrimStart('/');
}
