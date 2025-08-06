;-- Create the EventLog table
CREATE TABLE IF NOT EXISTS EventLogStore.EventLogs (
    EventId     CHAR(36)       NOT NULL PRIMARY KEY,
    EventType   INT            NOT NULL,
    EventData   TEXT           NOT NULL,
    FiredAt     TIMESTAMP      NOT NULL DEFAULT CURRENT_TIMESTAMP
);
