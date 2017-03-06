--Propósito. Inserta datos en la tabla dibb_CollectionOfBankSlips
--3/2/16 jcf Creación
--
IF OBJECT_ID('dbo.dibb_spCollectionOfBankSlips_insUpd','P') IS NOT NULL
DROP PROC dbo.dibb_spCollectionOfBankSlips_insUpd
GO

create PROCEDURE dbo.dibb_spCollectionOfBankSlips_insUpd
@DOCNUMBR      char(21),	
@numBeneficiario varchar(21),
@archivo       varchar(150) = NULL,
@usuario       varchar(50) = NULL
AS

	IF EXISTS (SELECT 1 FROM dbo.dibb_CollectionOfBankSlips
	WHERE DOCNUMBR = @DOCNUMBR
	 )
	BEGIN
		UPDATE dbo.dibb_CollectionOfBankSlips
		   SET numBeneficiario = @numBeneficiario,
				archivo       = @archivo,
			   usuario       = @usuario,
			   fechaHora = getdate()
		 WHERE DOCNUMBR = @DOCNUMBR
	END
	ELSE
	BEGIN
 
		INSERT INTO dbo.dibb_CollectionOfBankSlips
		(DOCNUMBR,numBeneficiario,archivo,usuario)
		SELECT @DOCNUMBR,@numBeneficiario,@archivo,@usuario
 
	END
 
GO
