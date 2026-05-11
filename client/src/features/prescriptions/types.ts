export type PrescriptionVerificationStatus =
  | 'not_required'
  | 'pending'
  | 'verified'
  | 'rejected'

export type PrescriptionStatus =
  | 'draft'
  | 'active'
  | 'completed'
  | 'expired'
  | 'cancelled'

export type PrescriptionListItem = {
  id: number
  doctorId: number | null
  doctorNameSnapshot: string | null
  title: string | null
  prescribedDate: string | null
  status: PrescriptionStatus | string
  verificationStatus: PrescriptionVerificationStatus | string
  itemCount: number
  createdAt: string
  updatedAt: string
}

export type PrescriptionItem = {
  id: number
  prescriptionId: number
  medicationId: number | null
  medicationName: string
  dosage: string | null
  frequency: string | null
  duration: string | null
}

export type Prescription = PrescriptionListItem & {
  items: PrescriptionItem[]
}

export type PrescriptionDocument = {
  id: number
  prescriptionId: number
  fileUrl: string
  verificationStatus: 'pending' | 'verified' | 'rejected' | string
  verifiedByPharmacistId: number | null
  notes: string | null
  createdAt: string
  updatedAt: string
}

export type PrescriptionCreateRequest = {
  doctorId?: number | null
  doctorNameSnapshot?: string | null
  title?: string | null
  prescribedDate?: string | null
}

export type PrescriptionUpdateRequest = PrescriptionCreateRequest & {
  status: PrescriptionStatus | string
}

export type PrescriptionItemCreateRequest = {
  medicationId?: number | null
  medicationName?: string | null
  dosage?: string | null
  frequency?: string | null
  duration?: string | null
}
