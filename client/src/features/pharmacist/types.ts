export type PendingPrescriptionDocumentQuery = {
  page?: number
  pageSize?: number
}

export type PrescriptionDocumentVerification = {
  id: number
  prescriptionId: number
  userId: number
  userFullName: string | null
  prescriptionTitle: string | null
  fileUrl: string
  verificationStatus: string
  verifiedByPharmacistId: number | null
  notes: string | null
  createdAt: string
  updatedAt: string
}

export type PrescriptionDocumentDecisionRequest = {
  notes?: string | null
}
