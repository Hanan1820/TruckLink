
CREATE TABLE admins (
    admin_id     INT IDENTITY(1,1) NOT NULL,
    first_name   NVARCHAR(50)  NOT NULL,
    last_name    NVARCHAR(50)  NOT NULL,
    email        NVARCHAR(100) NOT NULL,
    password     VARCHAR(255)  NOT NULL,
    phone        VARCHAR(12)   NOT NULL,
    admin_role   VARCHAR(30)   NOT NULL CONSTRAINT DF_admins_role DEFAULT ('Operations Admin'),
    created_at   DATETIME2(0)  NOT NULL CONSTRAINT DF_admins_created DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_admins PRIMARY KEY (admin_id),
    CONSTRAINT UQ_admins_email UNIQUE (email),
    CONSTRAINT UQ_admins_phone UNIQUE (phone),
    CONSTRAINT CK_admins_phone CHECK (phone LIKE '9665[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'),
    CONSTRAINT CK_admins_role  CHECK (admin_role IN ('Super Admin','Operations Admin'))
);
GO

/* 2. CUSTOMER -------------------------------------------------------- */
CREATE TABLE customers (
    customer_id      INT IDENTITY(1,1) NOT NULL,
    first_name       NVARCHAR(50)  NOT NULL,
    last_name        NVARCHAR(50)  NOT NULL,
    email            NVARCHAR(100) NOT NULL,
    password         VARCHAR(255)  NOT NULL,
    phone            VARCHAR(12)   NOT NULL,
    address          NVARCHAR(200) NULL,
    city             NVARCHAR(50)  NOT NULL,
    card_holder_name NVARCHAR(100) NULL,
    card_number      VARCHAR(19)   NULL,   -- masked only: '**** **** **** 4821' (PCI: never the full PAN, never CVV)
    card_type        VARCHAR(20)   NULL,
    expiry_date      CHAR(5)       NULL,   -- 'MM/YY'
    account_status   VARCHAR(20)   NOT NULL CONSTRAINT DF_customers_status DEFAULT ('Active'),
    created_at       DATETIME2(0)  NOT NULL CONSTRAINT DF_customers_created DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_customers PRIMARY KEY (customer_id),
    CONSTRAINT UQ_customers_email UNIQUE (email),
    CONSTRAINT UQ_customers_phone UNIQUE (phone),
    CONSTRAINT CK_customers_phone  CHECK (phone LIKE '9665[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'),
    CONSTRAINT CK_customers_status CHECK (account_status IN ('Active','Suspended','Blocked')),
    CONSTRAINT CK_customers_card_type CHECK (card_type IS NULL OR card_type IN ('Mada','Visa','Mastercard')),
    CONSTRAINT CK_customers_card_mask CHECK (card_number IS NULL OR card_number LIKE '**** **** **** [0-9][0-9][0-9][0-9]'),
    CONSTRAINT CK_customers_expiry CHECK (expiry_date IS NULL OR expiry_date LIKE '[0-1][0-9]/[0-9][0-9]')
);
GO

/* 3. STAFF (drivers, movers, warehouse keepers) ---------------------- */
CREATE TABLE staff (
    staff_id        INT IDENTITY(1,1) NOT NULL,
    admin_id        INT           NOT NULL,
    first_name      NVARCHAR(50)  NOT NULL,
    last_name       NVARCHAR(50)  NOT NULL,
    email           NVARCHAR(100) NOT NULL,
    password        VARCHAR(255)  NOT NULL,
    phone           VARCHAR(12)   NOT NULL,
    job_title       VARCHAR(30)   NOT NULL,
    license_number  VARCHAR(30)   NULL,
    hire_date       DATE          NOT NULL,
    staff_status    VARCHAR(20)   NOT NULL CONSTRAINT DF_staff_status DEFAULT ('Active'),
    specialty       NVARCHAR(40)  NULL,   -- [UI] UC2-03 "Mover · assembly"
    shift_start     TIME(0)       NULL,   -- [UI] UC2-03 "free 07:00–17:00"
    shift_end       TIME(0)       NULL,   -- [UI]
    CONSTRAINT PK_staff PRIMARY KEY (staff_id),
    CONSTRAINT UQ_staff_email UNIQUE (email),
    CONSTRAINT UQ_staff_phone UNIQUE (phone),
    CONSTRAINT FK_staff_admin FOREIGN KEY (admin_id) REFERENCES admins (admin_id),
    CONSTRAINT CK_staff_phone  CHECK (phone LIKE '9665[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'),
    CONSTRAINT CK_staff_title  CHECK (job_title IN ('Driver','Mover','Warehouse Keeper')),
    CONSTRAINT CK_staff_status CHECK (staff_status IN ('Active','On Leave','Inactive')),
    CONSTRAINT CK_staff_license CHECK (job_title <> 'Driver' OR license_number IS NOT NULL),
    CONSTRAINT CK_staff_shift  CHECK (shift_start IS NULL OR shift_end IS NULL OR shift_end > shift_start)
);
GO
CREATE INDEX IX_staff_admin ON staff (admin_id);
GO

/* 4. VEHICLE --------------------------------------------------------- */
CREATE TABLE vehicles (
    vehicle_id                INT IDENTITY(1,1) NOT NULL,
    admin_id                  INT           NOT NULL,
    plate_number              VARCHAR(20)   NOT NULL,
    vehicle_type              VARCHAR(40)   NOT NULL,
    capacity_volume_m3        DECIMAL(6,2)  NOT NULL,
    max_weight_kg             DECIMAL(8,2)  NOT NULL,
    is_temperature_controlled BIT           NOT NULL CONSTRAINT DF_vehicles_temp DEFAULT (0),
    vehicle_status            VARCHAR(20)   NOT NULL CONSTRAINT DF_vehicles_status DEFAULT ('Available'),
    created_at                DATETIME2(0)  NOT NULL CONSTRAINT DF_vehicles_created DEFAULT (SYSDATETIME()), -- [UI] "Registered" column
    updated_at                DATETIME2(0)  NOT NULL CONSTRAINT DF_vehicles_updated DEFAULT (SYSDATETIME()), -- [UI] "Last updated"
    CONSTRAINT PK_vehicles PRIMARY KEY (vehicle_id),
    CONSTRAINT UQ_vehicles_plate UNIQUE (plate_number),
    CONSTRAINT FK_vehicles_admin FOREIGN KEY (admin_id) REFERENCES admins (admin_id),
    CONSTRAINT CK_vehicles_type     CHECK (vehicle_type IN ('Small Pickup','Medium Truck','Large Freight','Temp-Controlled')),
    CONSTRAINT CK_vehicles_capacity CHECK (capacity_volume_m3 > 0),
    CONSTRAINT CK_vehicles_weight   CHECK (max_weight_kg BETWEEN 100 AND 40000),
    CONSTRAINT CK_vehicles_status   CHECK (vehicle_status IN ('Available','On Job','Maintenance','Out of Service')),
    CONSTRAINT CK_vehicles_temp_type CHECK ((vehicle_type = 'Temp-Controlled' AND is_temperature_controlled = 1)
                                         OR (vehicle_type <> 'Temp-Controlled' AND is_temperature_controlled = 0))
);
GO
CREATE INDEX IX_vehicles_admin ON vehicles (admin_id);
GO

/* 5. WAREHOUSE ------------------------------------------------------- */
CREATE TABLE warehouses (
    warehouse_id      INT IDENTITY(1,1) NOT NULL,
    keeper_id         INT           NOT NULL,
    warehouse_name    NVARCHAR(80)  NOT NULL,
    address           NVARCHAR(200) NOT NULL,
    city              NVARCHAR(50)  NOT NULL,
    total_capacity_m3 DECIMAL(8,2)  NOT NULL,
    used_capacity_m3  DECIMAL(8,2)  NOT NULL CONSTRAINT DF_warehouses_used DEFAULT (0),
    opening_hours     NVARCHAR(60)  NOT NULL,
    CONSTRAINT PK_warehouses PRIMARY KEY (warehouse_id),
    CONSTRAINT UQ_warehouses_name UNIQUE (warehouse_name),
    CONSTRAINT FK_warehouses_keeper FOREIGN KEY (keeper_id) REFERENCES staff (staff_id),
    CONSTRAINT CK_warehouses_capacity CHECK (total_capacity_m3 > 0 AND used_capacity_m3 >= 0 AND used_capacity_m3 <= total_capacity_m3)
);
GO
CREATE INDEX IX_warehouses_keeper ON warehouses (keeper_id);
GO

/* 6. ITEM CATEGORY --------------------------------------------------- */
CREATE TABLE item_categories (
    category_id                  INT IDENTITY(1,1) NOT NULL,
    admin_id                     INT           NOT NULL,
    category_name                NVARCHAR(60)  NOT NULL,
    description                  NVARCHAR(255) NULL,
    requires_temperature_control BIT           NOT NULL CONSTRAINT DF_categories_temp DEFAULT (0),
    recommended_vehicle_type     VARCHAR(40)   NOT NULL,
    CONSTRAINT PK_item_categories PRIMARY KEY (category_id),
    CONSTRAINT UQ_item_categories_name UNIQUE (category_name),
    CONSTRAINT FK_item_categories_admin FOREIGN KEY (admin_id) REFERENCES admins (admin_id),
    CONSTRAINT CK_item_categories_vehicle CHECK (recommended_vehicle_type IN ('Small Pickup','Medium Truck','Large Freight','Temp-Controlled'))
);
GO

/* 7. SERVICE (transport, storage and value-added in one entity) ------ */
CREATE TABLE services (
    service_id    INT IDENTITY(1,1) NOT NULL,
    admin_id      INT           NOT NULL,
    service_name  NVARCHAR(60)  NOT NULL,
    service_type  VARCHAR(20)   NOT NULL,
    description   NVARCHAR(255) NULL,
    base_price    DECIMAL(10,2) NOT NULL,
    pricing_unit  VARCHAR(20)   NOT NULL,
    is_active     BIT           NOT NULL CONSTRAINT DF_services_active DEFAULT (1),
    CONSTRAINT PK_services PRIMARY KEY (service_id),
    CONSTRAINT UQ_services_name UNIQUE (service_name),
    CONSTRAINT FK_services_admin FOREIGN KEY (admin_id) REFERENCES admins (admin_id),
    CONSTRAINT CK_services_type  CHECK (service_type IN ('Transport','Storage','Value-Added')),
    CONSTRAINT CK_services_price CHECK (base_price >= 0),
    CONSTRAINT CK_services_unit  CHECK (pricing_unit IN ('per job','per item','per worker','per day','per week','per month','first 7 days'))
);
GO

/* 8. BOOKING (central entity) --------------------------------------- */
CREATE TABLE bookings (
    booking_id                 INT IDENTITY(4181,1) NOT NULL,  -- seeded at 4181 so references read BK-2026-004181… like the prototypes
    customer_id                INT            NOT NULL,
    staff_id                   INT            NULL,            -- assigned DRIVER
    vehicle_id                 INT            NULL,
    booking_status             VARCHAR(30)    NOT NULL CONSTRAINT DF_bookings_status DEFAULT ('Pending'),
    pickup_address             NVARCHAR(200)  NOT NULL,
    pickup_city                NVARCHAR(50)   NOT NULL,
    dropoff_address            NVARCHAR(200)  NOT NULL,
    dropoff_city               NVARCHAR(50)   NOT NULL,
    pickup_datetime            DATETIME2(0)   NOT NULL,
    total_volume_m3            DECIMAL(6,2)   NULL,
    recommended_vehicle_type   VARCHAR(40)    NULL,
    transport_fee              DECIMAL(10,2)  NULL,
    total_amount               DECIMAL(10,2)  NULL,
    expected_delivery_datetime DATETIME2(0)   NULL,
    created_at                 DATETIME2(0)   NOT NULL CONSTRAINT DF_bookings_created DEFAULT (SYSDATETIME()),
    customer_notes             NVARCHAR(240)  NULL,   -- [UI] UC1-02 "Notes for the crew", shown in UC2-02
    rejection_reason           NVARCHAR(200)  NULL,   -- [UI] UC2-04 "Recorded against the booking"
    distance_km                DECIMAL(7,2)   NULL,   -- [UI] UC2-02 "Riyadh · 23 km", fleet report kilometres
    CONSTRAINT PK_bookings PRIMARY KEY (booking_id),
    CONSTRAINT FK_bookings_customer FOREIGN KEY (customer_id) REFERENCES customers (customer_id),
    CONSTRAINT FK_bookings_driver   FOREIGN KEY (staff_id)    REFERENCES staff (staff_id),
    CONSTRAINT FK_bookings_vehicle  FOREIGN KEY (vehicle_id)  REFERENCES vehicles (vehicle_id),
    CONSTRAINT CK_bookings_status CHECK (booking_status IN ('Pending','Accepted','Rejected','Picked Up','In Transit','In Storage','Out for Delivery','Completed','Cancelled')),
    CONSTRAINT CK_bookings_vehicle_type CHECK (recommended_vehicle_type IS NULL OR recommended_vehicle_type IN ('Small Pickup','Medium Truck','Large Freight','Temp-Controlled')),
    CONSTRAINT CK_bookings_money  CHECK ((transport_fee IS NULL OR transport_fee >= 0) AND (total_amount IS NULL OR total_amount >= 0)),
    CONSTRAINT CK_bookings_volume CHECK (total_volume_m3 IS NULL OR total_volume_m3 >= 0),
    CONSTRAINT CK_bookings_delivery CHECK (expected_delivery_datetime IS NULL OR expected_delivery_datetime >= pickup_datetime),
    CONSTRAINT CK_bookings_reject CHECK (booking_status <> 'Rejected' OR rejection_reason IS NOT NULL)
);
GO
CREATE INDEX IX_bookings_customer ON bookings (customer_id);
CREATE INDEX IX_bookings_driver   ON bookings (staff_id);
CREATE INDEX IX_bookings_vehicle  ON bookings (vehicle_id);
CREATE INDEX IX_bookings_status   ON bookings (booking_status, pickup_datetime);
GO

/* 9. BOOKING SERVICE (resolves Booking M:N Service) ----------------- */
CREATE TABLE booking_services (
    booking_service_id INT IDENTITY(1,1) NOT NULL,
    booking_id         INT           NOT NULL,
    service_id         INT           NOT NULL,
    quantity           INT           NOT NULL CONSTRAINT DF_booking_services_qty DEFAULT (1),
    subtotal           DECIMAL(10,2) NOT NULL,
    CONSTRAINT PK_booking_services PRIMARY KEY (booking_service_id),
    CONSTRAINT UQ_booking_services UNIQUE (booking_id, service_id),
    CONSTRAINT FK_booking_services_booking FOREIGN KEY (booking_id) REFERENCES bookings (booking_id) ON DELETE CASCADE,
    CONSTRAINT FK_booking_services_service FOREIGN KEY (service_id) REFERENCES services (service_id),
    CONSTRAINT CK_booking_services_qty CHECK (quantity >= 1),
    CONSTRAINT CK_booking_services_sub CHECK (subtotal >= 0)
);
GO
CREATE INDEX IX_booking_services_service ON booking_services (service_id);
GO

/* 10. ITEM (AI measurement result stored on the item) --------------- */
CREATE TABLE items (
    item_id             INT IDENTITY(1,1) NOT NULL,
    booking_id          INT           NOT NULL,
    category_id         INT           NOT NULL,
    item_name           NVARCHAR(80)  NOT NULL,
    quantity            INT           NOT NULL CONSTRAINT DF_items_qty DEFAULT (1),
    estimated_length_cm DECIMAL(6,2)  NULL,
    estimated_width_cm  DECIMAL(6,2)  NULL,
    estimated_height_cm DECIMAL(6,2)  NULL,
    estimated_volume_m3 DECIMAL(6,2)  NULL,
    estimated_weight_kg DECIMAL(8,2)  NULL,
    ai_confidence_score DECIMAL(5,2)  NULL,   -- percent 0–100, shown as "92%"
    measured_at         DATETIME2(0)  NULL,   -- NULL = Queued / Measuring
    CONSTRAINT PK_items PRIMARY KEY (item_id),
    CONSTRAINT FK_items_booking  FOREIGN KEY (booking_id)  REFERENCES bookings (booking_id) ON DELETE CASCADE,
    CONSTRAINT FK_items_category FOREIGN KEY (category_id) REFERENCES item_categories (category_id),
    CONSTRAINT CK_items_qty  CHECK (quantity >= 1),
    CONSTRAINT CK_items_conf CHECK (ai_confidence_score IS NULL OR ai_confidence_score BETWEEN 0 AND 100),
    CONSTRAINT CK_items_dims CHECK ((estimated_length_cm IS NULL OR estimated_length_cm > 0)
                                AND (estimated_width_cm  IS NULL OR estimated_width_cm  > 0)
                                AND (estimated_height_cm IS NULL OR estimated_height_cm > 0)
                                AND (estimated_volume_m3 IS NULL OR estimated_volume_m3 >= 0)
                                AND (estimated_weight_kg IS NULL OR estimated_weight_kg >= 0))
);
GO
CREATE INDEX IX_items_booking  ON items (booking_id);
CREATE INDEX IX_items_category ON items (category_id);
GO

/* 11. ITEM PHOTO ----------------------------------------------------- */
CREATE TABLE item_photos (
    item_photo_id INT IDENTITY(1,1) NOT NULL,
    item_id       INT           NOT NULL,
    photo_url     NVARCHAR(500) NOT NULL,   -- 500: external image URLs (03_update_item_photos.sql) can be ~400 chars
    uploaded_at   DATETIME2(0)  NOT NULL CONSTRAINT DF_item_photos_uploaded DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_item_photos PRIMARY KEY (item_photo_id),
    CONSTRAINT FK_item_photos_item FOREIGN KEY (item_id) REFERENCES items (item_id) ON DELETE CASCADE
);
GO
CREATE INDEX IX_item_photos_item ON item_photos (item_id);
GO

/* 12. STORAGE REQUEST (1:1 with booking) ---------------------------- */
CREATE TABLE storage_requests (
    storage_id           INT IDENTITY(1,1) NOT NULL,
    booking_id           INT           NOT NULL,
    warehouse_id         INT           NOT NULL,
    storage_location     VARCHAR(50)   NOT NULL,
    start_date           DATE          NOT NULL,
    free_period_end_date DATE          NOT NULL,
    end_date             DATE          NULL,
    extension_type       VARCHAR(20)   NULL,
    extension_units      INT           NULL,
    storage_fee          DECIMAL(10,2) NOT NULL CONSTRAINT DF_storage_fee DEFAULT (0),
    storage_status       VARCHAR(20)   NOT NULL CONSTRAINT DF_storage_status DEFAULT ('Reserved'),
    CONSTRAINT PK_storage_requests PRIMARY KEY (storage_id),
    CONSTRAINT UQ_storage_requests_booking UNIQUE (booking_id),
    CONSTRAINT FK_storage_booking   FOREIGN KEY (booking_id)   REFERENCES bookings (booking_id) ON DELETE CASCADE,
    CONSTRAINT FK_storage_warehouse FOREIGN KEY (warehouse_id) REFERENCES warehouses (warehouse_id),
    CONSTRAINT CK_storage_free   CHECK (free_period_end_date = DATEADD(DAY, 7, start_date)),
    CONSTRAINT CK_storage_end    CHECK (end_date IS NULL OR end_date >= start_date),
    CONSTRAINT CK_storage_ext    CHECK ((extension_type IS NULL AND extension_units IS NULL)
                                     OR (extension_type IN ('Daily','Weekly','Monthly') AND extension_units >= 1)),
    CONSTRAINT CK_storage_fee    CHECK (storage_fee >= 0),
    CONSTRAINT CK_storage_status CHECK (storage_status IN ('Reserved','Free Period','Extended','Released'))
);
GO
CREATE INDEX IX_storage_warehouse ON storage_requests (warehouse_id);
GO

/* 13. PAYMENT (1:M — pay per service) -------------------------------- */
CREATE TABLE payments (
    payment_id       INT IDENTITY(1,1) NOT NULL,
    booking_id       INT           NOT NULL,
    invoice_number   VARCHAR(30)   NOT NULL,
    ref_number       VARCHAR(40)   NULL,
    amount           DECIMAL(10,2) NOT NULL,
    payment_method   VARCHAR(30)   NOT NULL,
    payment_status   VARCHAR(20)   NOT NULL CONSTRAINT DF_payments_status DEFAULT ('Payment Pending'),
    transaction_date DATETIME2(0)  NOT NULL CONSTRAINT DF_payments_date DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_payments PRIMARY KEY (payment_id),
    CONSTRAINT UQ_payments_invoice UNIQUE (invoice_number),
    CONSTRAINT FK_payments_booking FOREIGN KEY (booking_id) REFERENCES bookings (booking_id),
    CONSTRAINT CK_payments_amount CHECK (amount > 0),
    CONSTRAINT CK_payments_method CHECK (payment_method IN ('Mada','Credit Card','Apple Pay','STC Pay')),
    CONSTRAINT CK_payments_status CHECK (payment_status IN ('Paid','Payment Pending','Failed','Refunded'))
);
GO
CREATE INDEX IX_payments_booking ON payments (booking_id);
GO

/* 14. REVIEW (1:1 with booking) ------------------------------------- */
CREATE TABLE reviews (
    review_id   INT IDENTITY(1,1) NOT NULL,
    booking_id  INT           NOT NULL,
    customer_id INT           NOT NULL,
    admin_id    INT           NULL,     -- moderated by
    rating      TINYINT       NOT NULL,
    comment     NVARCHAR(500) NULL,
    created_at  DATETIME2(0)  NOT NULL CONSTRAINT DF_reviews_created DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_reviews PRIMARY KEY (review_id),
    CONSTRAINT UQ_reviews_booking UNIQUE (booking_id),
    CONSTRAINT FK_reviews_booking  FOREIGN KEY (booking_id)  REFERENCES bookings (booking_id),
    CONSTRAINT FK_reviews_customer FOREIGN KEY (customer_id) REFERENCES customers (customer_id),
    CONSTRAINT FK_reviews_admin    FOREIGN KEY (admin_id)    REFERENCES admins (admin_id),
    CONSTRAINT CK_reviews_rating CHECK (rating BETWEEN 1 AND 5)
);
GO
CREATE INDEX IX_reviews_customer ON reviews (customer_id);
GO

/* 15. MESSAGE (customer <-> assigned staff) ------------------------- */
CREATE TABLE messages (
    message_id   INT IDENTITY(1,1) NOT NULL,
    booking_id   INT            NOT NULL,
    customer_id  INT            NOT NULL,
    staff_id     INT            NOT NULL,
    sender_type  VARCHAR(20)    NOT NULL,
    message_text NVARCHAR(1000) NOT NULL,
    sent_at      DATETIME2(0)   NOT NULL CONSTRAINT DF_messages_sent DEFAULT (SYSDATETIME()),
    is_read      BIT            NOT NULL CONSTRAINT DF_messages_read DEFAULT (0),
    CONSTRAINT PK_messages PRIMARY KEY (message_id),
    CONSTRAINT FK_messages_booking  FOREIGN KEY (booking_id)  REFERENCES bookings (booking_id),
    CONSTRAINT FK_messages_customer FOREIGN KEY (customer_id) REFERENCES customers (customer_id),
    CONSTRAINT FK_messages_staff    FOREIGN KEY (staff_id)    REFERENCES staff (staff_id),
    CONSTRAINT CK_messages_sender CHECK (sender_type IN ('Customer','Staff'))
);
GO
CREATE INDEX IX_messages_booking ON messages (booking_id, sent_at);
GO

/* 16. ANNOUNCEMENT (admin announcements + system notifications) ----- */
CREATE TABLE announcements (
    announcement_id   INT IDENTITY(1,1) NOT NULL,
    admin_id          INT            NULL,   -- NULL = generated by the system
    customer_id       INT            NULL,
    staff_id          INT            NULL,
    announcement_type VARCHAR(40)    NOT NULL,
    title             NVARCHAR(100)  NOT NULL,
    content           NVARCHAR(500)  NOT NULL,
    sent_at           DATETIME2(0)   NOT NULL CONSTRAINT DF_announcements_sent DEFAULT (SYSDATETIME()),
    is_read           BIT            NOT NULL CONSTRAINT DF_announcements_read DEFAULT (0),
    CONSTRAINT PK_announcements PRIMARY KEY (announcement_id),
    CONSTRAINT FK_announcements_admin    FOREIGN KEY (admin_id)    REFERENCES admins (admin_id),
    CONSTRAINT FK_announcements_customer FOREIGN KEY (customer_id) REFERENCES customers (customer_id),
    CONSTRAINT FK_announcements_staff    FOREIGN KEY (staff_id)    REFERENCES staff (staff_id),
    CONSTRAINT CK_announcements_type CHECK (announcement_type IN ('Booking Accepted','Booking Rejected','Status Update','Payment Confirmed','Storage Reminder','Job Assigned','Service Update','Offer')),
    CONSTRAINT CK_announcements_target CHECK (customer_id IS NULL OR staff_id IS NULL)
);
GO
CREATE INDEX IX_announcements_customer ON announcements (customer_id, is_read);
CREATE INDEX IX_announcements_staff    ON announcements (staff_id, is_read);
GO
