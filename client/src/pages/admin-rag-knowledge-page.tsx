import { useEffect, useState } from 'react'
import { AdminPageShell } from '@/features/admin/components/AdminPageShell'
import { ragApi, type KnowledgeDocument } from '@/features/rag/rag-api'

const EMPTY_FORM = {
  title: '',
  sourceType: 'faq',
  sourceUrl: '',
  description: '',
  content: '',
  isActive: true,
}

export function AdminRagKnowledgePage() {
  const [items, setItems] = useState<KnowledgeDocument[]>([])
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [editingId, setEditingId] = useState<number | null>(null)
  const [form, setForm] = useState(EMPTY_FORM)
  const [error, setError] = useState<string | null>(null)

  const load = async () => {
    setLoading(true)
    try {
      setItems(await ragApi.listKnowledge())
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load()
  }, [])

  const resetForm = () => {
    setEditingId(null)
    setForm(EMPTY_FORM)
  }

  const submit = async () => {
    if (!form.title.trim() || !form.content.trim()) {
      setError('Cần nhập tiêu đề và nội dung tài liệu.')
      return
    }

    setSaving(true)
    setError(null)

    try {
      if (editingId) {
        await ragApi.updateKnowledge(editingId, {
          title: form.title,
          sourceType: form.sourceType,
          sourceUrl: form.sourceUrl || null,
          description: form.description || null,
          content: form.content,
          isActive: form.isActive,
        })
      } else {
        await ragApi.ingestKnowledge({
          title: form.title,
          sourceType: form.sourceType,
          sourceUrl: form.sourceUrl || null,
          content: form.content,
        })
      }

      resetForm()
      await load()
    } catch {
      setError('Không lưu được tài liệu.')
    } finally {
      setSaving(false)
    }
  }

  const reindex = async (id: number) => {
    await ragApi.reindexKnowledge(id)
    await load()
  }

  const remove = async (id: number) => {
    if (!confirm('Xóa tài liệu này khỏi knowledge base?')) return
    await ragApi.deleteKnowledge(id)
    await load()
  }

  return (
    <AdminPageShell
      title="Tri thức RAG"
      description="Quản lý tài liệu dùng cho vector search và chatbot AI."
    >
      {error && (
        <div className="mb-4 rounded-xl bg-error-container/40 text-error px-4 py-3 text-sm">
          {error}
        </div>
      )}

      <section className="rounded-2xl border border-outline-variant/40 bg-surface-container/60 p-5 mb-6">
        <h2 className="font-bold text-lg mb-4">
          {editingId ? 'Sửa tài liệu' : 'Thêm tài liệu'}
        </h2>

        <div className="grid gap-3 md:grid-cols-2">
          <input
            className="rounded-xl border border-outline-variant/40 bg-surface px-4 py-2"
            placeholder="Tiêu đề"
            value={form.title}
            onChange={(e) => setForm((f) => ({ ...f, title: e.target.value }))}
          />
          <input
            className="rounded-xl border border-outline-variant/40 bg-surface px-4 py-2"
            placeholder="Source type: faq, guideline, policy..."
            value={form.sourceType}
            onChange={(e) => setForm((f) => ({ ...f, sourceType: e.target.value }))}
          />
          <input
            className="md:col-span-2 rounded-xl border border-outline-variant/40 bg-surface px-4 py-2"
            placeholder="Source URL"
            value={form.sourceUrl}
            onChange={(e) => setForm((f) => ({ ...f, sourceUrl: e.target.value }))}
          />
          <textarea
            className="md:col-span-2 min-h-40 rounded-xl border border-outline-variant/40 bg-surface px-4 py-3"
            placeholder="Nội dung tài liệu để chunk + embedding"
            value={form.content}
            onChange={(e) => setForm((f) => ({ ...f, content: e.target.value }))}
          />
        </div>

        <div className="mt-4 flex gap-2">
          <button
            onClick={submit}
            disabled={saving}
            className="rounded-full bg-primary text-on-primary px-5 py-2 text-sm font-semibold disabled:opacity-60"
          >
            {saving ? 'Đang lưu...' : editingId ? 'Lưu & re-index' : 'Thêm & index'}
          </button>
          {editingId && (
            <button
              onClick={resetForm}
              className="rounded-full border border-outline-variant px-5 py-2 text-sm font-semibold"
            >
              Hủy
            </button>
          )}
        </div>
      </section>

      <section className="rounded-2xl border border-outline-variant/40 bg-surface-container/60 overflow-hidden">
        {loading ? (
          <p className="p-5 text-on-surface-variant">Đang tải...</p>
        ) : (
          <table className="w-full text-sm">
            <thead className="bg-surface-container-high text-on-surface-variant">
              <tr>
                <th className="text-left p-3">Tài liệu</th>
                <th className="text-left p-3">Loại</th>
                <th className="text-left p-3">Chunks</th>
                <th className="text-left p-3">Trạng thái</th>
                <th className="text-right p-3">Thao tác</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-outline-variant/30">
              {items.map((doc) => (
                <tr key={doc.id}>
                  <td className="p-3">
                    <p className="font-semibold">{doc.title}</p>
                    {doc.sourceUrl && (
                      <p className="text-xs text-on-surface-variant truncate">{doc.sourceUrl}</p>
                    )}
                  </td>
                  <td className="p-3">{doc.sourceType}</td>
                  <td className="p-3">{doc.chunkCount}</td>
                  <td className="p-3">{doc.isActive ? 'Active' : 'Disabled'}</td>
                  <td className="p-3">
                    <div className="flex justify-end gap-2">
                      <button
                        onClick={() => reindex(doc.id)}
                        className="text-primary font-semibold"
                      >
                        Re-index
                      </button>
                      <button
                        onClick={() => {
                          setEditingId(doc.id)
                          setForm({
                            title: doc.title,
                            sourceType: doc.sourceType,
                            sourceUrl: doc.sourceUrl ?? '',
                            description: doc.description ?? '',
                            content: '',
                            isActive: doc.isActive,
                          })
                        }}
                        className="text-secondary font-semibold"
                      >
                        Sửa
                      </button>
                      <button
                        onClick={() => remove(doc.id)}
                        className="text-error font-semibold"
                      >
                        Xóa
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </section>
    </AdminPageShell>
  )
}
