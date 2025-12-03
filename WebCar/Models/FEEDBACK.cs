namespace WebCar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("CARSALE.FEEDBACK")]
    public partial class FEEDBACK
    {
        [Key]
        public decimal MAFB { get; set; }

        public decimal? MAKH { get; set; }

        public decimal? MAXE { get; set; }

        [StringLength(1000)]
        public string NOIDUNG { get; set; }

        public bool? DIEMDANHGIA { get; set; }

        public DateTime? NGAYDANHGIA { get; set; }

        public virtual CAR CAR { get; set; }

        public virtual CUSTOMER CUSTOMER { get; set; }
    }
}
