USE [PET]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE  PROCEDURE [dbo].[User_Crud]
@Action varchar(20),
@UserId int = null,
@Name varchar(50) = null,
@Username varchar(50) = null,
@Mobile varchar(50) = null,
@Email varchar(50) = null,
@Address varchar(max) = null,
@PostCode varchar(50) = null,
@Password varchar(50) = null,
@ImageUrl varchar(max) = null
AS
BEGIN
	
	SET NOCOUNT ON;

	-- select for login
	if @Action = 'SELECT4LOGIN'
	begin
	     select * from dbo.Users 
		 where Username = @Username
		 and Password = @Password
   end

   --select for user profile
   if @Action = 'SELECT4PROFILE'
   begin
        select * from dbo.Users  where UserId = @UserId
	end

	-- insert (registration)
	if @Action = 'INSERT'
	begin
	     insert into dbo.Users(Name, Username, Mobile, Email, Address, PostCode,Password, ImageUrl,CreatedDate)
		 values (@Name,@Username,@Mobile,@Email,@Address,@PostCode,@Password,@ImageUrl,getdate())
    end

	-- update user profile
	if @Action = 'UPDATE'
	begin
	     declare @UPDATE_IMAGE varchar(20)
		 select @UPDATE_IMAGE = (case when @ImageUrl is null then 'NO' else 'YES' end)
		 if @UPDATE_IMAGE = 'NO'
		    begin
			     update dbo.Users
				 set Name = @Name, Username = @Username, Mobile = @Mobile, Email = @Email, Address = @Address, PostCode = @PostCode
				 where UserId = @UserId
				 end
				 else
				    begin
					update dbo.Users
					set Name=@Name,Username = @Username, Mobile = @Mobile, Email = @Email, Address = @Address, PostCode = @PostCode, ImageUrl = @ImageUrl
					where UserId = @UserId
					end
           end


					--select for admin
					if @Action = 'SELECT4ADMIN'
					begin
					      select row_number() over(order by (select 1)) as [SrNo], UserId, Name,
						  Username, Email, CreatedDate
						  from Users
				    end

					--delete by admin
					if @Action = 'DELETE'
					begin
					     delete from dbo.Users 
						 where UserId = @UserId
				    end

END



---
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Category_Crud]
	-- Add the parameters for the stored procedure here
	@Action varchar(10),
	@CategoryId int = null,
	@Name varchar(100) = null,
	@IsActive bit = false,
	@ImageUrl varchar(max) = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--select
	IF @Action ='SELECT'
    BEGIN
	     SELECT * FROM dbo.Categories ORDER BY CreatedDate DESC
	END

	-- activecategory
	IF @Action = 'ACTIVECAT'
	BEGIN
	     SELECT * FROM dbo.Categories
		 WHERE IsActive = 1
    END
		 -- insert
		 IF @Action = 'INSERT'
		 BEGIN
		      INSERT INTO dbo.Categories(Name, ImageUrl, IsActive, CreatedDate)
			  VALUES (@Name, @ImageUrl, @IsActive, GETDATE())
         END
		 
		 --update
	 IF @Action = 'UPDATE'
		BEGIN 
		    DECLARE @UPDATE_IMAGE VARCHAR(20)
			SELECT @UPDATE_IMAGE = (CASE WHEN  @ImageUrl IS NULL THEN 'NO' ELSE 'YES' END)
		IF @UPDATE_IMAGE = 'NO'
				 BEGIN
				 UPDATE dbo.Categories
				 SET NAME = @Name, IsActive = @IsActive
			 WHERE CategoryId = @CategoryId
		 END
	ELSE
				 BEGIN
				  UPDATE dbo.Categories
				  SET NAME = @Name,ImageUrl = @ImageUrl, IsActive = @IsActive
				  WHERE CategoryId = @CategoryId
		 END
	END

	--delete
	IF @Action = 'DELETE' 
	BEGIN
	  DELETE FROM dbo.Categories WHERE CategoryId = @CategoryId
   END
   
   --getbyid
   IF @Action = 'GETBYID'
   BEGIN
        SELECT * FROM dbo.Categories WHERE CategoryId = @CategoryId
    END

END

--
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[Invoice]
@Action varchar(10),
@PaymentId int = null,
@UserId int = null,
@OrderDetailsId int = null,
@Status varchar(50) = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- get invoice by id
	if @Action = 'INVOICBYID'
	begin
	select ROW_NUMBER() over(order by (select 1)) as [SrNo], o.OrderNo, p.Name, p.Price, o.Quantity,
	(p.Price * o.Quantity) as TotalPrice, o.OrderDate,o.Status from Orders o
	inner join Products p on p.ProductId = o.ProductId
	where o.PaymentId = @PaymentId and o.UserId = @UserId
	end

	--select order history
	if @Action = 'ODRHISTORY'
	begin
	select distinct o.PaymentId, p.PaymentMethod,p.CardNo from Orders o
	inner join Payment p on p.PaymentId = o.PaymentId
	where o.UserId = @UserId
	end

	--get order status
	if @Action = 'GETSTATUS'
	begin
	select o.OrderDetailsId, o.OrderNo, (pr.Price * o.Quantity) as TotalPrice, o.Status,
	o.OrderDate, p.PaymentMethod, pr.Name from Orders o
	inner join Payment p on p.PaymentId = o.PaymentId
	inner join Products pr on pr.ProductId = o.ProductId
	end

	--get order status by id
	if @Action = 'STATUSBYID'
	begin
	 select OrderDetailsId, Status from Orders
	 where OrderDeatailsId = @OrderDetailsId
	 end

	--update order status
	if @Action= 'UPDTSTATUS'
	begin
	update dbo.Orders
	set Status = @Status 
	where OrderDeatailsId = @OrderDetailsId
	end



END

--
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Product_Crud]
	@Action varchar(10),
	@ProductId int = null,
	@Name varchar(100) = null,
	@Description varchar(max) = null,
	@Price decimal(18,2) = 0,
	@Quantity int = null,
	@ImageUrl varchar(max) = null,
	@CategoryId int = null,
	@IsActive bit = false
AS
BEGIN
	SET NOCOUNT ON;
	--SELECT
	IF @Action = 'SELECT'
	BEGIN
	     SELECT p.*, c.Name AS CategoryName FROM dbo.Products p
		 INNER JOIN dbo.Categories c ON c.CategoryId = p.CategoryId ORDER BY p.CreatedDate DESC
     END

	 --SELECT
	IF @Action = 'ACTIVEPROD'
	BEGIN
	     SELECT p.*, c.Name AS CategoryName FROM dbo.Products p
		 INNER JOIN dbo.Categories c ON c.CategoryId = p.CategoryId 
		 WHERE p.IsActive =	1
     END

	 -- INSERT
	 IF @Action = 'INSERT'
	 BEGIN
	       INSERT INTO dbo.Products(Name, Description, Price, Quantity, ImageUrl, CategoryId,IsActive, CreatedDate)
		   VALUES (@Name, @Description, @Price, @Quantity,@ImageUrl, @CategoryId, @IsActive, GETDATE())
    END

	-- UPDATE
	IF @Action = 'UPDATE'
	BEGIN
	     DECLARE @UPDATE_IMAGE varchar(20)
		 SELECT @UPDATE_IMAGE = (CASE WHEN @ImageUrl IS NULL THEN 'NO' ELSE 'YES' END)
		 IF @UPDATE_IMAGE = 'NO'
		   BEGIN
		        UPDATE dbo.Products
				SET Name= @Name, Description = @Description, Price = @Price,
				Quantity = @Quantity, CategoryId = @CategoryId, IsActive = @IsActive
				WHERE ProductId = @ProductId
            END
		 ELSE
		     BEGIN
			      UPDATE dbo.Products
				  SET Name = @Name, Description = @Description, Price = @Price,
				  ImageUrl = @ImageUrl, CategoryId = @CategoryId, IsActive = @IsActive
				  WHERE ProductId = @ProductId
              END
         END

		 -- UPDATE QUANTITY
		 IF @Action = 'QTYUPDATE'
		 BEGIN
		      UPDATE dbo.Products SET Quantity = @Quantity
			  WHERE ProductId = @ProductId
         END

		 --DELETE
		 IF @Action = 'DELETE'
		 BEGIN
		      DELETE FROM dbo.Products WHERE ProductId = @ProductId
         END

		 -- GETBYID
		 IF @Action = 'GETBYID'
		 BEGIN
		      SELECT * FROM dbo.Products WHERE ProductId = @ProductId
		 END

END          

--
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Save_Orders] @tblOrders OrderDetails READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO Orders(OrderNo,ProductId,Quatity,UserId, Status, PaymentId,OrderDate)
	SELECT OrderNo,ProductId, Quantity, UserId, Status, PaymentId, OrderDate FROM  @tblOrders
END


--
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Save_Payment]
@Name varchar(100) = null,
@CardNo varchar(50) = null,
@ExpiryDate varchar(50) = null,
@Cvv int = null,
@Address varchar(max) = null,
@PaymentMethod varchar(10) = 'card',
@InsertedId int output
as
begin
SET NOCOUNT ON;

--INSERT
BEGIN 
INSERT INTO dbo.Payment(Name, CardNo, ExpiryDate,CvvNo,Address,PaymentMethod)
VALUES(@Name, @CardNo,@ExpiryDate,@Cvv,@Address,@PaymentMethod)
 
 SELECT @InsertedId = SCOPE_IDENTITY();
 END
 END
--
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[User_Crud]
@Action varchar(20),
@UserId int = null,
@Name varchar(50) = null,
@Username varchar(50) = null,
@Mobile varchar(50) = null,
@Email varchar(50) = null,
@Address varchar(max) = null,
@PostCode varchar(50) = null,
@Password varchar(50) = null,
@ImageUrl varchar(max) = null
AS
BEGIN
	
	SET NOCOUNT ON;

	-- select for login
	if @Action = 'SELECT4LOGIN'
	begin
	     select * from dbo.Users 
		 where Username = @Username
		 and Password = @Password
   end

   --select for user profile
   if @Action = 'SELECT4PROFILE'
   begin
        select * from dbo.Users  where UserId = @UserId
	end

	-- insert (registration)
	if @Action = 'INSERT'
	begin
	     insert into dbo.Users(Name, Username, Mobile, Email, Address, PostCode,Password, ImageUrl,CreatedDate)
		 values (@Name,@Username,@Mobile,@Email,@Address,@PostCode,@Password,@ImageUrl,getdate())
    end

	-- update user profile
	if @Action = 'UPDATE'
	begin
	     declare @UPDATE_IMAGE varchar(20)
		 select @UPDATE_IMAGE = (case when @ImageUrl is null then 'NO' else 'YES' end)
		 if @UPDATE_IMAGE = 'NO'
		    begin
			     update dbo.Users
				 set Name = @Name, Username = @Username, Mobile = @Mobile, Email = @Email, Address = @Address, PostCode = @PostCode
				 where UserId = @UserId
				 end
				 else
				    begin
					update dbo.Users
					set Name=@Name,Username = @Username, Mobile = @Mobile, Email = @Email, Address = @Address, PostCode = @PostCode, ImageUrl = @ImageUrl
					where UserId = @UserId
					end
           end


					--select for admin
					if @Action = 'SELECT4ADMIN'
					begin
					      select row_number() over(order by (select 1)) as [SrNo], UserId, Name,
						  Username, Email, CreatedDate
						  from Users
				    end

					--delete by admin
					if @Action = 'DELETE'
					begin
					     delete from dbo.Users 
						 where UserId = @UserId
				    end

END

Create type [dbo].[OrderDetails] as table(
[OrderNo] [varchar] (max) null,
[ProductId] [int] null,
[Quantity] [int] null,
[UserId] [int] null,
[Status] [varchar](50) null,
[PaymentId] [int] null,
[OrderDate] [datetime] null
)
