import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';

export interface SessionItem {
  id: number;
  sessionNumber: number;
  date: string;
  topic: string;
  homework: string;
  attitude: 'Tốt' | 'Chăm chỉ' | 'Lười học' | 'Nói chuyện' | 'Cần tập trung';
}

export interface StudentProfile {
  id: string;
  name: string;
  shortName: string;
  subject: string;
  grade: string;
  ratePerSession: number;
  fixedTotalAmount?: number;
  groupType: '1-1' | 'Lớp nhóm';
  className?: string;
  parentPhone: string;
  transferSyntax: string;
  isPaid: boolean;
  parentAcknowledged: boolean;
  parentNote?: string;
  parentNoteTime?: string;
  aiFeedback: string;
  sessions: SessionItem[];
}

@Component({
  selector: 'app-report',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './report.component.html',
  styleUrl: './report.component.scss'
})
export class ReportComponent implements OnInit {
  selectedMonth: string = 'Tháng 8 / 2026';

  teacherName: string = 'Thầy Đình Quyền';
  teacherBankCode: string = 'TCB';
  teacherAccountNum: string = '9986678999';
  teacherAccountName: string = 'NGUYEN DINH QUYEN';

  // Notification Toast state
  toastMessage: string | null = null;
  toastType: 'success' | 'info' | 'warning' = 'success';
  private toastTimer: any = null;

  // Add Session Modal
  isAddSessionModalOpen = false;
  newSessionDate = '18/08';
  newSessionTopic = 'Luyện giải đề tổng hợp & sửa bài kiểm tra 15 phút';
  newSessionHw = '85%';
  newSessionAttitude: 'Tốt' | 'Chăm chỉ' | 'Lười học' | 'Nói chuyện' | 'Cần tập trung' = 'Tốt';

  // Parent Note Modal
  isParentNoteModalOpen = false;
  parentInputMessage = '';

  // Students Database for Live Preview
  students: StudentProfile[] = [
    {
      id: 'duyanh',
      name: 'Duy Anh',
      shortName: 'DA',
      subject: 'Toán',
      grade: 'Lớp 9',
      ratePerSession: 120000,
      fixedTotalAmount: 10000,
      groupType: '1-1',
      parentPhone: '0988.123.xxx',
      transferSyntax: 'DUYANH HOCPHI T8',
      isPaid: false,
      parentAcknowledged: true,
      parentNote: 'Cảm ơn thầy. Dạo này cháu có tập trung làm bài ở nhà hơn, nhờ thầy đôn đốc thêm phần hình học giúp gia đình nhé ạ.',
      parentNoteTime: 'Hôm qua 21:15',
      aiFeedback: 'Tư duy số học tốt, nắm được cách giải hệ phương trình. Còn thường xuyên tính toán ẩu, cần rèn thêm lượng giác cơ bản.',
      sessions: [
        { id: 1, sessionNumber: 1, date: '04/08', topic: 'Giải bài toán bằng cách lập hệ phương trình', homework: '70%', attitude: 'Tốt' },
        { id: 2, sessionNumber: 2, date: '07/08', topic: 'Tỉ số lượng giác góc nhọn và hệ quả', homework: '60%', attitude: 'Lười học' },
        { id: 3, sessionNumber: 3, date: '07/08', topic: '[Bổ trợ] Chữa bài tập hình học đường tròn', homework: '--', attitude: 'Tốt' },
        { id: 4, sessionNumber: 4, date: '09/08', topic: 'Giải bài toán lập HPT (Bài toán chuyển động)', homework: '70%', attitude: 'Nói chuyện' },
        { id: 5, sessionNumber: 5, date: '12/08', topic: 'Bài toán năng suất và làm chung làm riêng', homework: '100%', attitude: 'Chăm chỉ' },
        { id: 6, sessionNumber: 6, date: '14/08', topic: 'Ôn tập đồ thị hàm số bậc nhất y = ax + b', homework: '80%', attitude: 'Tốt' },
        { id: 7, sessionNumber: 7, date: '16/08', topic: 'Luyện đề thi thử vào 10 chuyên đề Đại số', homework: '85%', attitude: 'Tốt' },
        { id: 8, sessionNumber: 8, date: '19/08', topic: 'Hình học: Chứng minh tứ giác nội tiếp', homework: '75%', attitude: 'Cần tập trung' },
        { id: 9, sessionNumber: 9, date: '21/08', topic: 'Luyện tập chứng minh hệ thức lượng trong đường tròn', homework: '90%', attitude: 'Tốt' },
        { id: 10, sessionNumber: 10, date: '23/08', topic: 'Rút gọn biểu thức chứa căn thức bậc hai', homework: '80%', attitude: 'Chăm chỉ' },
        { id: 11, sessionNumber: 11, date: '26/08', topic: 'Phương trình bậc hai một ẩn và định lý Vi-ét', homework: '80%', attitude: 'Tốt' }
      ]
    },
    {
      id: 'minhkhang',
      name: 'Minh Khang',
      shortName: 'MK',
      subject: 'Hình học',
      grade: 'Lớp 11',
      ratePerSession: 150000,
      groupType: '1-1',
      parentPhone: '0912.456.xxx',
      transferSyntax: 'MINHKHANG HOCPHI T8',
      isPaid: false,
      parentAcknowledged: false,
      parentNote: 'Thầy cho cháu thêm bài tập nâng cao phần quan hệ vuông góc nhé ạ.',
      parentNoteTime: '3 ngày trước',
      aiFeedback: 'Tư duy hình không gian rất sáng tạo, giải nhanh các bài toán góc giữa đường thẳng và mặt phẳng. Cần chú ý cách trình bày bài tự luận chuẩn mực hơn.',
      sessions: [
        { id: 1, sessionNumber: 1, date: '03/08', topic: 'Đại cương đường thẳng và mặt phẳng trong không gian', homework: '90%', attitude: 'Tốt' },
        { id: 2, sessionNumber: 2, date: '06/08', topic: 'Hai đường thẳng song song và chéo nhau', homework: '100%', attitude: 'Chăm chỉ' },
        { id: 3, sessionNumber: 3, date: '10/08', topic: 'Đường thẳng song song với mặt phẳng', homework: '85%', attitude: 'Tốt' },
        { id: 4, sessionNumber: 4, date: '13/08', topic: 'Hai mặt phẳng song song và định lý Thales không gian', homework: '80%', attitude: 'Tốt' },
        { id: 5, sessionNumber: 5, date: '17/08', topic: 'Vectơ trong không gian và quan hệ vuông góc', homework: '70%', attitude: 'Cần tập trung' },
        { id: 6, sessionNumber: 6, date: '20/08', topic: 'Đường thẳng vuông góc với mặt phẳng', homework: '95%', attitude: 'Tốt' },
        { id: 7, sessionNumber: 7, date: '24/08', topic: 'Luyện tập xác định góc giữa đường thẳng và mặt phẳng', homework: '90%', attitude: 'Chăm chỉ' },
        { id: 8, sessionNumber: 8, date: '27/08', topic: 'Hai mặt phẳng vuông góc & hình chóp tứ giác đều', homework: '100%', attitude: 'Tốt' }
      ]
    },
    {
      id: 'baotram',
      name: 'Bảo Trâm',
      shortName: 'BT',
      subject: 'Toán',
      grade: 'Lớp 8',
      ratePerSession: 120000,
      groupType: '1-1',
      parentPhone: '0973.999.xxx',
      transferSyntax: 'BAOTRAM HOCPHI T8',
      isPaid: true,
      parentAcknowledged: true,
      parentNote: 'Cháu bảo học với Thầy dễ hiểu hơn trên lớp nhiều, cảm ơn Thầy ạ!',
      parentNoteTime: '5 ngày trước',
      aiFeedback: 'Tiến bộ vượt bậc về tính toán hằng đẳng thức đáng nhớ. Rất tự giác làm bài tập về nhà, ý thức học tập chăm chỉ và ngoan ngoãn.',
      sessions: [
        { id: 1, sessionNumber: 1, date: '02/08', topic: 'Bảy hằng đẳng thức đáng nhớ & áp dụng', homework: '100%', attitude: 'Chăm chỉ' },
        { id: 2, sessionNumber: 2, date: '05/08', topic: 'Phân tích đa thức thành nhân tử bằng PP đặt nhân tử chung', homework: '95%', attitude: 'Tốt' },
        { id: 3, sessionNumber: 3, date: '09/08', topic: 'Phân tích đa thức bằng PP dùng hằng đẳng thức & nhóm', homework: '90%', attitude: 'Tốt' },
        { id: 4, sessionNumber: 4, date: '12/08', topic: 'Chia đơn thức cho đơn thức & đa thức cho đơn thức', homework: '100%', attitude: 'Chăm chỉ' },
        { id: 5, sessionNumber: 5, date: '16/08', topic: 'Hình học: Tứ giác, tính chất đường trung bình tam giác', homework: '85%', attitude: 'Tốt' },
        { id: 6, sessionNumber: 6, date: '19/08', topic: 'Hình thang cân & dấu hiệu nhận biết', homework: '90%', attitude: 'Tốt' },
        { id: 7, sessionNumber: 7, date: '23/08', topic: 'Hình bình hành và hình chữ nhật', homework: '100%', attitude: 'Chăm chỉ' },
        { id: 8, sessionNumber: 8, date: '26/08', topic: 'Luyện tập tổng hợp tứ giác & chữa đề kiểm tra giữa kỳ', homework: '95%', attitude: 'Tốt' }
      ]
    },
    {
      id: 'lop9a_hs1',
      name: 'Hoàng Yến',
      shortName: 'HY',
      subject: 'Toán',
      grade: 'Lớp Toán 9A',
      ratePerSession: 80000,
      groupType: 'Lớp nhóm',
      className: 'Lớp Toán 9A (Chuyên Đề Vào 10)',
      parentPhone: '0934.567.xxx',
      transferSyntax: 'HOANGYEN 9A HOCPHI T8',
      isPaid: false,
      parentAcknowledged: false,
      aiFeedback: 'Tích cực xung phong phát biểu trong lớp nhóm, tốc độ làm bài nhanh và chính xác. Cần kèm thêm các dạng toán thực tế ứng dụng parabol.',
      sessions: [
        { id: 1, sessionNumber: 1, date: '04/08', topic: 'Chuyên đề giải bài toán bằng cách lập hệ phương trình', homework: '100%', attitude: 'Tốt' },
        { id: 2, sessionNumber: 2, date: '06/08', topic: 'Luyện tập dạng toán chuyển động và năng suất', homework: '100%', attitude: 'Chăm chỉ' },
        { id: 3, sessionNumber: 3, date: '11/08', topic: 'Hình học đường tròn: Góc nội tiếp & góc tạo bởi tia tiếp tuyến', homework: '95%', attitude: 'Tốt' },
        { id: 4, sessionNumber: 4, date: '13/08', topic: 'Cung bị chắn và góc có đỉnh ở bên trong, bên ngoài đường tròn', homework: '90%', attitude: 'Tốt' },
        { id: 5, sessionNumber: 5, date: '18/08', topic: 'Phương trình quy về phương trình bậc hai', homework: '100%', attitude: 'Chăm chỉ' },
        { id: 6, sessionNumber: 6, date: '20/08', topic: 'Định lý Vi-ét và các bài toán tìm tham số m', homework: '90%', attitude: 'Tốt' },
        { id: 7, sessionNumber: 7, date: '25/08', topic: 'Tứ giác nội tiếp và các bổ đề kinh điển', homework: '95%', attitude: 'Tốt' },
        { id: 8, sessionNumber: 8, date: '27/08', topic: 'Kiểm tra định kỳ tháng 8 (Đề thi thử vào 10)', homework: '100%', attitude: 'Tốt' }
      ]
    },
    {
      id: 'lop9a_hs2',
      name: 'Đức Anh',
      shortName: 'ĐA',
      subject: 'Toán',
      grade: 'Lớp Toán 9A',
      ratePerSession: 80000,
      groupType: 'Lớp nhóm',
      className: 'Lớp Toán 9A (Chuyên Đề Vào 10)',
      parentPhone: '0903.888.xxx',
      transferSyntax: 'DUCANH 9A HOCPHI T8',
      isPaid: true,
      parentAcknowledged: true,
      parentNote: 'Gia đình đã gửi học phí rồi ạ, nhờ thầy gửi thêm phiếu bài tập cho cháu.',
      parentNoteTime: 'Hôm qua 18:30',
      aiFeedback: 'Có nhiều tiến bộ so với tháng trước, không còn lơ đễnh trong giờ học nhóm. Bài kiểm tra định kỳ đạt 8.5 điểm.',
      sessions: [
        { id: 1, sessionNumber: 1, date: '04/08', topic: 'Chuyên đề giải bài toán bằng cách lập hệ phương trình', homework: '100%', attitude: 'Tốt' },
        { id: 2, sessionNumber: 2, date: '06/08', topic: 'Luyện tập dạng toán chuyển động và năng suất', homework: '90%', attitude: 'Chăm chỉ' },
        { id: 3, sessionNumber: 3, date: '11/08', topic: 'Hình học đường tròn: Góc nội tiếp & góc tạo bởi tia tiếp tuyến', homework: '85%', attitude: 'Tốt' },
        { id: 4, sessionNumber: 4, date: '13/08', topic: 'Cung bị chắn và góc có đỉnh ở bên trong, bên ngoài đường tròn', homework: '80%', attitude: 'Cần tập trung' },
        { id: 5, sessionNumber: 5, date: '18/08', topic: 'Phương trình quy về phương trình bậc hai', homework: '90%', attitude: 'Tốt' },
        { id: 6, sessionNumber: 6, date: '20/08', topic: 'Định lý Vi-ét và các bài toán tìm tham số m', homework: '85%', attitude: 'Tốt' },
        { id: 7, sessionNumber: 7, date: '25/08', topic: 'Tứ giác nội tiếp và các bổ đề kinh điển', homework: '90%', attitude: 'Tốt' },
        { id: 8, sessionNumber: 8, date: '27/08', topic: 'Kiểm tra định kỳ tháng 8 (Đề thi thử vào 10)', homework: '85%', attitude: 'Tốt' }
      ]
    }
  ];

  selectedStudentId: string = 'duyanh';
  currentStudent!: StudentProfile;

  constructor(private route: ActivatedRoute) {}

  ngOnInit(): void {
    // Read optional student queryParam (e.g. ?student=duyanh)
    this.route.queryParams.subscribe(params => {
      const studentId = params['student'];
      if (studentId && this.students.some(s => s.id === studentId)) {
        this.selectedStudentId = studentId;
      }
      this.loadStudent(this.selectedStudentId);
    });
  }

  onStudentChange(): void {
    this.loadStudent(this.selectedStudentId);
    this.showToast(`Đã chuyển sang hồ sơ: ${this.currentStudent.name}`, 'info');
  }

  loadStudent(id: string): void {
    const found = this.students.find(s => s.id === id);
    if (found) {
      this.currentStudent = found;
      this.selectedStudentId = found.id;
    } else {
      this.currentStudent = this.students[0];
      this.selectedStudentId = this.students[0].id;
    }
  }

  get totalSessions(): number {
    return this.currentStudent.sessions.length;
  }

  get totalHours(): number {
    return Number((this.totalSessions * 2.2).toFixed(1));
  }

  get totalAmount(): number {
    if (this.currentStudent?.fixedTotalAmount !== undefined) {
      return this.currentStudent.fixedTotalAmount;
    }
    return this.totalSessions * this.currentStudent.ratePerSession;
  }

  get formattedTotalAmount(): string {
    return this.formatMoney(this.totalAmount);
  }

  get vietQrUrl(): string {
    const amount = this.totalAmount;
    const addInfo = encodeURIComponent(this.currentStudent.transferSyntax);
    return `https://img.vietqr.io/image/${this.teacherBankCode}-${this.teacherAccountNum}-compact2.png?amount=${amount}&addInfo=${addInfo}`;
  }

  formatMoney(num: number): string {
    return num.toLocaleString('vi-VN') + ' đ';
  }

  simulateBankWebhook(): void {
    this.currentStudent.isPaid = !this.currentStudent.isPaid;
    if (this.currentStudent.isPaid) {
      this.showToast(
        `⚡ Webhook Techcombank khớp thành công! Đã tự động gạch nợ ${this.formattedTotalAmount} cho học sinh ${this.currentStudent.name}.`,
        'success'
      );
    } else {
      this.showToast(`Đã hoàn tác trạng thái thanh toán về Chờ quét mã.`, 'info');
    }
  }

  triggerAI(): void {
    const name = this.currentStudent.name;
    const subject = this.currentStudent.subject;
    const grade = this.currentStudent.grade;
    const sessions = this.currentStudent.sessions.length;

    const templates = [
      `Trong tháng 8 vừa qua, em ${name} tham gia đầy đủ ${sessions} ca học môn ${subject} (${grade}). Em có khả năng tiếp thu bài nhanh, tư duy logic số học tốt. Tuy nhiên trong lúc làm bài tự luận đôi khi còn vội vàng dẫn đến sai sót nhỏ ở các bước biến đổi trung gian. Đề xuất tháng tới tiếp tục củng cố dạng toán nâng cao và rèn tính cẩn trọng.`,
      `Đánh giá tổng quát tháng 8: Em ${name} giữ vững thái độ học tập tích cực, tỷ lệ hoàn thành bài tập về nhà đạt mức xuất sắc. Nắm vững phương pháp giải các dạng bài trọng tâm của ${grade}. Khuyến khích em tự tin xung phong và trao đổi nhiều hơn khi gặp các bài toán thực tế.`,
      `Em ${name} có nhiều nỗ lực đáng ghi nhận trong tháng 8. Đã khắc phục được điểm yếu ở phần kiến thức cơ bản, làm chủ tốt các công thức và định lý. Thầy đề nghị phụ huynh nhắc nhở em duy trì thói quen ôn lại bài trong 15 phút sau mỗi ca dạy để đạt hiệu quả cao nhất.`
    ];

    const randomTemplate = templates[Math.floor(Math.random() * templates.length)];
    this.currentStudent.aiFeedback = randomTemplate;
    this.showToast('✨ AI đã phân tích dữ liệu chuyên cần và sinh nhận xét sư phạm thành công!', 'success');
  }

  openAddSessionModal(): void {
    this.newSessionDate = `${new Date().getDate()}/08`;
    this.isAddSessionModalOpen = true;
  }

  closeAddSessionModal(): void {
    this.isAddSessionModalOpen = false;
  }

  submitAddSession(): void {
    if (!this.newSessionTopic.trim()) {
      this.showToast('Vui lòng nhập nội dung bài học!', 'warning');
      return;
    }

    const nextId = this.currentStudent.sessions.length > 0 
      ? Math.max(...this.currentStudent.sessions.map(s => s.id)) + 1 
      : 1;

    const newSession: SessionItem = {
      id: nextId,
      sessionNumber: this.currentStudent.sessions.length + 1,
      date: this.newSessionDate || '18/08',
      topic: this.newSessionTopic.trim(),
      homework: this.newSessionHw || '100%',
      attitude: this.newSessionAttitude
    };

    this.currentStudent.sessions.push(newSession);
    this.closeAddSessionModal();
    this.showToast(`Đã thêm ca dạy mới! Tổng học phí: ${this.formattedTotalAmount}`, 'success');
  }

  deleteSession(sessionId: number): void {
    const idx = this.currentStudent.sessions.findIndex(s => s.id === sessionId);
    if (idx !== -1) {
      this.currentStudent.sessions.splice(idx, 1);
      this.currentStudent.sessions.forEach((s, i) => s.sessionNumber = i + 1);
      this.showToast(`Đã xóa ca dạy. Tổng tiền đã cập nhật: ${this.formattedTotalAmount}`, 'info');
    }
  }

  parentAcknowledge(): void {
    this.currentStudent.parentAcknowledged = true;
    this.showToast(`👍 Phụ huynh đã xác nhận đã nhận & xem phiếu báo cáo!`, 'success');
  }

  parentSendNote(): void {
    this.parentInputMessage = '';
    this.isParentNoteModalOpen = true;
  }

  closeParentNoteModal(): void {
    this.isParentNoteModalOpen = false;
  }

  submitParentNote(): void {
    if (!this.parentInputMessage.trim()) {
      this.showToast('Vui lòng nhập lời nhắn!', 'warning');
      return;
    }
    this.currentStudent.parentNote = this.parentInputMessage.trim();
    this.currentStudent.parentNoteTime = 'Vừa xong';
    this.currentStudent.parentAcknowledged = true;
    this.closeParentNoteModal();
    this.showToast('Đã lưu phản hồi của phụ huynh vào phiếu báo cáo!', 'success');
  }

  downloadCardAlert(): void {
    this.showToast('📸 Đang kết xuất thẻ học phí PNG chất lượng cao chuẩn VietQR (2K)... Tải xuống thành công!', 'success');
  }

  copyMagicLink(): void {
    const dummyUrl = `https://smarttutor.vn/p/report/${this.currentStudent.id}?token=mgt_${Date.now()}`;
    if (navigator?.clipboard) {
      navigator.clipboard.writeText(dummyUrl).then(() => {
        this.showToast(`🔗 Đã sao chép Magic Link tra cứu không cần mật khẩu cho phụ huynh: ${dummyUrl}`, 'success');
      }).catch(() => {
        this.showToast(`🔗 Link phụ huynh: ${dummyUrl}`, 'info');
      });
    } else {
      this.showToast(`🔗 Link phụ huynh: ${dummyUrl}`, 'info');
    }
  }

  sendZaloNotification(): void {
    this.showToast(
      `💬 Đã tự động gửi thông báo báo cáo & mã VietQR qua Zalo OA đến SĐT phụ huynh (${this.currentStudent.parentPhone})!`,
      'success'
    );
  }

  showToast(message: string, type: 'success' | 'info' | 'warning' = 'success'): void {
    this.toastMessage = message;
    this.toastType = type;
    if (this.toastTimer) clearTimeout(this.toastTimer);
    this.toastTimer = setTimeout(() => {
      this.toastMessage = null;
    }, 4500);
  }

  closeToast(): void {
    this.toastMessage = null;
  }
}
