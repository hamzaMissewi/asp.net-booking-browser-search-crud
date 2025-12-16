import React, { useEffect, useMemo, useState } from 'react'
import type { Book, AiSearchResult } from './types'
import { aiSearch, createBook, deleteBook, listBooks, updateBook } from './api'

function BookForm({ initial, onSubmit, onCancel }: { initial?: Partial<Book>, onSubmit: (input: Omit<Book, 'id'>) => void, onCancel?: () => void }) {
  const [title, setTitle] = useState(initial?.title ?? '')
  const [author, setAuthor] = useState(initial?.author ?? '')
  const [isbn, setIsbn] = useState(initial?.isbn ?? '')
  const [description, setDescription] = useState(initial?.description ?? '')
  const [year, setYear] = useState(initial?.year?.toString() ?? '')
  const [genre, setGenre] = useState(initial?.genre ?? '')

  return (
    <form onSubmit={(e) => { e.preventDefault(); onSubmit({ title, author, isbn, description, year: year ? Number(year) : null, genre }) }} style={{ display: 'grid', gap: 8, maxWidth: 500 }}>
      <input placeholder="Title" value={title} onChange={e => setTitle(e.target.value)} required />
      <input placeholder="Author" value={author} onChange={e => setAuthor(e.target.value)} required />
      <input placeholder="ISBN" value={isbn ?? ''} onChange={e => setIsbn(e.target.value)} />
      <input placeholder="Genre" value={genre ?? ''} onChange={e => setGenre(e.target.value)} />
      <input placeholder="Year" type="number" value={year ?? ''} onChange={e => setYear(e.target.value)} />
      <textarea placeholder="Description" value={description ?? ''} onChange={e => setDescription(e.target.value)} />
      <div style={{ display: 'flex', gap: 8 }}>
        <button type="submit">Save</button>
        {onCancel && <button type="button" onClick={onCancel}>Cancel</button>}
      </div>
    </form>
  )
}

export default function App() {
  const [books, setBooks] = useState<Book[]>([])
  const [q, setQ] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [editing, setEditing] = useState<Book | null>(null)
  const [aiQuery, setAiQuery] = useState('')
  const [aiResults, setAiResults] = useState<AiSearchResult[] | null>(null)

  const reload = async () => {
    setLoading(true); setError(null)
    try {
      const data = await listBooks({ q })
      setBooks(data)
    } catch (e: any) {
      setError(e.message ?? 'Failed to load')
    } finally { setLoading(false) }
  }

  useEffect(() => { reload() }, [])

  const onCreate = async (input: Omit<Book, 'id'>) => {
    setLoading(true); setError(null)
    try {
      await createBook(input)
      await reload()
    } catch (e: any) { setError(e.message ?? 'Create failed') } finally { setLoading(false) }
  }

  const onUpdate = async (input: Omit<Book, 'id'> & { id: number }) => {
    setLoading(true); setError(null)
    try {
      await updateBook(input.id, input)
      setEditing(null)
      await reload()
    } catch (e: any) { setError(e.message ?? 'Update failed') } finally { setLoading(false) }
  }

  const onDelete = async (id: number) => {
    if (!confirm('Delete this book?')) return
    setLoading(true); setError(null)
    try { await deleteBook(id); await reload() } catch (e: any) { setError(e.message ?? 'Delete failed') } finally { setLoading(false) }
  }

  const doAiSearch = async () => {
    setLoading(true); setError(null)
    try { const res = await aiSearch(aiQuery); setAiResults(res) } catch (e: any) { setError(e.message ?? 'AI search failed') } finally { setLoading(false) }
  }

  const formInitial = useMemo<Partial<Book> | undefined>(() => editing ?? undefined, [editing])

  return (
    <div style={{ fontFamily: 'system-ui, Arial, sans-serif', padding: 16, display: 'grid', gap: 16 }}>
      <h2>Books CRUD + AI Search</h2>
      {error && <div style={{ color: 'crimson' }}>{error}</div>}

      <section style={{ display: 'grid', gap: 8 }}>
        <h3>Create new</h3>
        <BookForm onSubmit={onCreate} />
      </section>

      <section style={{ display: 'grid', gap: 8 }}>
        <h3>List</h3>
        <div style={{ display: 'flex', gap: 8 }}>
          <input placeholder="Search..." value={q} onChange={e => setQ(e.target.value)} />
          <button onClick={reload} disabled={loading}>Search</button>
        </div>
        {loading && <div>Loading...</div>}
        <table style={{ borderCollapse: 'collapse', width: '100%' }}>
          <thead>
            <tr>
              <th style={{ textAlign: 'left', borderBottom: '1px solid #ddd' }}>Title</th>
              <th style={{ textAlign: 'left', borderBottom: '1px solid #ddd' }}>Author</th>
              <th style={{ textAlign: 'left', borderBottom: '1px solid #ddd' }}>Genre</th>
              <th style={{ textAlign: 'left', borderBottom: '1px solid #ddd' }}>Year</th>
              <th style={{ borderBottom: '1px solid #ddd' }}>Actions</th>
            </tr>
          </thead>
          <tbody>
            {books.map(b => (
              <tr key={b.id}>
                <td style={{ borderBottom: '1px solid #eee' }}>{b.title}</td>
                <td style={{ borderBottom: '1px solid #eee' }}>{b.author}</td>
                <td style={{ borderBottom: '1px solid #eee' }}>{b.genre ?? ''}</td>
                <td style={{ borderBottom: '1px solid #eee' }}>{b.year ?? ''}</td>
                <td style={{ borderBottom: '1px solid #eee' }}>
                  <button onClick={() => setEditing(b)}>Edit</button>{' '}
                  <button onClick={() => onDelete(b.id)}>Delete</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        {editing && (
          <div>
            <h4>Edit</h4>
            <BookForm initial={formInitial} onSubmit={(input) => onUpdate({ ...input, id: editing.id })} onCancel={() => setEditing(null)} />
          </div>
        )}
      </section>

      <section style={{ display: 'grid', gap: 8 }}>
        <h3>AI Search</h3>
        <div style={{ display: 'flex', gap: 8 }}>
          <input placeholder="Natural language (e.g., clean code books)" value={aiQuery} onChange={e => setAiQuery(e.target.value)} />
          <button onClick={doAiSearch} disabled={loading || !aiQuery.trim()}>Ask AI</button>
        </div>
        {aiResults && (
          <div>
            <p>Results: {aiResults.length}</p>
            <ul>
              {aiResults.map(r => (
                <li key={r.book.id}>
                  <strong>{r.book.title}</strong> by {r.book.author} — score {r.score.toFixed(2)}
                  {r.reason && <div style={{ color: '#666' }}>{r.reason}</div>}
                </li>
              ))}
            </ul>
          </div>
        )}
      </section>
    </div>
  )
}
