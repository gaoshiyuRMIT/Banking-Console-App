create procedure [dbo].[TransferBetweenAccounts]
    @SrcNo int, @DestNo int, @Amount money
as
update Account set Balance += @Amount where AccountNumber = @SrcNo;
update Account set Balance -= @Amount where AccountNumber = @DestNo;
go
