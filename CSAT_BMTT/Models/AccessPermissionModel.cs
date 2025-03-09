using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSAT_BMTT.Models
{
    public class AccessPermissionModel
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Requestor")]
        public int RequestorID { get; set; } // Id Người gửi

        public virtual User Requestor { get; set; }
        [ForeignKey("Target")]
        public int TargetId { get; set; } // Id Người phê duyệt
        public virtual User Target { get; set; }

        public string? RequestorPublicKey { get; set; } // Publickey của người gửi
        public string? TargetIvKey { get; set; } // IvKey của người phê duyệt được mã hóa theo PublicKey của người gửi
        public string? TargetStaticKey { get; set; } // StaticKey của người phê duyệt được mã hóa theo PublicKey của người gửi
        public AccessPermissionStatus Status { get; set; } = AccessPermissionStatus.Pending; // Trạng thái của yêu cầu, mặc định là Pending
    }

    public enum AccessPermissionStatus
    {
        Pending,
        Approved,
        Declined
    }
}
