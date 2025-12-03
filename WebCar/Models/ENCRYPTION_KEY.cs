namespace WebCar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("CARSALE.ENCRYPTION_KEY")]
    public partial class ENCRYPTION_KEY
    {
        [Key]
        public decimal KEYID { get; set; }

        [StringLength(20)]
        public string KEYTYPE { get; set; }

        [StringLength(4000)]
        public string PUBLICKEY { get; set; }

        [StringLength(4000)]
        public string PRIVATEKEY { get; set; }

        public DateTime? CREATEDDATE { get; set; }
    }
}
