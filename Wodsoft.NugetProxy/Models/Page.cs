using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Wodsoft.NugetProxy.Models
{
    public class Page
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Path { get; set; }

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime ExpiredDate { get; set; }

        [Required]
        public string ContentType { get; set; }

        [Required]
        public byte[] Content { get; set; }
    }
}