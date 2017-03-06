
------------------------------------------------------------
--Propósito. Obtiene cobros de boletos bancarios que todavía no ingresaron a GP
--16/2/16 jcf Creación
-- revisar planilla 160129 gbra facturas faltantes para aplicar pagos en PROD.xls para datos adicionales
--
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[vwDibbCobrosFaltantesEnGP]') AND OBJECTPROPERTY(id,N'IsView') = 1)
    DROP view dbo.vwDibbCobrosFaltantesEnGP;
GO

create view dbo.vwDibbCobrosFaltantesEnGP as
select mi.archivo, mi.docnumbr nossoNumero, mi.numBeneficiario, 'Factura disponible en GP' obs, fc.docdate, fc.docnumbr, fc.custnmbr, fc.custname, fc.montoActual, fc.voidstts 
from vwRmTransaccionesTodas fc
inner join	
	(
	select bs.archivo, bs.docnumbr, bs.numBeneficiario
	from dibb_CollectionOfBankSlips bs
	where not exists(select docnumbr 
					from vwRmTransaccionesTodas rm
					where rm.docnumbr = 'RB'+bs.docnumbr
					union all
					select docnumbr
					from rm10201
					where docnumbr = 'RB'+bs.docnumbr
					)
	) mi
	on left(replace(mi.numBeneficiario, '-', ''), 6) = left(fc.docnumbr, 6)
--outer apply (
--	select top 1 APFRDCNM from vwRmTrxAplicadas where apfrdcty = 9 and APTODCNM = fc.docnumbr and APTODCTY = 1
--	) ap
where fc.soptype = 3
union all

select bs.archivo, bs.docnumbr, numBeneficiario, 'Factura no está en GP' obs, '1/1/1900' docdate, '' docnumbr, '' custnmbr, '' custname, 0.0 montoActual, 0 voidstts 
	from dibb_CollectionOfBankSlips bs
	where not exists(select docnumbr 
					from vwRmTransaccionesTodas rm
					where rm.docnumbr = 'RB'+bs.docnumbr
					union all
					select docnumbr
					from rm10201
					where docnumbr = 'RB'+bs.docnumbr
					)
go
IF (@@Error = 0) PRINT 'Creación exitosa de la vista: vwDibbCobrosFaltantesEnGP'
ELSE PRINT 'Error en la creación de la vista: vwDibbCobrosFaltantesEnGP'
GO

-------------------------------------------------------------------------
select *
from vwDibbCobrosFaltantesEnGP 
order by 1

--test
--select * --distinct right(bs.archivo, 33)
----, bs.docnumbr, numBeneficiario
--from dibb_CollectionOfBankSlips bs
--order by 1

--sp_columns dibb_CollectionOfBankSlips 
