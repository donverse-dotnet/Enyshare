;-- Allow access to the EventLogStore database
GRANT ALL PRIVILEGES ON EventLogStore.* TO 'user'@'%';
FLUSH PRIVILEGES;
