
SET IDENTITY_INSERT admins ON;
INSERT INTO admins (admin_id, first_name, last_name, email, password, phone, admin_role, created_at) VALUES
(1, N'Abdulaziz', N'Al-Rashid', N'a.alrashid@trucklink.sa', '$2y$10$J4eyO6nIhvbH0RhH3Qs5WOs1sy3fXsSWH5q64A71RrD7OyaRQ6m4G', '966500112233', 'Super Admin', '2025-12-01 09:00:00'),
(2, N'Sara', N'Al-Dosari', N's.aldosari@trucklink.sa', '$2y$10$J4eyO6nIhvbH0RhH3Qs5WOs1sy3fXsSWH5q64A71RrD7OyaRQ6m4G', '966532447781', 'Operations Admin', '2026-01-05 09:00:00'),
(3, N'Majed', N'Al-Otaibi', N'm.alotaibi@trucklink.sa', '$2y$10$J4eyO6nIhvbH0RhH3Qs5WOs1sy3fXsSWH5q64A71RrD7OyaRQ6m4G', '966541239900', 'Operations Admin', '2026-02-10 09:00:00');
SET IDENTITY_INSERT admins OFF;
GO

/* customers (10) */
SET IDENTITY_INSERT customers ON;
INSERT INTO customers (customer_id, first_name, last_name, email, password, phone, address, city, card_holder_name, card_number, card_type, expiry_date, account_status, created_at) VALUES
(1, N'Hanan', N'Mohammed', N'hanan.mohammed@gmail.com', '$2y$10$J4eyO6nIhvbH0RhH3Qs5WOs1sy3fXsSWH5q64A71RrD7OyaRQ6m4G', '966551840227', N'Al Fanateer District, Street 12, Villa 4', N'Jubail', N'HANAN MOHAMMED', '**** **** **** 4821', 'Mada', '08/28', 'Active', '2026-08-20 10:00:00'),
(2, N'Huda', N'Ahmed', N'huda.ahmed@gmail.com', '$2y$10$J4eyO6nIhvbH0RhH3Qs5WOs1sy3fXsSWH5q64A71RrD7OyaRQ6m4G', '966563309145', N'Al Faisaliyah, King Saud Street, Bldg 9', N'Dammam', N'HUDA AHMED', '**** **** **** 1190', 'Visa', '11/27', 'Active', '2026-08-22 11:30:00'),
(3, N'Fai', N'Rashed', N'fai.rashed@hotmail.com', '$2y$10$J4eyO6nIhvbH0RhH3Qs5WOs1sy3fXsSWH5q64A71RrD7OyaRQ6m4G', '966577451062', N'Al Malqa District, Anas bin Malik Road', N'Riyadh', NULL, NULL, NULL, NULL, 'Active', '2026-08-25 09:15:00'),
(4, N'Danah', N'Mohmmad', N'danah.mohmmad@gmail.com', '$2y$10$J4eyO6nIhvbH0RhH3Qs5WOs1sy3fXsSWH5q64A71RrD7OyaRQ6m4G', '966509938417', N'Al Aqrabiyah, Prince Turki Street', N'Khobar', N'DANAH MOHMMAD', '**** **** **** 7754', 'Mastercard', '03/29', 'Active', '2026-08-28 14:00:00'),
(5, N'Rawa', N'Alhrbi', N'rawa.alhrbi@outlook.com', '$2y$10$J4eyO6nIhvbH0RhH3Qs5WOs1sy3fXsSWH5q64A71RrD7OyaRQ6m4G', '966548120376', N'Al Nakheel District, Street 33', N'Riyadh', N'RAWA ALHRBI', '**** **** **** 3086', 'Mada', '06/28', 'Active', '2026-09-01 16:20:00'),
(6, N'Elaf', N'Alzahrani', N'elaf.alzahrani@gmail.com', '$2y$10$J4eyO6nIhvbH0RhH3Qs5WOs1sy3fXsSWH5q64A71RrD7OyaRQ6m4G', '966536674519', N'Al Narjis District, Othman bin Affan Rd', N'Riyadh', NULL, NULL, NULL, NULL, 'Active', '2026-09-03 12:45:00'),
(7, N'Nora', N'Al-Qahtani', N'n.alqahtani@email.com', '$2y$10$J4eyO6nIhvbH0RhH3Qs5WOs1sy3fXsSWH5q64A71RrD7OyaRQ6m4G', '966554128830', N'King Fahd Road, Al Olaya, Building 42', N'Riyadh', N'NORA AL-QAHTANI', '**** **** **** 5543', 'Mada', '08/29', 'Active', '2026-03-14 18:00:00'),
(8, N'Faisal', N'Al-Harbi', N'faisal.alharbi@gmail.com', '$2y$10$J4eyO6nIhvbH0RhH3Qs5WOs1sy3fXsSWH5q64A71RrD7OyaRQ6m4G', '966561127730', N'Al Rawdah District, Prince Sultan Road', N'Jeddah', NULL, NULL, NULL, NULL, 'Active', '2026-05-02 08:40:00'),
(9, N'Maha', N'Al-Dosari', N'maha.aldosari@glowcare.sa', '$2y$10$J4eyO6nIhvbH0RhH3Qs5WOs1sy3fXsSWH5q64A71RrD7OyaRQ6m4G', '966590045218', N'Al Shati District, Gulf Road, Unit 3', N'Dammam', N'MAHA AL-DOSARI', '**** **** **** 9902', 'Visa', '01/28', 'Active', '2026-04-11 10:10:00'),
(10, N'Abdullah', N'Al-Ghamdi', N'abdullah.ghamdi@yahoo.com', '$2y$10$J4eyO6nIhvbH0RhH3Qs5WOs1sy3fXsSWH5q64A71RrD7OyaRQ6m4G', '966525586314', N'Al Rabwah District, Street 7', N'Riyadh', NULL, NULL, NULL, NULL, 'Suspended', '2026-06-19 13:05:00');
SET IDENTITY_INSERT customers OFF;
GO

/* staff (11) */
SET IDENTITY_INSERT staff ON;
INSERT INTO staff (staff_id, admin_id, first_name, last_name, email, password, phone, job_title, license_number, hire_date, staff_status, specialty, shift_start, shift_end) VALUES
(1, 2, N'Khalid', N'Al-Mutairi', N'k.almutairi@trucklink.sa', '$2y$10$J4eyO6nIhvbH0RhH3Qs5WOs1sy3fXsSWH5q64A71RrD7OyaRQ6m4G', '966553281940', 'Driver', '2843-HV', '2025-11-02', 'Active', NULL, '07:00', '17:00'),
(2, 2, N'Ibrahim', N'Al-Shammari', N'i.alshammari@trucklink.sa', '$2y$10$J4eyO6nIhvbH0RhH3Qs5WOs1sy3fXsSWH5q64A71RrD7OyaRQ6m4G', '966567720184', 'Driver', '3910-HV', '2025-11-15', 'Active', NULL, '08:00', '18:00'),
(3, 2, N'Yasser', N'Al-Ghamdi', N'y.alghamdi@trucklink.sa', '$2y$10$J4eyO6nIhvbH0RhH3Qs5WOs1sy3fXsSWH5q64A71RrD7OyaRQ6m4G', '966502298477', 'Driver', '1177-HV', '2026-01-08', 'On Leave', NULL, '06:00', '16:00'),
(4, 2, N'Nasser', N'Al-Qahtani', N'n.alqahtani@trucklink.sa', '$2y$10$J4eyO6nIhvbH0RhH3Qs5WOs1sy3fXsSWH5q64A71RrD7OyaRQ6m4G', '966541650923', 'Driver', '4502-HV', '2026-01-20', 'Active', NULL, '07:00', '17:00'),
(5, 2, N'Saeed', N'Al-Zahrani', N's.alzahrani@trucklink.sa', '$2y$10$J4eyO6nIhvbH0RhH3Qs5WOs1sy3fXsSWH5q64A71RrD7OyaRQ6m4G', '966558870312', 'Mover', NULL, '2025-11-02', 'Active', N'assembly', '07:00', '17:00'),
(6, 2, N'Mansour', N'Al-Shehri', N'm.alshehri@trucklink.sa', '$2y$10$J4eyO6nIhvbH0RhH3Qs5WOs1sy3fXsSWH5q64A71RrD7OyaRQ6m4G', '966534419078', 'Mover', NULL, '2025-12-01', 'Active', N'packing', '07:00', '17:00'),
(7, 2, N'Turki', N'Al-Anzi', N't.alanzi@trucklink.sa', '$2y$10$J4eyO6nIhvbH0RhH3Qs5WOs1sy3fXsSWH5q64A71RrD7OyaRQ6m4G', '966507763215', 'Mover', NULL, '2026-02-14', 'Active', NULL, '07:00', '17:00'),
(8, 3, N'Hamad', N'Al-Dawsari', N'h.aldawsari@trucklink.sa', '$2y$10$J4eyO6nIhvbH0RhH3Qs5WOs1sy3fXsSWH5q64A71RrD7OyaRQ6m4G', '966593301847', 'Mover', NULL, '2026-03-01', 'Active', N'assembly', '08:00', '18:00'),
(9, 3, N'Fahad', N'Al-Juhani', N'f.aljuhani@trucklink.sa', '$2y$10$J4eyO6nIhvbH0RhH3Qs5WOs1sy3fXsSWH5q64A71RrD7OyaRQ6m4G', '966562204519', 'Mover', NULL, '2026-03-10', 'Active', N'loading', '08:00', '18:00'),
(10, 2, N'Bandar', N'Al-Subaie', N'b.alsubaie@trucklink.sa', '$2y$10$J4eyO6nIhvbH0RhH3Qs5WOs1sy3fXsSWH5q64A71RrD7OyaRQ6m4G', '966545580631', 'Warehouse Keeper', NULL, '2025-12-10', 'Active', NULL, '08:00', '20:00'),
(11, 3, N'Ziyad', N'Al-Malki', N'z.almalki@trucklink.sa', '$2y$10$J4eyO6nIhvbH0RhH3Qs5WOs1sy3fXsSWH5q64A71RrD7OyaRQ6m4G', '966508842206', 'Warehouse Keeper', NULL, '2026-04-05', 'Active', NULL, '08:00', '18:00');
SET IDENTITY_INSERT staff OFF;
GO

/* vehicles (10) */
SET IDENTITY_INSERT vehicles ON;
INSERT INTO vehicles (vehicle_id, admin_id, plate_number, vehicle_type, capacity_volume_m3, max_weight_kg, is_temperature_controlled, vehicle_status, created_at, updated_at) VALUES
(31, 2, '7409 PKP', 'Small Pickup', 3.50, 1200.00, 0, 'Available', '2026-02-02 10:00:00', '2026-02-02 10:00:00'),
(32, 2, '2287 PKP', 'Small Pickup', 3.50, 1200.00, 0, 'Available', '2026-06-19 10:00:00', '2026-06-19 10:00:00'),
(33, 2, '3390 CLD', 'Temp-Controlled', 9.60, 2800.00, 1, 'Maintenance', '2026-04-27 10:00:00', '2026-10-05 09:00:00'),
(34, 1, '9014 CLD', 'Temp-Controlled', 9.60, 2800.00, 1, 'Out of Service', '2026-07-30 10:00:00', '2026-09-29 15:00:00'),
(35, 2, '6633 TRK', 'Medium Truck', 14.00, 3500.00, 0, 'On Job', '2026-05-05 10:00:00', '2026-10-11 15:40:00'),
(36, 2, '7712 TRK', 'Medium Truck', 14.00, 3500.00, 0, 'Available', '2026-01-28 10:00:00', '2026-08-28 10:00:00'),
(37, 1, '1156 FRT', 'Large Freight', 34.00, 12000.00, 0, 'Available', '2026-01-11 10:00:00', '2026-01-11 10:00:00'),
(38, 2, '5520 CLD', 'Temp-Controlled', 9.60, 2800.00, 1, 'Available', '2026-03-02 10:00:00', '2026-03-02 10:00:00'),
(39, 2, '4821 TRK', 'Medium Truck', 14.00, 3500.00, 0, 'Available', '2026-03-18 10:00:00', '2026-08-12 11:00:00'),
(40, 3, '8845 FRT', 'Large Freight', 34.00, 12000.00, 0, 'Available', '2026-05-21 10:00:00', '2026-05-21 10:00:00');
SET IDENTITY_INSERT vehicles OFF;
GO

/* warehouses (2) */
SET IDENTITY_INSERT warehouses ON;
INSERT INTO warehouses (warehouse_id, keeper_id, warehouse_name, address, city, total_capacity_m3, used_capacity_m3, opening_hours) VALUES
(1, 10, N'Warehouse Riyadh North', N'Al Qirawan District, Northern Ring Road', N'Riyadh', 1200.00, 312.22, N'Sun–Thu 08:00–20:00'),
(2, 11, N'Warehouse Jeddah South', N'Al Khumrah Industrial Area, Street 41', N'Jeddah', 800.00, 141.20, N'Sat–Thu 08:00–18:00');
SET IDENTITY_INSERT warehouses OFF;
GO

/* item_categories (5) */
SET IDENTITY_INSERT item_categories ON;
INSERT INTO item_categories (category_id, admin_id, category_name, description, requires_temperature_control, recommended_vehicle_type) VALUES
(1, 1, N'Large Home & Office Furniture', N'Sofas, beds, wardrobes, tables, desks', 0, 'Medium Truck'),
(2, 1, N'Electrical Appliances & Electronics', N'Refrigerators, washing machines, screens/TVs, air conditioners', 0, 'Medium Truck'),
(3, 1, N'Temperature-Sensitive Products', N'Skincare and care products, makeup, perfumes, medicine, sweets, chocolate', 1, 'Temp-Controlled'),
(4, 1, N'Personal Items & Goods', N'Boxes, travel bags, student housing supplies, single-item moves', 0, 'Small Pickup'),
(5, 1, N'Heavy / Special Items', N'Sports equipment, musical instruments, steel cabinets/safes', 0, 'Large Freight');
SET IDENTITY_INSERT item_categories OFF;
GO

/* services (11) */
SET IDENTITY_INSERT services ON;
INSERT INTO services (service_id, admin_id, service_name, service_type, description, base_price, pricing_unit) VALUES
(1, 1, N'Single Move', 'Transport', N'One pickup and one drop-off location.', 150.00, 'per job'),
(2, 1, N'Multi-Stop Move', 'Transport', N'Several pickup or drop-off points in one trip.', 280.00, 'per job'),
(3, 1, N'Temperature-Controlled', 'Transport', N'For creams, medicine, perfumes and chocolate.', 450.00, 'per job'),
(4, 1, N'Full Home / Office Move', 'Transport', N'Complete household or office relocation.', 700.00, 'per job'),
(5, 1, N'Premium Storage', 'Storage', N'Our own warehouse holds your furniture. First week free.', 0.00, 'first 7 days'),
(6, 1, N'Storage Daily Extension', 'Storage', N'For short additional storage needs after the free week.', 30.00, 'per day'),
(7, 1, N'Storage Weekly Extension', 'Storage', N'For medium-length storage needs after the free week.', 180.00, 'per week'),
(8, 1, N'Storage Monthly Extension', 'Storage', N'For longer storage needs after the free week.', 600.00, 'per month'),
(9, 1, N'Assembly & Disassembly', 'Value-Added', N'Crew disassembles and reassembles your furniture.', 120.00, 'per job'),
(10, 1, N'Packing & Protection', 'Value-Added', N'Boxes, bubble wrap and packing tape.', 90.00, 'per item'),
(11, 1, N'Extra Loading Labor', 'Value-Added', N'Additional workers for loading and unloading.', 90.00, 'per worker');
SET IDENTITY_INSERT services OFF;
GO

/* bookings (12) */
SET IDENTITY_INSERT bookings ON;
INSERT INTO bookings (booking_id, customer_id, staff_id, vehicle_id, booking_status, pickup_address, pickup_city, dropoff_address, dropoff_city, pickup_datetime, total_volume_m3, recommended_vehicle_type, transport_fee, total_amount, expected_delivery_datetime, created_at, customer_notes, rejection_reason, distance_km) VALUES
(4181, 1, 4, 36, 'Completed', N'Al Fanateer District, Street 12, Villa 4', N'Jubail', N'Al Faisaliyah, Street 28, Building 6', N'Dammam', '2026-09-05 08:00:00', 4.58, 'Medium Truck', 350.00, 470.00, '2026-09-05 12:30:00', '2026-09-01 19:20:00', N'Ground floor, the building guard will open the gate.', NULL, 95.00),
(4182, 2, 4, 38, 'Completed', N'Al Faisaliyah, King Saud Street, Bldg 9', N'Dammam', N'Al Ulaya District, Dhahran Street, Shop 14', N'Khobar', '2026-09-12 10:00:00', 0.88, 'Temp-Controlled', 450.00, 630.00, '2026-09-12 11:30:00', '2026-09-09 13:00:00', N'Keep the cartons below 25°C at all times.', NULL, 22.00),
(4183, 3, 2, 37, 'Completed', N'Al Malqa District, Anas bin Malik Road', N'Riyadh', N'Al Arid District, Street 9, Villa 21', N'Riyadh', '2026-09-20 07:00:00', 14.21, 'Large Freight', 700.00, 970.00, '2026-10-02 10:00:00', '2026-09-15 21:10:00', N'Villa not ready until 2 October — please store everything.', NULL, 31.00),
(4184, 4, 4, 31, 'Completed', N'Al Aqrabiyah, Prince Turki Street', N'Khobar', N'Al Mazruiyah District, Street 3', N'Dammam', '2026-09-27 09:00:00', 0.88, 'Small Pickup', 150.00, 240.00, '2026-09-27 10:15:00', '2026-09-25 10:05:00', NULL, NULL, 18.00),
(4185, 5, 1, 39, 'Completed', N'Al Nakheel District, Street 33', N'Riyadh', N'Al Sahafa District, Street 14, Apt 8', N'Riyadh', '2026-10-03 08:30:00', 4.85, 'Medium Truck', 350.00, 620.00, '2026-10-08 12:00:00', '2026-09-29 17:40:00', N'Two stops: pick up the bookshelves from my sister in Al Yasmin first.', NULL, 27.00),
(4186, 8, NULL, NULL, 'Cancelled', N'Al Rawdah District, Prince Sultan Road', N'Jeddah', N'Al Hamra District, Street 5', N'Jeddah', '2026-10-06 14:30:00', 0.58, 'Small Pickup', 150.00, 150.00, NULL, '2026-10-02 09:30:00', NULL, NULL, 12.00),
(4187, 7, 1, 39, 'In Storage', N'King Fahd Road, Al Olaya, Building 42', N'Riyadh', N'Al Yasmin District, Street 18, Villa 7', N'Riyadh', '2026-10-07 09:00:00', 6.42, 'Medium Truck', 350.00, 800.00, '2026-10-19 14:00:00', '2026-10-05 20:14:00', N'Second floor, no lift. Please bring furniture blankets for the dining table.', NULL, 23.00),
(4188, 8, NULL, NULL, 'Pending', N'Al Rawdah District, Prince Sultan Road', N'Jeddah', N'Obhur Al Shamaliyah, Street 22', N'Jeddah', '2026-10-13 14:30:00', 2.09, 'Small Pickup', 150.00, 420.00, '2026-10-13 16:00:00', '2026-10-11 09:40:00', N'Please call 15 minutes before arrival.', NULL, 26.00),
(4189, 9, 4, 38, 'Accepted', N'Al Shati District, Gulf Road, Unit 3', N'Dammam', N'Al Khobar Al Shamaliyah, Pharmacy Plaza', N'Khobar', '2026-10-14 08:00:00', 1.16, 'Temp-Controlled', 450.00, 810.00, '2026-10-14 09:30:00', '2026-10-09 15:25:00', N'Business delivery — receiving desk opens at 08:30.', NULL, 24.00),
(4190, 6, NULL, NULL, 'Pending', N'Al Narjis District, Othman bin Affan Rd', N'Riyadh', N'Al Iskan District, Street 11, Villa 3', N'Buraidah', '2026-10-15 07:00:00', 13.63, 'Large Freight', 700.00, 1270.00, '2026-10-15 13:00:00', '2026-10-10 22:05:00', N'Moving to my family home in Qassim.', NULL, 335.00),
(4191, 3, NULL, NULL, 'Rejected', N'Al Malqa District, Anas bin Malik Road', N'Riyadh', N'Al Wurud District, Street 2', N'Riyadh', '2026-10-09 10:00:00', 0.30, 'Small Pickup', 150.00, 150.00, NULL, '2026-10-08 08:15:00', NULL, N'No suitable vehicle available on the requested date', 15.00),
(4192, 1, 4, 35, 'In Transit', N'Al Fanateer District, Street 12, Villa 4', N'Jubail', N'Jubail Industrial City, Al Deffi District, Street 4', N'Jubail', '2026-10-11 14:00:00', 2.61, 'Medium Truck', 350.00, 440.00, '2026-10-11 16:30:00', '2026-10-08 18:30:00', N'Gate code 4471.', NULL, 14.00);
SET IDENTITY_INSERT bookings OFF;
GO

/* booking_services (30) */
SET IDENTITY_INSERT booking_services ON;
INSERT INTO booking_services (booking_service_id, booking_id, service_id, quantity, subtotal) VALUES
(1, 4181, 1, 1, 350.00),
(2, 4181, 9, 1, 120.00),
(3, 4182, 3, 1, 450.00),
(4, 4182, 10, 2, 180.00),
(5, 4183, 4, 1, 700.00),
(6, 4183, 9, 1, 120.00),
(7, 4183, 5, 1, 0.00),
(8, 4183, 6, 5, 150.00),
(9, 4184, 1, 1, 150.00),
(10, 4184, 11, 1, 90.00),
(11, 4185, 2, 1, 350.00),
(12, 4185, 10, 3, 270.00),
(13, 4185, 5, 1, 0.00),
(14, 4186, 1, 1, 150.00),
(15, 4187, 1, 1, 350.00),
(16, 4187, 9, 1, 120.00),
(17, 4187, 10, 2, 180.00),
(18, 4187, 5, 1, 0.00),
(19, 4187, 6, 5, 150.00),
(20, 4188, 1, 1, 150.00),
(21, 4188, 10, 3, 270.00),
(22, 4189, 3, 1, 450.00),
(23, 4189, 10, 4, 360.00),
(24, 4190, 4, 1, 700.00),
(25, 4190, 9, 1, 120.00),
(26, 4190, 10, 5, 450.00),
(27, 4190, 5, 1, 0.00),
(28, 4191, 1, 1, 150.00),
(29, 4192, 1, 1, 350.00),
(30, 4192, 10, 1, 90.00);
SET IDENTITY_INSERT booking_services OFF;
GO

/* items (33) */
SET IDENTITY_INSERT items ON;
INSERT INTO items (item_id, booking_id, category_id, item_name, quantity, estimated_length_cm, estimated_width_cm, estimated_height_cm, estimated_volume_m3, estimated_weight_kg, ai_confidence_score, measured_at) VALUES
(1, 4181, 1, N'Three-seater sofa', 1, 210.00, 95.00, 85.00, 1.70, 68.00, 92.00, '2026-09-01 19:25:00'),
(2, 4181, 1, N'Wardrobe (3-door)', 1, 180.00, 60.00, 200.00, 2.16, 95.00, 88.00, '2026-09-01 19:25:00'),
(3, 4181, 4, N'Moving boxes', 10, 50.00, 40.00, 36.00, 0.72, 70.00, 84.00, '2026-09-01 19:25:00'),
(4, 4182, 3, N'Skincare stock cartons', 14, 40.00, 30.00, 30.00, 0.50, 140.00, 90.00, '2026-09-09 13:04:00'),
(5, 4182, 3, N'Perfume display crates', 4, 60.00, 40.00, 40.00, 0.38, 48.00, 86.00, '2026-09-09 13:04:00'),
(6, 4183, 1, N'King bed with mattress', 1, 210.00, 190.00, 60.00, 2.39, 110.00, 91.00, '2026-09-15 21:16:00'),
(7, 4183, 1, N'Dining table + 8 chairs', 1, 240.00, 110.00, 78.00, 4.10, 160.00, 85.00, '2026-09-15 21:16:00'),
(8, 4183, 2, N'Washing machine', 1, 60.00, 60.00, 85.00, 0.31, 72.00, 94.00, '2026-09-15 21:16:00'),
(9, 4183, 1, N'L-shaped sofa', 1, 300.00, 220.00, 85.00, 5.61, 140.00, 89.00, '2026-09-15 21:16:00'),
(10, 4183, 4, N'Moving boxes', 25, 50.00, 40.00, 36.00, 1.80, 175.00, 83.00, '2026-09-15 21:16:00'),
(11, 4184, 4, N'Student desk', 1, 120.00, 60.00, 75.00, 0.54, 25.00, 93.00, '2026-09-25 10:07:00'),
(12, 4184, 4, N'Travel bags', 3, 75.00, 50.00, 30.00, 0.34, 36.00, 90.00, '2026-09-25 10:07:00'),
(13, 4185, 1, N'Three-seater sofa', 1, 200.00, 90.00, 85.00, 1.53, 62.00, 91.00, '2026-09-29 17:47:00'),
(14, 4185, 2, N'Refrigerator', 1, 80.00, 70.00, 175.00, 0.98, 95.00, 95.00, '2026-09-29 17:47:00'),
(15, 4185, 1, N'Bookshelf', 2, 90.00, 35.00, 200.00, 1.26, 80.00, 87.00, '2026-09-29 17:47:00'),
(16, 4185, 4, N'Moving boxes', 15, 50.00, 40.00, 36.00, 1.08, 105.00, 82.00, '2026-09-29 17:47:00'),
(17, 4186, 4, N'Moving boxes', 8, 50.00, 40.00, 36.00, 0.58, 56.00, 85.00, '2026-10-02 09:31:00'),
(18, 4187, 1, N'Three-seater sofa', 1, 210.00, 95.00, 85.00, 1.70, 68.00, 92.00, '2026-10-05 20:20:00'),
(19, 4187, 2, N'Double-door refrigerator', 1, 91.00, 74.00, 179.00, 1.21, 112.00, 95.00, '2026-10-05 20:20:00'),
(20, 4187, 1, N'Dining table + 6 chairs', 1, 180.00, 100.00, 78.00, 2.63, 95.00, 87.00, '2026-10-05 20:20:00'),
(21, 4187, 4, N'Moving boxes', 12, 50.00, 40.00, 36.00, 0.88, 84.00, 81.00, '2026-10-05 20:20:00'),
(22, 4188, 4, N'Moving boxes', 14, 50.00, 40.00, 36.00, 1.01, 98.00, 86.00, '2026-10-11 09:42:00'),
(23, 4188, 4, N'Office chair', 2, 70.00, 70.00, 110.00, 1.08, 30.00, 92.00, '2026-10-11 09:42:00'),
(24, 4189, 3, N'Skincare creams (cartons)', 20, 40.00, 30.00, 30.00, 0.72, 180.00, 90.00, '2026-10-09 15:28:00'),
(25, 4189, 3, N'Chocolate gift boxes', 10, 50.00, 35.00, 25.00, 0.44, 60.00, 88.00, '2026-10-09 15:28:00'),
(26, 4190, 1, N'Bedroom set (bed + 2 nightstands)', 1, 220.00, 200.00, 110.00, 4.84, 180.00, 88.00, '2026-10-10 22:12:00'),
(27, 4190, 1, N'Majlis sofa set', 1, 320.00, 240.00, 80.00, 6.14, 150.00, 86.00, '2026-10-10 22:12:00'),
(28, 4190, 2, N'Split air conditioner units', 3, 90.00, 30.00, 60.00, 0.49, 90.00, 93.00, '2026-10-10 22:12:00'),
(29, 4190, 4, N'Moving boxes', 30, 50.00, 40.00, 36.00, 2.16, 210.00, 84.00, '2026-10-10 22:12:00'),
(30, 4191, 5, N'Steel safe', 1, 60.00, 55.00, 90.00, 0.30, 210.00, 79.00, '2026-10-08 08:17:00'),
(31, 4192, 1, N'Wardrobe (2-door)', 1, 120.00, 60.00, 200.00, 1.44, 70.00, 90.00, '2026-10-08 18:34:00'),
(32, 4192, 2, N'Washing machine', 1, 60.00, 60.00, 85.00, 0.31, 72.00, 94.00, '2026-10-08 18:34:00'),
(33, 4192, 4, N'Moving boxes', 12, 50.00, 40.00, 36.00, 0.86, 84.00, 85.00, '2026-10-08 18:34:00');
SET IDENTITY_INSERT items OFF;
GO

/* item_photos (52) */
SET IDENTITY_INSERT item_photos ON;
INSERT INTO item_photos (item_photo_id, item_id, photo_url, uploaded_at) VALUES
(1, 1, N'/storage/item_photos/4181/item1_1.jpg', '2026-09-01 19:22:00'),
(2, 1, N'/storage/item_photos/4181/item1_2.jpg', '2026-09-01 19:22:00'),
(3, 1, N'/storage/item_photos/4181/item1_3.jpg', '2026-09-01 19:22:00'),
(4, 2, N'/storage/item_photos/4181/item2_1.jpg', '2026-09-01 19:23:00'),
(5, 2, N'/storage/item_photos/4181/item2_2.jpg', '2026-09-01 19:23:00'),
(6, 3, N'/storage/item_photos/4181/item3_1.jpg', '2026-09-01 19:24:00'),
(7, 4, N'/storage/item_photos/4182/item4_1.jpg', '2026-09-09 13:02:00'),
(8, 4, N'/storage/item_photos/4182/item4_2.jpg', '2026-09-09 13:02:00'),
(9, 5, N'/storage/item_photos/4182/item5_1.jpg', '2026-09-09 13:03:00'),
(10, 6, N'/storage/item_photos/4183/item6_1.jpg', '2026-09-15 21:12:00'),
(11, 6, N'/storage/item_photos/4183/item6_2.jpg', '2026-09-15 21:12:00'),
(12, 7, N'/storage/item_photos/4183/item7_1.jpg', '2026-09-15 21:13:00'),
(13, 7, N'/storage/item_photos/4183/item7_2.jpg', '2026-09-15 21:13:00'),
(14, 7, N'/storage/item_photos/4183/item7_3.jpg', '2026-09-15 21:13:00'),
(15, 8, N'/storage/item_photos/4183/item8_1.jpg', '2026-09-15 21:14:00'),
(16, 9, N'/storage/item_photos/4183/item9_1.jpg', '2026-09-15 21:15:00'),
(17, 9, N'/storage/item_photos/4183/item9_2.jpg', '2026-09-15 21:15:00'),
(18, 10, N'/storage/item_photos/4183/item10_1.jpg', '2026-09-15 21:16:00'),
(19, 11, N'/storage/item_photos/4184/item11_1.jpg', '2026-09-25 10:07:00'),
(20, 12, N'/storage/item_photos/4184/item12_1.jpg', '2026-09-25 10:08:00'),
(21, 13, N'/storage/item_photos/4185/item13_1.jpg', '2026-09-29 17:42:00'),
(22, 13, N'/storage/item_photos/4185/item13_2.jpg', '2026-09-29 17:42:00'),
(23, 14, N'/storage/item_photos/4185/item14_1.jpg', '2026-09-29 17:43:00'),
(24, 14, N'/storage/item_photos/4185/item14_2.jpg', '2026-09-29 17:43:00'),
(25, 15, N'/storage/item_photos/4185/item15_1.jpg', '2026-09-29 17:44:00'),
(26, 16, N'/storage/item_photos/4185/item16_1.jpg', '2026-09-29 17:45:00'),
(27, 17, N'/storage/item_photos/4186/item17_1.jpg', '2026-10-02 09:32:00'),
(28, 18, N'/storage/item_photos/4187/item18_1.jpg', '2026-10-05 20:16:00'),
(29, 18, N'/storage/item_photos/4187/item18_2.jpg', '2026-10-05 20:16:00'),
(30, 18, N'/storage/item_photos/4187/item18_3.jpg', '2026-10-05 20:16:00'),
(31, 19, N'/storage/item_photos/4187/item19_1.jpg', '2026-10-05 20:17:00'),
(32, 19, N'/storage/item_photos/4187/item19_2.jpg', '2026-10-05 20:17:00'),
(33, 20, N'/storage/item_photos/4187/item20_1.jpg', '2026-10-05 20:18:00'),
(34, 20, N'/storage/item_photos/4187/item20_2.jpg', '2026-10-05 20:18:00'),
(35, 20, N'/storage/item_photos/4187/item20_3.jpg', '2026-10-05 20:18:00'),
(36, 21, N'/storage/item_photos/4187/item21_1.jpg', '2026-10-05 20:19:00'),
(37, 22, N'/storage/item_photos/4188/item22_1.jpg', '2026-10-11 09:42:00'),
(38, 23, N'/storage/item_photos/4188/item23_1.jpg', '2026-10-11 09:43:00'),
(39, 24, N'/storage/item_photos/4189/item24_1.jpg', '2026-10-09 15:27:00'),
(40, 24, N'/storage/item_photos/4189/item24_2.jpg', '2026-10-09 15:27:00'),
(41, 25, N'/storage/item_photos/4189/item25_1.jpg', '2026-10-09 15:28:00'),
(42, 26, N'/storage/item_photos/4190/item26_1.jpg', '2026-10-10 22:07:00'),
(43, 26, N'/storage/item_photos/4190/item26_2.jpg', '2026-10-10 22:07:00'),
(44, 27, N'/storage/item_photos/4190/item27_1.jpg', '2026-10-10 22:08:00'),
(45, 27, N'/storage/item_photos/4190/item27_2.jpg', '2026-10-10 22:08:00'),
(46, 28, N'/storage/item_photos/4190/item28_1.jpg', '2026-10-10 22:09:00'),
(47, 29, N'/storage/item_photos/4190/item29_1.jpg', '2026-10-10 22:10:00'),
(48, 30, N'/storage/item_photos/4191/item30_1.jpg', '2026-10-08 08:17:00'),
(49, 30, N'/storage/item_photos/4191/item30_2.jpg', '2026-10-08 08:17:00'),
(50, 31, N'/storage/item_photos/4192/item31_1.jpg', '2026-10-08 18:32:00'),
(51, 32, N'/storage/item_photos/4192/item32_1.jpg', '2026-10-08 18:33:00'),
(52, 33, N'/storage/item_photos/4192/item33_1.jpg', '2026-10-08 18:34:00');
SET IDENTITY_INSERT item_photos OFF;
GO

/* storage_requests (4) */
SET IDENTITY_INSERT storage_requests ON;
INSERT INTO storage_requests (storage_id, booking_id, warehouse_id, storage_location, start_date, free_period_end_date, end_date, extension_type, extension_units, storage_fee, storage_status) VALUES
(1, 4183, 1, 'RN-A-07', '2026-09-20', '2026-09-27', '2026-10-02', 'Daily', 5, 150.00, 'Released'),
(2, 4185, 1, 'RN-C-02', '2026-10-03', '2026-10-10', '2026-10-08', NULL, NULL, 0.00, 'Released'),
(3, 4187, 1, 'RN-B-14', '2026-10-07', '2026-10-14', '2026-10-19', 'Daily', 5, 150.00, 'Free Period'),
(4, 4190, 1, 'RN-D-03', '2026-10-15', '2026-10-22', NULL, NULL, NULL, 0.00, 'Reserved');
SET IDENTITY_INSERT storage_requests OFF;
GO

/* payments (18) */
SET IDENTITY_INSERT payments ON;
INSERT INTO payments (payment_id, booking_id, invoice_number, ref_number, amount, payment_method, payment_status, transaction_date) VALUES
(1, 4181, 'INV-2026-00001', 'PG-4181-0001', 350.00, 'Apple Pay', 'Paid', '2026-09-05 09:00:00'),
(2, 4181, 'INV-2026-00002', 'PG-4181-0002', 120.00, 'Credit Card', 'Paid', '2026-09-05 09:00:00'),
(3, 4182, 'INV-2026-00003', 'PG-4182-0003', 450.00, 'STC Pay', 'Paid', '2026-09-12 11:00:00'),
(4, 4182, 'INV-2026-00004', 'PG-4182-0004', 180.00, 'Mada', 'Paid', '2026-09-12 11:00:00'),
(5, 4183, 'INV-2026-00005', 'PG-4183-0005', 700.00, 'Apple Pay', 'Paid', '2026-09-20 08:00:00'),
(6, 4183, 'INV-2026-00006', 'PG-4183-0006', 120.00, 'Credit Card', 'Paid', '2026-09-20 08:00:00'),
(7, 4183, 'INV-2026-00007', 'PG-4183-0007', 150.00, 'STC Pay', 'Paid', '2026-09-20 08:00:00'),
(8, 4184, 'INV-2026-00008', 'PG-4184-0008', 150.00, 'Mada', 'Paid', '2026-09-27 10:00:00'),
(9, 4184, 'INV-2026-00009', 'PG-4184-0009', 90.00, 'Apple Pay', 'Paid', '2026-09-27 10:00:00'),
(10, 4185, 'INV-2026-00010', 'PG-4185-0010', 350.00, 'Credit Card', 'Paid', '2026-10-03 09:30:00'),
(11, 4185, 'INV-2026-00011', 'PG-4185-0011', 270.00, 'STC Pay', 'Paid', '2026-10-03 09:30:00'),
(12, 4187, 'INV-2026-00012', 'PG-4187-0012', 350.00, 'Mada', 'Paid', '2026-10-07 10:00:00'),
(13, 4187, 'INV-2026-00013', 'PG-4187-0013', 120.00, 'Apple Pay', 'Paid', '2026-10-07 10:00:00'),
(14, 4187, 'INV-2026-00014', 'PG-4187-0014', 180.00, 'Credit Card', 'Paid', '2026-10-07 10:00:00'),
(15, 4187, 'INV-2026-00015', NULL, 150.00, 'STC Pay', 'Payment Pending', '2026-10-07 10:00:00'),
(16, 4192, 'INV-2026-00016', 'PG-4192-0016', 350.00, 'Mada', 'Paid', '2026-10-11 15:00:00'),
(17, 4192, 'INV-2026-00017', 'PG-4192-0017', 90.00, 'Apple Pay', 'Paid', '2026-10-11 15:00:00'),
(18, 4192, 'INV-2026-00018', NULL, 350.00, 'Credit Card', 'Failed', '2026-10-10 18:00:00');
SET IDENTITY_INSERT payments OFF;
GO

/* reviews (5) */
SET IDENTITY_INSERT reviews ON;
INSERT INTO reviews (review_id, booking_id, customer_id, admin_id, rating, comment, created_at) VALUES
(1, 4181, 1, NULL, 5, N'They measured everything from my photos and the truck was exactly the right size. Nothing scratched.', '2026-09-06 10:00:00'),
(2, 4182, 2, NULL, 5, N'The refrigerated van kept my skincare stock cool all the way to Khobar. Very professional driver.', '2026-09-13 09:30:00'),
(3, 4183, 3, 2, 4, N'Storage saved our move because the villa was late. Delivery from the warehouse started an hour late.', '2026-10-03 18:45:00'),
(4, 4184, 4, NULL, 5, N'Quick and cheap for a student move. The extra worker was worth it.', '2026-09-28 12:10:00'),
(5, 4185, 5, NULL, 4, N'Two stops handled smoothly. Packing materials were good, one box arrived slightly crushed.', '2026-10-09 08:20:00');
SET IDENTITY_INSERT reviews OFF;
GO

/* messages (10) */
SET IDENTITY_INSERT messages ON;
INSERT INTO messages (message_id, booking_id, customer_id, staff_id, sender_type, message_text, sent_at, is_read) VALUES
(1, 4187, 7, 1, 'Customer', N'Hi Khalid, the building has no lift — will the crew bring blankets for the dining table?', '2026-10-06 18:02:00', 1),
(2, 4187, 7, 1, 'Staff', N'Yes, we have blankets and straps on board. We will arrive at 09:00 sharp.', '2026-10-06 18:10:00', 1),
(3, 4187, 7, 1, 'Customer', N'Great. Everything goes to storage, my new villa is ready on the 19th.', '2026-10-06 18:12:00', 1),
(4, 4187, 7, 1, 'Staff', N'Noted — items are now in Warehouse Riyadh North, bay RN-B-14.', '2026-10-07 13:05:00', 0),
(5, 4192, 1, 4, 'Staff', N'We are on the way to the drop-off, around 20 minutes.', '2026-10-11 15:40:00', 1),
(6, 4192, 1, 4, 'Customer', N'Thank you. The gate code is 4471.', '2026-10-11 15:43:00', 0),
(7, 4189, 9, 4, 'Customer', N'Can you confirm the van will stay below 25°C for the chocolate boxes?', '2026-10-10 11:00:00', 1),
(8, 4189, 9, 4, 'Staff', N'Confirmed, the unit is set to 18°C and logged during the trip.', '2026-10-10 11:25:00', 0),
(9, 4183, 3, 2, 'Customer', N'Please deliver from storage on 2 October in the morning.', '2026-09-29 09:00:00', 1),
(10, 4183, 3, 2, 'Staff', N'Booked for 2 October at 09:00. See you then.', '2026-09-29 09:30:00', 1);
SET IDENTITY_INSERT messages OFF;
GO

/* announcements (10) */
SET IDENTITY_INSERT announcements ON;
INSERT INTO announcements (announcement_id, admin_id, customer_id, staff_id, announcement_type, title, content, sent_at, is_read) VALUES
(1, NULL, 7, NULL, 'Booking Accepted', N'Your booking is confirmed', N'BK-2026-004187 is accepted. Medium Truck 4821 TRK and a 3-person crew are reserved for 07 Oct at 09:00.', '2026-10-06 08:12:00', 1),
(2, NULL, NULL, 1, 'Job Assigned', N'New job assigned', N'BK-2026-004187 · 07 Oct, 09:00 · King Fahd Road, Al Olaya → Al Yasmin District.', '2026-10-06 08:12:00', 1),
(3, NULL, 7, NULL, 'Storage Reminder', N'Free storage ends soon', N'Your free storage period ends on 14 Oct. Extra days are charged at 30 SAR a day.', '2026-10-11 09:00:00', 0),
(4, NULL, 3, NULL, 'Booking Rejected', N'Booking request rejected', N'BK-2026-004191: No suitable vehicle available on the requested date. Please choose 12 or 13 October.', '2026-10-08 11:40:00', 1),
(5, NULL, 1, NULL, 'Status Update', N'Your move is in transit', N'BK-2026-004192 is on the way to the drop-off address.', '2026-10-11 15:35:00', 0),
(6, NULL, 9, NULL, 'Booking Accepted', N'Your booking is confirmed', N'BK-2026-004189 is accepted for 14 Oct at 08:00 with a temperature-controlled van.', '2026-10-10 10:05:00', 1),
(7, NULL, 5, NULL, 'Payment Confirmed', N'Payment received', N'Payments for BK-2026-004185 are complete. Your invoices are available under Invoices.', '2026-10-03 10:00:00', 1),
(8, 2, 8, NULL, 'Offer', N'Weekend moving offer', N'Book a Single Move for Friday or Saturday in October and get packing materials at half price.', '2026-10-01 12:00:00', 0),
(9, 2, NULL, 5, 'Service Update', N'Updated safety briefing', N'All movers: new lifting and strapping procedure from 15 October. Read it before your next job.', '2026-10-09 16:00:00', 0),
(10, 1, NULL, 10, 'Service Update', N'Warehouse audit', N'Quarterly warehouse occupancy audit on 20 October, 08:00–12:00.', '2026-10-10 08:30:00', 0);
SET IDENTITY_INSERT announcements OFF;
GO
/* row counts */
SELECT 'admins' t, COUNT(*) n FROM admins UNION ALL SELECT 'customers', COUNT(*) FROM customers
UNION ALL SELECT 'staff', COUNT(*) FROM staff UNION ALL SELECT 'vehicles', COUNT(*) FROM vehicles
UNION ALL SELECT 'warehouses', COUNT(*) FROM warehouses UNION ALL SELECT 'item_categories', COUNT(*) FROM item_categories
UNION ALL SELECT 'services', COUNT(*) FROM services UNION ALL SELECT 'bookings', COUNT(*) FROM bookings
UNION ALL SELECT 'booking_services', COUNT(*) FROM booking_services UNION ALL SELECT 'items', COUNT(*) FROM items
UNION ALL SELECT 'item_photos', COUNT(*) FROM item_photos UNION ALL SELECT 'storage_requests', COUNT(*) FROM storage_requests
UNION ALL SELECT 'payments', COUNT(*) FROM payments UNION ALL SELECT 'reviews', COUNT(*) FROM reviews
UNION ALL SELECT 'messages', COUNT(*) FROM messages UNION ALL SELECT 'announcements', COUNT(*) FROM announcements;
GO
