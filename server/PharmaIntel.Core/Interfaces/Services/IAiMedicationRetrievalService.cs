// =============================================================================
// Interface: IAiMedicationRetrievalService
// Chuc nang: Retrieval cho RAG Muc 1 - tim thuoc lien quan tu SQL Server theo
//            triệu chứng và lich su chat de inject vao prompt Gemini.
// Quan he:
//   - DiagnosticService goi truoc khi gui prompt cho IDiagnosticEngine.
//   - Implement: AiMedicationRetrievalService (keyword search tren Medications).
// Luu y:
//   - Khong dung embedding/vector. Chi keyword LIKE tren cot Name/GenericName/
//     ActiveIngredients/Benefits/Description.
//   - Tra ve tap thuoc OTC + prescription kem co IsPrescriptionRequired flag de
//     prompt rang buoc Gemini.
// =============================================================================
namespace PharmaIntel.Core.Interfaces.Services;

public interface IAiMedicationRetrievalService
{
    Task<IReadOnlyList<AiMedicationContext>> SearchRelevantMedicationsAsync(
        string symptomsSummary,
        IReadOnlyList<string> conversationMessages,
        CancellationToken ct = default);
}
