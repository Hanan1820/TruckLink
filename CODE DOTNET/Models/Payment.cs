
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TruckLink.Models;


[Table("payments")]
public class Payment
{
    public static readonly string[] METHODS = { "Mada", "Credit Card", "Apple Pay", "STC Pay" };

    public static readonly string[] STATUSES = { "Paid", "Payment Pending", "Failed", "Refunded" };

    [Key, Column("payment_id")] public int PaymentId { get; set; }
    [Column("booking_id")] public int BookingId { get; set; }
    [Column("invoice_number")] public string InvoiceNumber { get; set; } = "";
    [Column("ref_number")] public string? RefNumber { get; set; }
    [Column("amount")] public decimal Amount { get; set; }
    [Column("payment_method")] public string PaymentMethod { get; set; } = "";
    [Column("payment_status")] public string PaymentStatus { get; set; } = "Payment Pending";
    [Column("transaction_date")] public DateTime TransactionDate { get; set; }

    public virtual Booking Booking { get; set; } = null!;   
}
