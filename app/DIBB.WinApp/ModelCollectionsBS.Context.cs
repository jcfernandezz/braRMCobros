﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DIBB.WinApp
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class GBRAEntities : DbContext
    {
        public GBRAEntities()
            : base("name=GBRAEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
    
        public virtual int dibb_spCollectionOfBankSlips_insUpd(string dOCNUMBR, string numBeneficiario, string archivo, string usuario)
        {
            var dOCNUMBRParameter = dOCNUMBR != null ?
                new ObjectParameter("DOCNUMBR", dOCNUMBR) :
                new ObjectParameter("DOCNUMBR", typeof(string));
    
            var numBeneficiarioParameter = numBeneficiario != null ?
                new ObjectParameter("numBeneficiario", numBeneficiario) :
                new ObjectParameter("numBeneficiario", typeof(string));
    
            var archivoParameter = archivo != null ?
                new ObjectParameter("archivo", archivo) :
                new ObjectParameter("archivo", typeof(string));
    
            var usuarioParameter = usuario != null ?
                new ObjectParameter("usuario", usuario) :
                new ObjectParameter("usuario", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("dibb_spCollectionOfBankSlips_insUpd", dOCNUMBRParameter, numBeneficiarioParameter, archivoParameter, usuarioParameter);
        }
    }
}
