//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DBAccess.Database
{
    using System;
    using System.Collections.Generic;
    
    public partial class StockData
    {
        public int Id { get; set; }
        public float Open { get; set; }
        public float High { get; set; }
        public float Low { get; set; }
        public float Close { get; set; }
        public float Volume { get; set; }
        public int ItemId { get; set; }
        public System.DateTime DateTimeStamp { get; set; }
    
        public virtual Items Item { get; set; }
    }
}
