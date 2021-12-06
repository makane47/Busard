USE master
GO

CREATE LOGIN [monitoring_ro]
WITH PASSWORD=N'password !',
DEFAULT_DATABASE=[master],
DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF,
GO

-- to allow the ErrorLogWatcher to read the error log
CREATE USER monitoring_ro FOR login monitoring_ro
GO
GRANT EXEC ON xp_readerrorlog TO monitoring_ro


GRANT VIEW SERVER STATE TO [monitoring_ro]
GRANT VIEW ANY DEFINITION TO [monitoring_ro]
GRANT ALTER ANY EVENT SESSION TO [monitoring_ro]
GO
