namespace WebCar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("CARSALE.AUDIT_LOG")]
    public partial class AUDIT_LOG
    {
        [Key]
        public decimal MALOG { get; set; }

        public decimal? MATK { get; set; }

        [StringLength(100)]
        public string HANHDONG { get; set; }

        [StringLength(50)]
        public string BANGTACDONG { get; set; }

        public DateTime? NGAYGIO { get; set; }

        [StringLength(30)]
        public string IP { get; set; }
    }
}
