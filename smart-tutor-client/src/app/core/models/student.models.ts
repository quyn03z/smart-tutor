export interface RequestStudentModel {
  id?: number;
  fullName: string;
  className?: string;
  classType?: string; // 'Individual' | 'Group'
  gradeLevel?: string;
  parentName?: string;
  parentPhone?: string;
  feePerSession?: number;
  classId?: number;
  customFee?: number;
}

export interface StudentsResponseModel {
  id: string;
  fullName: string;
  className?: string;
  classType?: string;
  gradeLevel?: string;
  parentName?: string;
  parentPhone?: string;
  feePerSession?: number;
}
