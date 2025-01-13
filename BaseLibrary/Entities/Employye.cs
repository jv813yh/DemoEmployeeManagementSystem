using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseLibrary.Entities
{
    public class Employye 
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }
        [Required, MaxLength(100)]
        public string FullName { get; set; }

        [Required, MaxLength(50)]
        public string JobName { get; set; }

        [Required, MaxLength(100)]
        public string Address { get; set; }

        [Required, MaxLength(50)]
        public string PhoneNumber { get; set; }

        [Required, MaxLength(50)]
        public string Photo { get; set; }

        [Required, MaxLength(50)]
        public string FileNumber { get; set; }

        [Required, MaxLength(100)]
        public string OtherInfo { get; set; }


        // Relationships Many to One
        public GeneralDeparment GeneralDeparment { get; set; }
        public int GeneralDepartmentId { get; set; }

        public Department Department { get; set; }
        public int DepartmentId { get; set; }

        public Branch Branch { get; set; }
        public int BranchId { get; set; }

        public Town Town { get; set; }
        public int TownId { get; set; }
    }
}
