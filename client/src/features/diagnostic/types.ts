// =============================================================================
// Diagnostic types - khop voi PharmaIntel.Core/DTOs/Diagnostics + SymptomDto
// =============================================================================

export type Symptom = {
  id: number
  name: string
  groupName: string | null
  displayOrder: number
}

export type DiagnosticMessage = {
  id: number
  sessionId: number
  senderType: 'user' | 'ai' | 'system'
  content: string
  sentAt: string
}

export type DiagnosticSuggestedMedication = {
  id: number
  medicationId: number
  medicationName: string
  price: number
  discountPercent: number
  imageUrl: string | null
  isPrescriptionRequired: boolean
  priority: number
}

export type DiagnosticRiskLevel = 'low' | 'medium' | 'high' | 'emergency'

export type DiagnosticResult = {
  id: number
  sessionId: number
  aiConclusion: string | null
  confidenceScore: number
  riskLevel: DiagnosticRiskLevel
  redFlags: string | null
  requiresDoctorVisit: boolean
  modelName: string | null
  modelVersion: string | null
  diagnosedAt: string
  suggestedMedications: DiagnosticSuggestedMedication[]
}

export type DiagnosticSessionListItem = {
  id: number
  status: string
  messageCount: number
  symptoms: string[]
  createdAt: string
  completedAt: string | null
}

export type DiagnosticSession = DiagnosticSessionListItem & {
  messages: DiagnosticMessage[]
  result: DiagnosticResult | null
}

export type CreateDiagnosticSessionRequest = {
  symptomIds: number[]
  initialMessage?: string
}
