
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TruckLink.Support.Rules;

namespace TruckLink.ViewModels.Customer;


public class AddItemRequest : Request
{
    [FromForm(Name = "item_name"), Display(Name = "Item name")]
    [Required, StringLength(80)]
    public string ItemName { get; set; } = "";

    [FromForm(Name = "category_id"), Display(Name = "Category")]
    [Required, Exists("item_categories", "category_id")]
    public int? CategoryId { get; set; }

    [FromForm(Name = "quantity"), Display(Name = "Quantity")]
    [Required, Range(1, 99)]                                  
    public int? Quantity { get; set; }

    
    [FromForm(Name = "photos"), Display(Name = "Item photos")]
    [Required, MinLength(1), MaxLength(8, ErrorMessage = "Add up to 8 photos per item.")]
    [ImageFile(MaxKb = 10240, MimesMessage = "Photos must be JPG or PNG.", MaxMessage = "Each photo can be up to 10 MB.")]
    public List<IFormFile> Photos { get; set; } = new();
}
