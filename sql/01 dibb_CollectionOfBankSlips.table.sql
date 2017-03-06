
--Propósito. Guarda el nosso numero de los boletos bancarios cargados desde archivos excel.
--3/2/16 jcf Creación
--
IF OBJECT_ID('dbo.dibb_CollectionOfBankSlips', 'U') IS NOT NULL
  DROP TABLE dbo.dibb_CollectionOfBankSlips
GO

CREATE TABLE dbo.dibb_CollectionOfBankSlips
(
	DOCNUMBR char(21) not null,
	numBeneficiario varchar(21) default '',
	archivo varchar(150) default '' ,
	usuario varchar(50) default '' ,
	fechaHora datetime default(getdate()),
    CONSTRAINT pk_collectionOfBankSlips PRIMARY KEY (DOCNUMBR)
)
GO

-------------------------------------------------------------------------------------
--sp_columns rm20101 
--sp_statistics rm20101 
---- vwLocBraRmFacturasYBolBancarios 

--select top 100 *
--from rm20101 
--where rmdtypal = 9

--select *
--from dibb_CollectionOfBankSlips


