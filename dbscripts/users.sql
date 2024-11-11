CREATE TABLE Users (
    UserId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Username VARCHAR(50) UNIQUE NOT NULL,
    PasswordHash TEXT NOT NULL,
    Email VARCHAR(100) UNIQUE NOT NULL,
    FullName VARCHAR(100),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    LastLogin TIMESTAMP
);

CREATE TABLE UserSessions (
    SessionId uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL,
    Token TEXT NOT NULL,
    IpAddress VARCHAR(45),
    DeviceType VARCHAR(50),
    DeviceFamily VARCHAR(50),
    BrowserName VARCHAR(50),
    BrowserVersion VARCHAR(50),
    ExpirationUnixTime bigint NOT NULL,
    LoginTime TIMESTAMP NOT NULL,
	IsActive bool not null default true
);


