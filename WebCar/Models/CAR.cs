namespace WebCar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("CARSALE.CAR")]
    public partial class CAR
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CAR()
        {
            FEEDBACKs = new HashSet<FEEDBACK>();
            ORDER_DETAIL = new HashSet<ORDER_DETAIL>();
        }

        [Key]
        public decimal MAXE { get; set; }

        [StringLength(100)]
        public string TENXE { get; set; }

        [StringLength(50)]
        public string HANGXE { get; set; }

        public decimal? GIA { get; set; }

        public short? NAMSX { get; set; }

        [StringLength(4000)]
        public string MOTA { get; set; }

        [StringLength(200)]
        public string HINHANH { get; set; }

        [StringLength(30)]
        public string TRANGTHAI { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FEEDBACK> FEEDBACKs { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ORDER_DETAIL> ORDER_DETAIL { get; set; }
    }
}
