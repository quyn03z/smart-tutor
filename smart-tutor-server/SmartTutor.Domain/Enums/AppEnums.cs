using System;
using System.Collections.Generic;
using System.Text;

namespace SmartTutor.Domain.Enums
{
    public static class AppEnums
    {
        public enum ClassType
        {
            Individual, // Gia sư 1-1
            Group       // Lớp nhóm (10 - 20 HS)
        }


        public enum StudentStatus
        {
            Active,     // Đang học
            Inactive,   // Tạm nghỉ
            Deleted     // Đã xóa (Soft Delete)
        }


        public enum EnrollmentStatus
        {
            Active,     // Đang theo học
            Dropped,    // Đã thôi học / Rút khỏi lớp
            Completed   // Đã hoàn thành khóa
        }


        public enum SessionStatus
        {
            Scheduled,  // Đã lên lịch
            Completed,  // Đã hoàn thành buổi dạy
            Cancelled   // Ca học bị hủy
        }

        public enum AttendanceStatus
        {
            Present,    // Có mặt
            Absent,     // Vắng mặt không phép
            Excused,    // Nghỉ có phép
            Late        // Đi muộn
        }


        public enum PaymentStatus
        {
            Pending,        // Chờ thanh toán
            PartiallyPaid,  // Đã thanh toán 1 phần
            Paid,           // Đã thanh toán đủ
            Overpaid        // Đóng dư tiền
        }


        public enum PaymentGateway
        {
            BankTransfer,   // Chuyển khoản ngân hàng / VietQR
            Cash,           // Tiền mặt
            Momo,
            ZaloPay
        }



    }
}
