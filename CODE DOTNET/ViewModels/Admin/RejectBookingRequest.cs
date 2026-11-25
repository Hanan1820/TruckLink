
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TruckLink.Support.Rules;
using TruckLink.Support.Services;

namespace TruckLink.ViewModels.Admin;


public class RejectBookingRequest : Request
{
    [FromForm(Name = "rejection_reason"), Display(Name = "Reason for rejection")]
    [Required]
    public string RejectionReason { get; set; } = "";   

    [FromForm(Name = "customer_message"), Display(Name = "Message to the customer")]
    [Required, StringLength(BookingRejection.MESSAGE_MAX, MinimumLength = 20)]
    public string CustomerMessage { get; set; } = "";

    [FromForm(Name = "offer_dates")]
    public bool? OfferDates { get; set; }                

    public override IEnumerable<ValidationResult> Validate(ValidationContext ctx)
    {
        if (!BookingRejection.REASONS.Contains(RejectionReason))
            yield return Fail("rejection_reason", "The selected Reason for rejection is invalid.");
    }
}
