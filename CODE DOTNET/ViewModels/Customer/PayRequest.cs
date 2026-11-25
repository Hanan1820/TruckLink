
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TruckLink.Models;
using TruckLink.Support.Helpers;
using TruckLink.Support.Rules;

namespace TruckLink.ViewModels.Customer;


public class PayRequest : Request
{
    
    public override string ErrorBag => "payment";

    
    [BindNever] public Models.Customer? User { get; set; }

    [FromForm(Name = "lines"), Display(Name = "Services")]
    [Required(ErrorMessage = "Choose at least one service to pay for."), MinLength(1)]
    public List<int> Lines { get; set; } = new();                 

    [FromForm(Name = "payment_method"), Display(Name = "Payment method")]
    [Required, In("Mada", "Credit Card", "Apple Pay", "STC Pay")] 
    public string PaymentMethod { get; set; } = "";

    [FromForm(Name = "card_source")]
    [In("saved", "new")]
    public string? CardSource { get; set; }

    [FromForm(Name = "save_card")]
    public bool? SaveCard { get; set; }

    
    [FromForm(Name = "card_holder_name"), Display(Name = "Name on card")] public string? CardHolderName { get; set; }
    [FromForm(Name = "card_number"), Display(Name = "Card number")] public string? CardNumber { get; set; }
    [FromForm(Name = "expiry_date"), Display(Name = "Expiry date")] public string? ExpiryDate { get; set; }
    [FromForm(Name = "cvv"), Display(Name = "CVV")] public string? Cvv { get; set; }

    public bool UsesNewCard() =>
        (PaymentMethod == "Mada" || PaymentMethod == "Credit Card")
        && (CardSource != "saved" || User?.HasSavedCard() != true);

    public override IEnumerable<ValidationResult> Validate(ValidationContext ctx)
    {
        
        if (!UsesNewCard()) yield break;
        foreach (var (field, error) in TruckLink.Support.Card.Validate("required", CardHolderName, CardNumber, ExpiryDate, Cvv))
            yield return Fail(field, error);
    }
}
