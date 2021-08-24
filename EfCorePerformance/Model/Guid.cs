using System.ComponentModel.DataAnnotations;

namespace EFTestApp.Model
{
    public class Guid
    {
        public System.Guid Id { set; get; }
        
        [StringLength(50)]
        public string FirstName { set; get; }

        [StringLength(50)]
        public string Surname { set; get; }

        [StringLength(20)]
        public string IdNumber { set; get; } = string.Empty;
    }
}