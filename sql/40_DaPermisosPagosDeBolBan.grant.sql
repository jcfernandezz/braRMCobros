--Brasil
--Pagos de boletos bancarios
--Propósito. Rol que da accesos a objetos de pagos de boletos bancarios
--Requisitos. Ejecutar en la compañía.
--25/01/16 JCF Creación
--
-----------------------------------------------------------------------------------
--use [COMPAÑIA]

IF DATABASE_PRINCIPAL_ID('rol_locBrasil') IS NULL
	create role rol_locBrasil;

grant select on dbo.vwLocBraRmFacturasYBolBancarios to rol_locBrasil, dyngrp;
grant select on dbo.vwLocBraBoletosBancarios to rol_locBrasil, dyngrp;
grant select, update, delete on dbo.dibb_CollectionOfBankSlips to rol_locBrasil, dyngrp;
grant execute on dbo.dibb_spCollectionOfBankSlips_insUpd to rol_locBrasil, dyngrp;
grant select on dbo.vwDibbCobrosFaltantesEnGP to rol_locBrasil, dyngrp;
