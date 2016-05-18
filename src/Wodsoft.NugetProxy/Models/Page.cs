using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Wodsoft.NugetProxy.Models
{
    [Table("Pages")]
    public class Page
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Path { get; set; }

        [Required]
        public DateTime ExpiredDate { get; set; }

        [Required]
        public string ContentType { get; set; }

        [Required]
        public byte[] Content { get; set; }
    }
}
