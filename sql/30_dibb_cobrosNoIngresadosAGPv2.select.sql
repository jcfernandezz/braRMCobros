
------------------------------------------------------------
--Propósito. Obtiene cobros de boletos bancarios que todavía no ingresaron a GP
--16/2/16 jcf Creación
-- revisar planilla 160129 gbra facturas faltantes para aplicar pagos en PROD.xls para datos adicionales
--
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[vwDibbCobrosFaltantesEnGP]') AND OBJECTPROPERTY(id,N'IsView') = 1)
    DROP view dbo.vwDibbCobrosFaltantesEnGP;
GO

alter view dbo.vwDibbCobrosFaltantesEnGP as
select bs.archivo, bs.docnumbr nossoNumero, bs.numBeneficiario, 
	CASE WHEN fc.docnumbr is null then 'Factura no está en GP' else 'Factura disponible en GP' end obs, 
	fc.Inv_Date invoiceDate, fc.docnumbr, fc.custnmbr, fc.Customer_Name customerName, fc.amount currentAmount, fc.totalDoc,
	ap.apfrdcnm payment, sa.sumApfrmaplyamt sumOfPayments,
	case when charindex( rtrim(bs.docnumbr), ap.apfrdcnm) > 0 then 'yes' else 'no' end automaticUploaded
from dibb_CollectionOfBankSlips bs
	left join dbo.vwLocBraRmFacturasYBolBancarios fc
		on left(replace(bs.numBeneficiario, '-', ''), 6) = left(fc.docnumbr, 6)
	outer apply dbo.fnRmGetCobrosAplicados(1, fc.docnumbr) ap
	outer apply (
				select sum(apfrmaplyamt) sumApfrmaplyamt
				from vwRmTrxAplicadas
				where APTODCNM = fc.docnumbr
				and APTODCTY = 1
				and APFRDCTY = 9
				) sa
go
IF (@@Error = 0) PRINT 'Creación exitosa de la vista: vwDibbCobrosFaltantesEnGP'
ELSE PRINT 'Error en la creación de la vista: vwDibbCobrosFaltantesEnGP'
GO

-------------------------------------------------------------------------
select *
from vwDibbCobrosFaltantesEnGP 
where --docnumbr = 'B33981'
automaticUploaded = 'no'
order by 1 --numBeneficiario

select count(*)
from dibb_CollectionOfBankSlips

--test
--select * --distinct right(bs.archivo, 33)
----, bs.docnumbr, numBeneficiario
--from dibb_CollectionOfBankSlips bs
--order by 1

--sp_columns dibb_CollectionOfBankSlips 

select *
from dbo.vwRmTransaccionesTodas
where --custnmbr = '42516278/000166'
docnumbr in ('PYMNT000000001272', 'PYMNT000000001653', 'RB58578900037', 'RB58578900045')

select *
from dbo.vwRmTransaccionesWork
where --custnmbr = '42516278/000166'
docnumbr in ('PYMNT000000001272', 'PYMNT000000001653', 'RB58578900037', 'RB58578900045')
