namespace WebCar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("CARSALE.ORDER_DETAIL")]
    public partial class ORDER_DETAIL
    {
        [Key]
        [Column(Order = 0)]
        public decimal MADON { get; set; }

        [Key]
        [Column(Order = 1)]
        public decimal MAXE { get; set; }

        public decimal? SOLUONG { get; set; }

        public decimal? DONGIA { get; set; }

        public virtual CAR CAR { get; set; }

        public virtual ORDER ORDER { get; set; }
    }
}
