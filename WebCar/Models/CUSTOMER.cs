namespace WebCar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("CARSALE.CUSTOMER")]
    public partial class CUSTOMER
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CUSTOMER()
        {
            ACCOUNT_ROLE = new HashSet<ACCOUNT_ROLE>();
            FEEDBACKs = new HashSet<FEEDBACK>();
            ORDERS = new HashSet<ORDER>();
        }

        [Key]
        public decimal MAKH { get; set; }

        [StringLength(100)]
        public string HOTEN { get; set; }

        [StringLength(100)]
        public string EMAIL { get; set; }

        [StringLength(20)]
        public string SDT { get; set; }

        [StringLength(256)]
        public string MATKHAU { get; set; }

        [StringLength(200)]
        public string DIACHI { get; set; }

        public DateTime? NGAYDANGKY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ACCOUNT_ROLE> ACCOUNT_ROLE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FEEDBACK> FEEDBACKs { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ORDER> ORDERS { get; set; }
    }
}
