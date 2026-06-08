-- Seed data for testing and development

-- Insert sample animals
INSERT INTO Animals (IdentificationNumber, Name, BirthDate) VALUES
('NL-123456789', 'Bella', '2020-03-15'),
('NL-987654321', 'Daisy', '2019-07-22'),
('NL-456789123', 'Rosie', '2021-01-10'),
('NL-789123456', 'Buttercup', '2018-11-05'),
('NL-321654987', 'Clover', '2020-09-18');

-- Insert sample robots
INSERT INTO Robots (Name, Location, IsActive) VALUES
('Robot-A1', 'Barn Section A', 1),
('Robot-A2', 'Barn Section A', 1),
('Robot-B1', 'Barn Section B', 1),
('Robot-B2', 'Barn Section B', 0);

-- Insert some historical milking events
INSERT INTO MilkingEvents (AnimalId, RobotId, Timestamp, MilkYieldLiters, Duration) VALUES
(1, 1, DATEADD(DAY, -2, GETUTCDATE()), 28.5, 420),
(1, 1, DATEADD(DAY, -2, DATEADD(HOUR, 12, GETUTCDATE())), 26.3, 390),
(2, 2, DATEADD(DAY, -2, GETUTCDATE()), 32.1, 450),
(3, 1, DATEADD(DAY, -1, GETUTCDATE()), 24.8, 380),
(4, 3, DATEADD(DAY, -1, GETUTCDATE()), 29.7, 410);

-- Insert some historical weight measurements
INSERT INTO WeightMeasurements (AnimalId, RobotId, Timestamp, WeightKg) VALUES
(1, 1, DATEADD(DAY, -7, GETUTCDATE()), 652.5),
(2, 2, DATEADD(DAY, -7, GETUTCDATE()), 701.2),
(3, 1, DATEADD(DAY, -7, GETUTCDATE()), 589.8),
(4, 3, DATEADD(DAY, -7, GETUTCDATE()), 678.3),
(5, 3, DATEADD(DAY, -7, GETUTCDATE()), 623.1);
