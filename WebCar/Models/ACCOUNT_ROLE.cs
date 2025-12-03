namespace WebCar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("CARSALE.ACCOUNT_ROLE")]
    public partial class ACCOUNT_ROLE
    {
        [Key]
        public decimal MATK { get; set; }

        public decimal? MAKH { get; set; }

        [StringLength(30)]
        public string ROLENAME { get; set; }

        public virtual CUSTOMER CUSTOMER { get; set; }
    }
}
