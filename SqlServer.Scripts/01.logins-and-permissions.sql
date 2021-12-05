/*
to allow the ErrorLogWatcher to read the error log
*/
use master
go
create user monitoring_ro for login monitoring_ro
go
grant exec on xp_readerrorlog to monitoring_ro