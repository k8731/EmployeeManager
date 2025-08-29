using System.ComponentModel.DataAnnotations;

namespace EmployeeManager.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "姓名必填")]
        [StringLength(10, ErrorMessage = "姓名不能超過10個字")]
        public string Name { get; set; }

        [Required(ErrorMessage = "部門必填")]
        public string Department { get; set; }

        [Required(ErrorMessage = "Email 必填")]
        [EmailAddress(ErrorMessage = "Email 格式錯誤")]
        public string Email { get; set; }
    }
}
