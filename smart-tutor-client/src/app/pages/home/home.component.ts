import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {
  activeFaqIndex: number | null = 0;
  isSimulatedSuccess = false;

  faqs = [
    {
      question: 'Phụ huynh có cần phải tải app hoặc tạo tài khoản để xem báo cáo không?',
      answer: 'Hoàn toàn KHÔNG. SmartTutor sử dụng công nghệ Magic Link độc quyền bảo mật. Phụ huynh chỉ cần nhấp vào link gửi qua Zalo/SMS là có thể xem trực tiếp phiếu báo cáo chi tiết và quét mã VietQR để thanh toán ngay trên trình duyệt điện thoại.'
    },
    {
      question: 'Tính năng tự động gạch nợ qua VietQR hoạt động như thế nào?',
      answer: 'Hệ thống tự động sinh một mã chuyển khoản duy nhất (TransferCode) cho từng học sinh trong tháng. Khi phụ huynh quét mã VietQR và chuyển khoản, Webhook ngân hàng sẽ tự động đối soát và cập nhật trạng thái hóa đơn thành "Đã thanh toán" tức thì 24/7 mà giáo viên không cần phải kiểm tra sao kê bằng tay.'
    },
    {
      question: 'Trợ lý AI viết nhận xét học tập có chính xác không?',
      answer: 'AI của SmartTutor được tối ưu chuyên biệt cho ngữ cảnh sư phạm tại Việt Nam. AI sẽ đọc toàn bộ dữ liệu điểm danh, tỷ lệ hoàn thành BTVN (%), thái độ từng buổi học và nội dung bài học trong tháng để tạo ra đoạn nhận xét khách quan, vừa động viên vừa nêu rõ điểm cần khắc phục.'
    },
    {
      question: 'Nếu học sinh nộp thừa tiền từ tháng trước thì hệ thống xử lý ra sao?',
      answer: 'Khoản tiền dư sẽ được tự động lưu vào Số dư tích lũy (CreditBalance) của học sinh. Đến kỳ báo cáo tháng tiếp theo, SmartTutor sẽ tự động cấn trừ số dư này trước khi tính ra số tiền cuối cùng (FinalAmount) phụ huynh cần đóng.'
    },
    {
      question: 'Tôi có thể dùng SmartTutor trên điện thoại di động không?',
      answer: 'Có. Toàn bộ giao diện SmartTutor được thiết kế theo chuẩn Mobile-First cực kỳ mượt mà, giúp Thầy Cô có thể điểm danh và chốt công ngay trên điện thoại sau mỗi buổi dạy.'
    }
  ];

  toggleFaq(index: number): void {
    this.activeFaqIndex = this.activeFaqIndex === index ? null : index;
  }

  simulatePayment(): void {
    this.isSimulatedSuccess = true;
    setTimeout(() => {
      this.isSimulatedSuccess = false;
    }, 4000);
  }
}
