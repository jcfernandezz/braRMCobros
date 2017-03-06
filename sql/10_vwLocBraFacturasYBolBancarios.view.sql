--Localización Brasil
--
-----------------------------------------------------------------------------------------
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[vwLocBraRmFacturasYBolBancarios]') AND OBJECTPROPERTY(id,N'IsView') = 1)
    DROP view dbo.[vwLocBraRmFacturasYBolBancarios];
GO

create view dbo.vwLocBraRmFacturasYBolBancarios as
--Propósito. Lista boletos bancarios y facturas RM
--Requisito. Requiere la instalación de los siguientes objetos sql 
--			RMGeneraBoletoBancarioParaIntegra.SqlScripts
--			RMActualizaAgenciaAlGenerarBoletoBancario
--Utilizado por. Carga de cobro de boletos bancarios
--12/1/16 jcf Creación
--
select trx.Inv_Date, trx.Amount, trx.totalDoc,
	trx.inv_no,	
	trx.docnumbr, trx.intDueDate,
	trx.Payment_Date, trx.custnmbr, trx.Customer_Name, 	
	trx.Company_Address, trx.CITY, trx.STATE, trx.CEP, trx.CNPJ_CPF, 
	trx.IE,	trx.agCustnmbr,	trx.agName,	trx.agCompany_Address,	trx.agCity,	trx.agState,	trx.agCep,	trx.email
from dbo.vwLocBraBoletosBancarios trx

union all

--Facturas no convertidas en boletos
select trx.docdate Inv_Date, trx.montoActual Amount, trx.totalDoc,
	case when SUBSTRING(trx.docnumbr, 2, 1) = '-' then
		left(trx.docnumbr, 2) + dbo.fnLocBraGetRPS(SUBSTRING(trx.docnumbr, 3, 20))
	ELSE
		stuff(
		dbo.fnLocBraGetRPS(trx.docnumbr)
		, 2, 0, '-')
	end inv_no,
	trx.docnumbr, year(trx.duedate) * 10000 + month(trx.duedate) * 100 + day(trx.duedate) intDueDate,
	trx.duedate Payment_Date, trx.custnmbr, trx.custname Customer_Name, 
	rtrim(trx.address1) + ' ' + rtrim(trx.address2) Company_Address, trx.CITY, trx.STATE, trx.ZIP CEP, trx.TXRGNNUM CNPJ_CPF, 
	case when trx.userdef1 = '' then 'Isenta' else trx.userdef1 end IE,
	'' agCustnmbr,
	'' agName,
	'' agCompany_Address,
	'' agCity,
	'' agState,
	'' agCep,
	'' email
from dbo.vwRmTransaccionesTodas trx
where not exists(
		select rv.custnmbr
		from rvlsp014 rv				--rm scheduled instalments relation open
		where rv.custnmbr = trx.custnmbr
		and rv.docnumbr = trx.docnumbr
		and rv.rmdtypal = trx.rmdtypal
	)
and trx.bchsourc != 'XRM_Sales'			-- XRM_Sales: scheduled instalments	
and trx.rmdtypal = 1
and trx.voidstts = 0

go

IF (@@Error = 0) PRINT 'Creación exitosa de la vista: vwLocBraRmFacturasYBolBancarios'
ELSE PRINT 'Error en la creación de la vista: vwLocBraRmFacturasYBolBancarios'
GO
----------------------------------------------------------------------------
grant select on vwLocBraRmFacturasYBolBancarios to dyngrp;

------------------------------------------------------------------------------
--
--select count(docnumbr) repetidos
--from vwLocBraRmFacturasYBolBancarios
--where inv_no = 'B-26388'
--and payment_date = '12/4/2015'
--group by custnmbr, inv_no
--order by docnumbr, payment_date

--select top 100 *
--from vwLocBraRmFacturasYBolBancarios
--where docnumbr like '%29063%'	--'%2974%'	--

--select year(inv_date), month(inv_date), count(*)
--from vwLocBraRmFacturasYBolBancarios
--where year(inv_date) = 2015
--group by year(inv_date), month(inv_date)
--order by 1, 2
