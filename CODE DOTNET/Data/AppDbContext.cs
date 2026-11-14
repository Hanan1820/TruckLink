
using Microsoft.EntityFrameworkCore;
using TruckLink.Models;

namespace TruckLink.Data;







public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<ItemCategory> ItemCategories => Set<ItemCategory>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingService> BookingServices => Set<BookingService>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<ItemPhoto> ItemPhotos => Set<ItemPhoto>();
    public DbSet<StorageRequest> StorageRequests => Set<StorageRequest>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Announcement> Announcements => Set<Announcement>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        
        b.Entity<Admin>(e =>
        {
            e.ToTable("admins");
            e.HasKey(x => x.AdminId).HasName("PK_admins");
            e.Property(x => x.FirstName).HasMaxLength(50).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(50).IsRequired();
            e.Property(x => x.Email).HasMaxLength(100).IsRequired();
            e.Property(x => x.Password).HasMaxLength(255).IsUnicode(false).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(12).IsUnicode(false).IsRequired();
            e.Property(x => x.AdminRole).HasMaxLength(30).IsUnicode(false).HasDefaultValue("Operations Admin");
            e.Property(x => x.CreatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSDATETIME()").ValueGeneratedOnAdd();
            e.HasIndex(x => x.Email).IsUnique().HasDatabaseName("UQ_admins_email");
            e.HasIndex(x => x.Phone).IsUnique().HasDatabaseName("UQ_admins_phone");
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_admins_phone", "phone LIKE '9665[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'");
                t.HasCheckConstraint("CK_admins_role", "admin_role IN ('Super Admin','Operations Admin')");
            });
        });

        
        b.Entity<Customer>(e =>
        {
            e.ToTable("customers");
            e.HasKey(x => x.CustomerId).HasName("PK_customers");
            e.Property(x => x.FirstName).HasMaxLength(50).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(50).IsRequired();
            e.Property(x => x.Email).HasMaxLength(100).IsRequired();
            e.Property(x => x.Password).HasMaxLength(255).IsUnicode(false).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(12).IsUnicode(false).IsRequired();
            e.Property(x => x.Address).HasMaxLength(200);
            e.Property(x => x.City).HasMaxLength(50).IsRequired();
            e.Property(x => x.CardHolderName).HasMaxLength(100);
            e.Property(x => x.CardNumber).HasMaxLength(19).IsUnicode(false);   
            e.Property(x => x.CardType).HasMaxLength(20).IsUnicode(false);
            e.Property(x => x.ExpiryDate).HasColumnType("char(5)");            
            e.Property(x => x.AccountStatus).HasMaxLength(20).IsUnicode(false).HasDefaultValue("Active");
            e.Property(x => x.CreatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSDATETIME()").ValueGeneratedOnAdd();
            e.HasIndex(x => x.Email).IsUnique().HasDatabaseName("UQ_customers_email");
            e.HasIndex(x => x.Phone).IsUnique().HasDatabaseName("UQ_customers_phone");
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_customers_phone", "phone LIKE '9665[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'");
                t.HasCheckConstraint("CK_customers_status", "account_status IN ('Active','Suspended','Blocked')");
                t.HasCheckConstraint("CK_customers_card_type", "card_type IS NULL OR card_type IN ('Mada','Visa','Mastercard')");
                t.HasCheckConstraint("CK_customers_card_mask", "card_number IS NULL OR card_number LIKE '**** **** **** [0-9][0-9][0-9][0-9]'");
                t.HasCheckConstraint("CK_customers_expiry", "expiry_date IS NULL OR expiry_date LIKE '[0-1][0-9]/[0-9][0-9]'");
            });
        });

        
        b.Entity<Staff>(e =>
        {
            e.ToTable("staff");
            e.HasKey(x => x.StaffId).HasName("PK_staff");
            e.Property(x => x.FirstName).HasMaxLength(50).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(50).IsRequired();
            e.Property(x => x.Email).HasMaxLength(100).IsRequired();
            e.Property(x => x.Password).HasMaxLength(255).IsUnicode(false).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(12).IsUnicode(false).IsRequired();
            e.Property(x => x.JobTitle).HasMaxLength(30).IsUnicode(false).IsRequired();
            e.Property(x => x.LicenseNumber).HasMaxLength(30).IsUnicode(false);
            e.Property(x => x.HireDate).HasColumnType("date");
            e.Property(x => x.StaffStatus).HasMaxLength(20).IsUnicode(false).HasDefaultValue("Active");
            e.Property(x => x.Specialty).HasMaxLength(40);
            e.Property(x => x.ShiftStart).HasColumnType("time(0)");
            e.Property(x => x.ShiftEnd).HasColumnType("time(0)");
            e.HasIndex(x => x.Email).IsUnique().HasDatabaseName("UQ_staff_email");
            e.HasIndex(x => x.Phone).IsUnique().HasDatabaseName("UQ_staff_phone");
            e.HasIndex(x => x.AdminId).HasDatabaseName("IX_staff_admin");
            
            e.HasOne(x => x.Admin).WithMany(a => a.Staff).HasForeignKey(x => x.AdminId)
                .HasConstraintName("FK_staff_admin").OnDelete(DeleteBehavior.NoAction);
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_staff_phone", "phone LIKE '9665[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'");
                t.HasCheckConstraint("CK_staff_title", "job_title IN ('Driver','Mover','Warehouse Keeper')");
                t.HasCheckConstraint("CK_staff_status", "staff_status IN ('Active','On Leave','Inactive')");
                t.HasCheckConstraint("CK_staff_license", "job_title <> 'Driver' OR license_number IS NOT NULL");
                t.HasCheckConstraint("CK_staff_shift", "shift_start IS NULL OR shift_end IS NULL OR shift_end > shift_start");
            });
        });

        
        b.Entity<Vehicle>(e =>
        {
            e.ToTable("vehicles");
            e.HasKey(x => x.VehicleId).HasName("PK_vehicles");
            e.Property(x => x.PlateNumber).HasMaxLength(20).IsUnicode(false).IsRequired();
            e.Property(x => x.VehicleType).HasMaxLength(40).IsUnicode(false).IsRequired();
            e.Property(x => x.CapacityVolumeM3).HasColumnType("decimal(6,2)");
            e.Property(x => x.MaxWeightKg).HasColumnType("decimal(8,2)");
            e.Property(x => x.IsTemperatureControlled).HasDefaultValue(false);
            e.Property(x => x.VehicleStatus).HasMaxLength(20).IsUnicode(false).HasDefaultValue("Available");
            e.Property(x => x.CreatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSDATETIME()").ValueGeneratedOnAdd();
            e.Property(x => x.UpdatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSDATETIME()");
            e.HasIndex(x => x.PlateNumber).IsUnique().HasDatabaseName("UQ_vehicles_plate");
            e.HasIndex(x => x.AdminId).HasDatabaseName("IX_vehicles_admin");
            e.HasOne(x => x.Admin).WithMany(a => a.Vehicles).HasForeignKey(x => x.AdminId)
                .HasConstraintName("FK_vehicles_admin").OnDelete(DeleteBehavior.NoAction);
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_vehicles_type", "vehicle_type IN ('Small Pickup','Medium Truck','Large Freight','Temp-Controlled')");
                t.HasCheckConstraint("CK_vehicles_capacity", "capacity_volume_m3 > 0");
                t.HasCheckConstraint("CK_vehicles_weight", "max_weight_kg BETWEEN 100 AND 40000");
                t.HasCheckConstraint("CK_vehicles_status", "vehicle_status IN ('Available','On Job','Maintenance','Out of Service')");
                t.HasCheckConstraint("CK_vehicles_temp_type", "(vehicle_type = 'Temp-Controlled' AND is_temperature_controlled = 1) OR (vehicle_type <> 'Temp-Controlled' AND is_temperature_controlled = 0)");
            });
        });

        
        b.Entity<Warehouse>(e =>
        {
            e.ToTable("warehouses");
            e.HasKey(x => x.WarehouseId).HasName("PK_warehouses");
            e.Property(x => x.WarehouseName).HasMaxLength(80).IsRequired();
            e.Property(x => x.Address).HasMaxLength(200).IsRequired();
            e.Property(x => x.City).HasMaxLength(50).IsRequired();
            e.Property(x => x.TotalCapacityM3).HasColumnType("decimal(8,2)");
            e.Property(x => x.UsedCapacityM3).HasColumnType("decimal(8,2)").HasDefaultValue(0m);
            e.Property(x => x.OpeningHours).HasMaxLength(60).IsRequired();
            e.HasIndex(x => x.WarehouseName).IsUnique().HasDatabaseName("UQ_warehouses_name");
            e.HasIndex(x => x.KeeperId).HasDatabaseName("IX_warehouses_keeper");
            
            e.HasOne(x => x.Keeper).WithMany(s => s.Warehouses).HasForeignKey(x => x.KeeperId)
                .HasConstraintName("FK_warehouses_keeper").OnDelete(DeleteBehavior.NoAction);
            e.ToTable(t => t.HasCheckConstraint("CK_warehouses_capacity", "total_capacity_m3 > 0 AND used_capacity_m3 >= 0 AND used_capacity_m3 <= total_capacity_m3"));
        });

        
        b.Entity<ItemCategory>(e =>
        {
            e.ToTable("item_categories");
            e.HasKey(x => x.CategoryId).HasName("PK_item_categories");
            e.Property(x => x.CategoryName).HasMaxLength(60).IsRequired();
            e.Property(x => x.Description).HasMaxLength(255);
            e.Property(x => x.RequiresTemperatureControl).HasDefaultValue(false);
            e.Property(x => x.RecommendedVehicleType).HasMaxLength(40).IsUnicode(false).IsRequired();
            e.HasIndex(x => x.CategoryName).IsUnique().HasDatabaseName("UQ_item_categories_name");
            e.HasOne(x => x.Admin).WithMany(a => a.ItemCategories).HasForeignKey(x => x.AdminId)
                .HasConstraintName("FK_item_categories_admin").OnDelete(DeleteBehavior.NoAction);
            e.ToTable(t => t.HasCheckConstraint("CK_item_categories_vehicle", "recommended_vehicle_type IN ('Small Pickup','Medium Truck','Large Freight','Temp-Controlled')"));
        });

        
        b.Entity<Service>(e =>
        {
            e.ToTable("services");
            e.HasKey(x => x.ServiceId).HasName("PK_services");
            e.Property(x => x.ServiceName).HasMaxLength(60).IsRequired();
            e.Property(x => x.ServiceType).HasMaxLength(20).IsUnicode(false).IsRequired();
            e.Property(x => x.Description).HasMaxLength(255);
            e.Property(x => x.BasePrice).HasColumnType("decimal(10,2)");
            e.Property(x => x.PricingUnit).HasMaxLength(20).IsUnicode(false).IsRequired();
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.HasIndex(x => x.ServiceName).IsUnique().HasDatabaseName("UQ_services_name");
            e.HasOne(x => x.Admin).WithMany(a => a.Services).HasForeignKey(x => x.AdminId)
                .HasConstraintName("FK_services_admin").OnDelete(DeleteBehavior.NoAction);
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_services_type", "service_type IN ('Transport','Storage','Value-Added')");
                t.HasCheckConstraint("CK_services_price", "base_price >= 0");
                t.HasCheckConstraint("CK_services_unit", "pricing_unit IN ('per job','per item','per worker','per day','per week','per month','first 7 days')");
            });
        });

        
        b.Entity<Booking>(e =>
        {
            e.ToTable("bookings");
            e.HasKey(x => x.BookingId).HasName("PK_bookings");
            e.Property(x => x.BookingId).UseIdentityColumn(4181, 1);   
            e.Property(x => x.BookingStatus).HasMaxLength(30).IsUnicode(false).HasDefaultValue("Pending");
            e.Property(x => x.PickupAddress).HasMaxLength(200).IsRequired();
            e.Property(x => x.PickupCity).HasMaxLength(50).IsRequired();
            e.Property(x => x.DropoffAddress).HasMaxLength(200).IsRequired();
            e.Property(x => x.DropoffCity).HasMaxLength(50).IsRequired();
            e.Property(x => x.PickupDatetime).HasColumnType("datetime2(0)");
            e.Property(x => x.TotalVolumeM3).HasColumnType("decimal(6,2)");
            e.Property(x => x.RecommendedVehicleType).HasMaxLength(40).IsUnicode(false);
            e.Property(x => x.TransportFee).HasColumnType("decimal(10,2)");
            e.Property(x => x.TotalAmount).HasColumnType("decimal(10,2)");
            e.Property(x => x.ExpectedDeliveryDatetime).HasColumnType("datetime2(0)");
            e.Property(x => x.CreatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSDATETIME()").ValueGeneratedOnAdd();
            e.Property(x => x.CustomerNotes).HasMaxLength(240);
            e.Property(x => x.RejectionReason).HasMaxLength(200);
            e.Property(x => x.DistanceKm).HasColumnType("decimal(7,2)");
            e.HasIndex(x => x.CustomerId).HasDatabaseName("IX_bookings_customer");
            e.HasIndex(x => x.StaffId).HasDatabaseName("IX_bookings_driver");
            e.HasIndex(x => x.VehicleId).HasDatabaseName("IX_bookings_vehicle");
            e.HasIndex(x => new { x.BookingStatus, x.PickupDatetime }).HasDatabaseName("IX_bookings_status");
            e.HasOne(x => x.Customer).WithMany(c => c.Bookings).HasForeignKey(x => x.CustomerId)
                .HasConstraintName("FK_bookings_customer").OnDelete(DeleteBehavior.NoAction);
            
            e.HasOne(x => x.Driver).WithMany(s => s.Bookings).HasForeignKey(x => x.StaffId)
                .HasConstraintName("FK_bookings_driver").OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Vehicle).WithMany(v => v.Bookings).HasForeignKey(x => x.VehicleId)
                .HasConstraintName("FK_bookings_vehicle").OnDelete(DeleteBehavior.NoAction);
            
            e.HasMany(x => x.Services).WithMany(s => s.Bookings).UsingEntity<BookingService>(
                r => r.HasOne(bs => bs.Service).WithMany(s => s.BookingServices).HasForeignKey(bs => bs.ServiceId)
                    .HasConstraintName("FK_booking_services_service").OnDelete(DeleteBehavior.NoAction),
                l => l.HasOne(bs => bs.Booking).WithMany(bk => bk.BookingServices).HasForeignKey(bs => bs.BookingId)
                    .HasConstraintName("FK_booking_services_booking").OnDelete(DeleteBehavior.Cascade));
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_bookings_status", "booking_status IN ('Pending','Accepted','Rejected','Picked Up','In Transit','In Storage','Out for Delivery','Completed','Cancelled')");
                t.HasCheckConstraint("CK_bookings_vehicle_type", "recommended_vehicle_type IS NULL OR recommended_vehicle_type IN ('Small Pickup','Medium Truck','Large Freight','Temp-Controlled')");
                t.HasCheckConstraint("CK_bookings_money", "(transport_fee IS NULL OR transport_fee >= 0) AND (total_amount IS NULL OR total_amount >= 0)");
                t.HasCheckConstraint("CK_bookings_volume", "total_volume_m3 IS NULL OR total_volume_m3 >= 0");
                t.HasCheckConstraint("CK_bookings_delivery", "expected_delivery_datetime IS NULL OR expected_delivery_datetime >= pickup_datetime");
                t.HasCheckConstraint("CK_bookings_reject", "booking_status <> 'Rejected' OR rejection_reason IS NOT NULL");
            });
        });

        
        b.Entity<BookingService>(e =>
        {
            e.ToTable("booking_services");
            e.HasKey(x => x.BookingServiceId).HasName("PK_booking_services");
            e.Property(x => x.Quantity).HasDefaultValue(1);
            e.Property(x => x.Subtotal).HasColumnType("decimal(10,2)");
            e.HasIndex(x => new { x.BookingId, x.ServiceId }).IsUnique().HasDatabaseName("UQ_booking_services");
            e.HasIndex(x => x.ServiceId).HasDatabaseName("IX_booking_services_service");
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_booking_services_qty", "quantity >= 1");
                t.HasCheckConstraint("CK_booking_services_sub", "subtotal >= 0");
            });
        });

        
        b.Entity<Item>(e =>
        {
            e.ToTable("items");
            e.HasKey(x => x.ItemId).HasName("PK_items");
            e.Property(x => x.ItemName).HasMaxLength(80).IsRequired();
            e.Property(x => x.Quantity).HasDefaultValue(1);
            e.Property(x => x.EstimatedLengthCm).HasColumnType("decimal(6,2)");
            e.Property(x => x.EstimatedWidthCm).HasColumnType("decimal(6,2)");
            e.Property(x => x.EstimatedHeightCm).HasColumnType("decimal(6,2)");
            e.Property(x => x.EstimatedVolumeM3).HasColumnType("decimal(6,2)");
            e.Property(x => x.EstimatedWeightKg).HasColumnType("decimal(8,2)");
            e.Property(x => x.AiConfidenceScore).HasColumnType("decimal(5,2)");
            e.Property(x => x.MeasuredAt).HasColumnType("datetime2(0)");
            e.HasIndex(x => x.BookingId).HasDatabaseName("IX_items_booking");
            e.HasIndex(x => x.CategoryId).HasDatabaseName("IX_items_category");
            e.HasOne(x => x.Booking).WithMany(bk => bk.Items).HasForeignKey(x => x.BookingId)
                .HasConstraintName("FK_items_booking").OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Category).WithMany(c => c.Items).HasForeignKey(x => x.CategoryId)
                .HasConstraintName("FK_items_category").OnDelete(DeleteBehavior.NoAction);
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_items_qty", "quantity >= 1");
                t.HasCheckConstraint("CK_items_conf", "ai_confidence_score IS NULL OR ai_confidence_score BETWEEN 0 AND 100");
                t.HasCheckConstraint("CK_items_dims", "(estimated_length_cm IS NULL OR estimated_length_cm > 0) AND (estimated_width_cm IS NULL OR estimated_width_cm > 0) AND (estimated_height_cm IS NULL OR estimated_height_cm > 0) AND (estimated_volume_m3 IS NULL OR estimated_volume_m3 >= 0) AND (estimated_weight_kg IS NULL OR estimated_weight_kg >= 0)");
            });
        });

        
        b.Entity<ItemPhoto>(e =>
        {
            e.ToTable("item_photos");
            e.HasKey(x => x.ItemPhotoId).HasName("PK_item_photos");
            e.Property(x => x.PhotoUrl).HasMaxLength(500).IsRequired();   
            e.Property(x => x.UploadedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSDATETIME()").ValueGeneratedOnAdd();
            e.HasIndex(x => x.ItemId).HasDatabaseName("IX_item_photos_item");
            e.HasOne(x => x.Item).WithMany(i => i.Photos).HasForeignKey(x => x.ItemId)
                .HasConstraintName("FK_item_photos_item").OnDelete(DeleteBehavior.Cascade);
        });

        
        b.Entity<StorageRequest>(e =>
        {
            e.ToTable("storage_requests");
            e.HasKey(x => x.StorageId).HasName("PK_storage_requests");
            e.Property(x => x.StorageLocation).HasMaxLength(50).IsUnicode(false).IsRequired();
            e.Property(x => x.StartDate).HasColumnType("date");
            e.Property(x => x.FreePeriodEndDate).HasColumnType("date");
            e.Property(x => x.EndDate).HasColumnType("date");
            e.Property(x => x.ExtensionType).HasMaxLength(20).IsUnicode(false);
            e.Property(x => x.StorageFee).HasColumnType("decimal(10,2)").HasDefaultValue(0m);
            e.Property(x => x.StorageStatus).HasMaxLength(20).IsUnicode(false).HasDefaultValue("Reserved");
            e.HasIndex(x => x.BookingId).IsUnique().HasDatabaseName("UQ_storage_requests_booking");
            e.HasIndex(x => x.WarehouseId).HasDatabaseName("IX_storage_warehouse");
            e.HasOne(x => x.Booking).WithOne(bk => bk.StorageRequest).HasForeignKey<StorageRequest>(x => x.BookingId)
                .HasConstraintName("FK_storage_booking").OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Warehouse).WithMany(w => w.StorageRequests).HasForeignKey(x => x.WarehouseId)
                .HasConstraintName("FK_storage_warehouse").OnDelete(DeleteBehavior.NoAction);
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_storage_free", "free_period_end_date = DATEADD(DAY, 7, start_date)");
                t.HasCheckConstraint("CK_storage_end", "end_date IS NULL OR end_date >= start_date");
                t.HasCheckConstraint("CK_storage_ext", "(extension_type IS NULL AND extension_units IS NULL) OR (extension_type IN ('Daily','Weekly','Monthly') AND extension_units >= 1)");
                t.HasCheckConstraint("CK_storage_fee", "storage_fee >= 0");
                t.HasCheckConstraint("CK_storage_status", "storage_status IN ('Reserved','Free Period','Extended','Released')");
            });
        });

        
        b.Entity<Payment>(e =>
        {
            e.ToTable("payments");
            e.HasKey(x => x.PaymentId).HasName("PK_payments");
            e.Property(x => x.InvoiceNumber).HasMaxLength(30).IsUnicode(false).IsRequired();
            e.Property(x => x.RefNumber).HasMaxLength(40).IsUnicode(false);
            e.Property(x => x.Amount).HasColumnType("decimal(10,2)");
            e.Property(x => x.PaymentMethod).HasMaxLength(30).IsUnicode(false).IsRequired();
            e.Property(x => x.PaymentStatus).HasMaxLength(20).IsUnicode(false).HasDefaultValue("Payment Pending");
            e.Property(x => x.TransactionDate).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSDATETIME()");
            e.HasIndex(x => x.InvoiceNumber).IsUnique().HasDatabaseName("UQ_payments_invoice");
            e.HasIndex(x => x.BookingId).HasDatabaseName("IX_payments_booking");
            e.HasOne(x => x.Booking).WithMany(bk => bk.Payments).HasForeignKey(x => x.BookingId)
                .HasConstraintName("FK_payments_booking").OnDelete(DeleteBehavior.NoAction);
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_payments_amount", "amount > 0");
                t.HasCheckConstraint("CK_payments_method", "payment_method IN ('Mada','Credit Card','Apple Pay','STC Pay')");
                t.HasCheckConstraint("CK_payments_status", "payment_status IN ('Paid','Payment Pending','Failed','Refunded')");
            });
        });

        
        b.Entity<Review>(e =>
        {
            e.ToTable("reviews");
            e.HasKey(x => x.ReviewId).HasName("PK_reviews");
            e.Property(x => x.Rating).HasColumnType("tinyint");
            e.Property(x => x.Comment).HasMaxLength(500);
            e.Property(x => x.CreatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSDATETIME()").ValueGeneratedOnAdd();
            e.HasIndex(x => x.BookingId).IsUnique().HasDatabaseName("UQ_reviews_booking");
            e.HasIndex(x => x.CustomerId).HasDatabaseName("IX_reviews_customer");
            e.HasOne(x => x.Booking).WithOne(bk => bk.Review).HasForeignKey<Review>(x => x.BookingId)
                .HasConstraintName("FK_reviews_booking").OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Customer).WithMany(c => c.Reviews).HasForeignKey(x => x.CustomerId)
                .HasConstraintName("FK_reviews_customer").OnDelete(DeleteBehavior.NoAction);
            
            e.HasOne(x => x.Moderator).WithMany(a => a.ModeratedReviews).HasForeignKey(x => x.AdminId)
                .HasConstraintName("FK_reviews_admin").OnDelete(DeleteBehavior.NoAction);
            e.ToTable(t => t.HasCheckConstraint("CK_reviews_rating", "rating BETWEEN 1 AND 5"));
        });

        
        b.Entity<Message>(e =>
        {
            e.ToTable("messages");
            e.HasKey(x => x.MessageId).HasName("PK_messages");
            e.Property(x => x.SenderType).HasMaxLength(20).IsUnicode(false).IsRequired();
            e.Property(x => x.MessageText).HasMaxLength(1000).IsRequired();
            e.Property(x => x.SentAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSDATETIME()");
            e.Property(x => x.IsRead).HasDefaultValue(false);
            e.HasIndex(x => new { x.BookingId, x.SentAt }).HasDatabaseName("IX_messages_booking");
            e.HasOne(x => x.Booking).WithMany(bk => bk.Messages).HasForeignKey(x => x.BookingId)
                .HasConstraintName("FK_messages_booking").OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Customer).WithMany(c => c.Messages).HasForeignKey(x => x.CustomerId)
                .HasConstraintName("FK_messages_customer").OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Staff).WithMany(s => s.Messages).HasForeignKey(x => x.StaffId)
                .HasConstraintName("FK_messages_staff").OnDelete(DeleteBehavior.NoAction);
            e.ToTable(t => t.HasCheckConstraint("CK_messages_sender", "sender_type IN ('Customer','Staff')"));
        });

        
        b.Entity<Announcement>(e =>
        {
            e.ToTable("announcements");
            e.HasKey(x => x.AnnouncementId).HasName("PK_announcements");
            e.Property(x => x.AnnouncementType).HasMaxLength(40).IsUnicode(false).IsRequired();
            e.Property(x => x.Title).HasMaxLength(100).IsRequired();
            e.Property(x => x.Content).HasMaxLength(500).IsRequired();
            e.Property(x => x.SentAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSDATETIME()");
            e.Property(x => x.IsRead).HasDefaultValue(false);
            e.HasIndex(x => new { x.CustomerId, x.IsRead }).HasDatabaseName("IX_announcements_customer");
            e.HasIndex(x => new { x.StaffId, x.IsRead }).HasDatabaseName("IX_announcements_staff");
            e.HasOne(x => x.Admin).WithMany(a => a.Announcements).HasForeignKey(x => x.AdminId)
                .HasConstraintName("FK_announcements_admin").OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Customer).WithMany(c => c.Announcements).HasForeignKey(x => x.CustomerId)
                .HasConstraintName("FK_announcements_customer").OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Staff).WithMany(s => s.Announcements).HasForeignKey(x => x.StaffId)
                .HasConstraintName("FK_announcements_staff").OnDelete(DeleteBehavior.NoAction);
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_announcements_type", "announcement_type IN ('Booking Accepted','Booking Rejected','Status Update','Payment Confirmed','Storage Reminder','Job Assigned','Service Update','Offer')");
                t.HasCheckConstraint("CK_announcements_target", "customer_id IS NULL OR staff_id IS NULL");
            });
        });
    }

    
    
    
    
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        TouchVehicleTimestamps();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        TouchVehicleTimestamps();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void TouchVehicleTimestamps()
    {
        var now = DateTime.Now;
        foreach (var entry in ChangeTracker.Entries<Vehicle>())
        {
            if (entry.State == EntityState.Added) { entry.Entity.CreatedAt = now; entry.Entity.UpdatedAt = now; }
            else if (entry.State == EntityState.Modified) entry.Entity.UpdatedAt = now;
        }
    }
}
