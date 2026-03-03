CREATE TABLE users (
    id INT IDENTITY(1,1) PRIMARY KEY,
    email NVARCHAR(255) NOT NULL UNIQUE,
    hashed_password NVARCHAR(255) NOT NULL,
    first_name NVARCHAR(100) NOT NULL,
    last_name NVARCHAR(100) NOT NULL,
    enabled BIT NOT NULL DEFAULT 1,
    created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    updated_at DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);

CREATE TABLE time_audits (
    id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    type NVARCHAR(20) NOT NULL CHECK (type IN ('clock_in', 'clock_out')),
    timestamp DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_time_audits_users FOREIGN KEY (user_id) REFERENCES users(id)
);

