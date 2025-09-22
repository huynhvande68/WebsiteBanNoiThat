namespace Models.EF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Category")]
    public partial class Category
    {
        [DisplayName("Mã loại sản phẩm")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CategoryId { get; set; }

        [StringLength(50)]
        [DisplayName("Tên loại sản phẩm")]
        public string Name { get; set; }

        [StringLength(50)]
        [DisplayName("Diễn giải")]
        public string MetaTitle { get; set; }

        [DisplayName("Mã loại cha")]
        public int? ParId { get; set; }
    }
}
