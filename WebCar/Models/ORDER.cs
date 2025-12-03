namespace WebCar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("CARSALE.ORDERS")]
    public partial class ORDER
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ORDER()
        {
            ORDER_DETAIL = new HashSet<ORDER_DETAIL>();
        }

        [Key]
        public decimal MADON { get; set; }

        public decimal? MAKH { get; set; }

        public DateTime? NGAYDAT { get; set; }

        public decimal? TONGTIEN { get; set; }

        [StringLength(30)]
        public string TRANGTHAI { get; set; }

        public virtual CUSTOMER CUSTOMER { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ORDER_DETAIL> ORDER_DETAIL { get; set; }
    }
}
