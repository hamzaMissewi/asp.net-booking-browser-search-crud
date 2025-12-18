import React, { useEffect, useState } from 'react'
import type { Book } from '../types'
import { createBook, deleteBook, listBooks, updateBook } from '../api'

interface BookFormProps {
  initial?: Partial<Book>
  onSubmit: (input: Omit<Book, 'id'>) => void
  onCancel?: () => void
}

function BookForm({ initial, onSubmit, onCancel }: BookFormProps) {
  const [title, setTitle] = useState(initial?.title ?? '')
  const [author, setAuthor] = useState(initial?.author ?? '')
  const [isbn, setIsbn] = useState(initial?.isbn ?? '')
  const [description, setDescription] = useState(initial?.description ?? '')
  const [year, setYear] = useState(initial?.year?.toString() ?? '')
  const [genre, setGenre] = useState(initial?.genre ?? '')

  return (
    <form
      onSubmit={(e) => {
        e.preventDefault()
        onSubmit({ title, author, isbn, description, year: year ? Number(year) : null, genre })
      }}
      style={{
        display: 'grid',
        gap: '12px',
        padding: '16px',
        backgroundColor: '#f9f9f9',
        borderRadius: '8px',
        border: '1px solid #ddd'
      }}
    >
      <input
        placeholder="Title *"
        value={title}
        onChange={(e) => setTitle(e.target.value)}
        required
        style={inputStyle}
      />
      <input
        placeholder="Author *"
        value={author}
        onChange={(e) => setAuthor(e.target.value)}
        required
        style={inputStyle}
      />
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px' }}>
        <input
          placeholder="ISBN"
          value={isbn ?? ''}
          onChange={(e) => setIsbn(e.target.value)}
          style={inputStyle}
        />
        <input
          placeholder="Genre"
          value={genre ?? ''}
          onChange={(e) => setGenre(e.target.value)}
          style={inputStyle}
        />
      </div>
      <input
        placeholder="Year"
        type="number"
        value={year ?? ''}
        onChange={(e) => setYear(e.target.value)}
        style={inputStyle}
      />
      <textarea
        placeholder="Description"
        value={description ?? ''}
        onChange={(e) => setDescription(e.target.value)}
        style={{ ...inputStyle, minHeight: '80px', resize: 'vertical' }}
      />
      <div style={{ display: 'flex', gap: '8px', justifyContent: 'flex-end' }}>
        {onCancel && (
          <button type="button" onClick={onCancel} style={buttonSecondaryStyle}>
            Cancel
          </button>
        )}
        <button type="submit" style={buttonPrimaryStyle}>
          {initial ? 'Update' : 'Create'}
        </button>
      </div>
    </form>
  )
}

export function BooksManager() {
  const [books, setBooks] = useState<Book[]>([])
  const [q, setQ] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [editing, setEditing] = useState<Book | null>(null)
  const [showCreate, setShowCreate] = useState(false)

  const reload = async () => {
    setLoading(true)
    setError(null)
    try {
      const data = await listBooks({ q })
      setBooks(data)
    } catch (e: any) {
      setError(e.message ?? 'Failed to load')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    reload()
  }, [])

  const onCreate = async (input: Omit<Book, 'id'>) => {
    setLoading(true)
    setError(null)
    try {
      await createBook(input)
      setShowCreate(false)
      await reload()
    } catch (e: any) {
      setError(e.message ?? 'Create failed')
    } finally {
      setLoading(false)
    }
  }

  const onUpdate = async (input: Omit<Book, 'id'> & { id: number }) => {
    setLoading(true)
    setError(null)
    try {
      await updateBook(input.id, input)
      setEditing(null)
      await reload()
    } catch (e: any) {
      setError(e.message ?? 'Update failed')
    } finally {
      setLoading(false)
    }
  }

  const onDelete = async (id: number) => {
    if (!confirm('Delete this book?')) return
    setLoading(true)
    setError(null)
    try {
      await deleteBook(id)
      await reload()
    } catch (e: any) {
      setError(e.message ?? 'Delete failed')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
      {error && (
        <div
          style={{
            padding: '12px',
            backgroundColor: '#fee',
            color: '#c33',
            borderRadius: '6px',
            border: '1px solid #fcc'
          }}
        >
          {error}
        </div>
      )}

      {/* Search and Create */}
      <div style={{ display: 'flex', gap: '12px', alignItems: 'center' }}>
        <input
          placeholder="Search books..."
          value={q}
          onChange={(e) => setQ(e.target.value)}
          onKeyDown={(e) => e.key === 'Enter' && reload()}
          style={{ ...inputStyle, flex: 1 }}
        />
        <button onClick={reload} disabled={loading} style={buttonPrimaryStyle}>
          🔍 Search
        </button>
        <button onClick={() => setShowCreate(!showCreate)} style={buttonSuccessStyle}>
          {showCreate ? '✕ Cancel' : '+ Add Book'}
        </button>
      </div>

      {/* Create Form */}
      {showCreate && (
        <BookForm onSubmit={onCreate} onCancel={() => setShowCreate(false)} />
      )}

      {/* Books Table */}
      {loading && <div style={{ textAlign: 'center', padding: '20px' }}>Loading...</div>}
      
      {!loading && books.length === 0 && (
        <div style={{ textAlign: 'center', padding: '40px', color: '#666' }}>
          No books found. Try creating one!
        </div>
      )}

      {!loading && books.length > 0 && (
        <div style={{ overflowX: 'auto' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead>
              <tr style={{ backgroundColor: '#f5f5f5' }}>
                <th style={tableHeaderStyle}>Title</th>
                <th style={tableHeaderStyle}>Author</th>
                <th style={tableHeaderStyle}>Genre</th>
                <th style={tableHeaderStyle}>Year</th>
                <th style={tableHeaderStyle}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {books.map((b) => (
                <React.Fragment key={b.id}>
                  <tr style={{ borderBottom: '1px solid #eee' }}>
                    <td style={tableCellStyle}>{b.title}</td>
                    <td style={tableCellStyle}>{b.author}</td>
                    <td style={tableCellStyle}>{b.genre ?? '-'}</td>
                    <td style={tableCellStyle}>{b.year ?? '-'}</td>
                    <td style={{ ...tableCellStyle, textAlign: 'center' }}>
                      <button
                        onClick={() => setEditing(b)}
                        style={{ ...buttonSmallStyle, marginRight: '8px' }}
                      >
                        ✏️ Edit
                      </button>
                      <button onClick={() => onDelete(b.id)} style={buttonDangerSmallStyle}>
                        🗑️ Delete
                      </button>
                    </td>
                  </tr>
                  {editing?.id === b.id && (
                    <tr>
                      <td colSpan={5} style={{ padding: '16px', backgroundColor: '#fafafa' }}>
                        <BookForm
                          initial={editing}
                          onSubmit={(input) => onUpdate({ ...input, id: editing.id })}
                          onCancel={() => setEditing(null)}
                        />
                      </td>
                    </tr>
                  )}
                </React.Fragment>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}

// Styles
const inputStyle: React.CSSProperties = {
  padding: '10px 12px',
  border: '1px solid #ddd',
  borderRadius: '6px',
  fontSize: '14px',
  outline: 'none',
  transition: 'border-color 0.2s'
}

const buttonPrimaryStyle: React.CSSProperties = {
  padding: '10px 20px',
  backgroundColor: '#667eea',
  color: 'white',
  border: 'none',
  borderRadius: '6px',
  cursor: 'pointer',
  fontWeight: '500',
  fontSize: '14px',
  transition: 'background-color 0.2s'
}

const buttonSecondaryStyle: React.CSSProperties = {
  padding: '10px 20px',
  backgroundColor: '#6c757d',
  color: 'white',
  border: 'none',
  borderRadius: '6px',
  cursor: 'pointer',
  fontWeight: '500',
  fontSize: '14px'
}

const buttonSuccessStyle: React.CSSProperties = {
  padding: '10px 20px',
  backgroundColor: '#28a745',
  color: 'white',
  border: 'none',
  borderRadius: '6px',
  cursor: 'pointer',
  fontWeight: '500',
  fontSize: '14px'
}

const buttonSmallStyle: React.CSSProperties = {
  padding: '6px 12px',
  backgroundColor: '#667eea',
  color: 'white',
  border: 'none',
  borderRadius: '4px',
  cursor: 'pointer',
  fontSize: '12px'
}

const buttonDangerSmallStyle: React.CSSProperties = {
  padding: '6px 12px',
  backgroundColor: '#dc3545',
  color: 'white',
  border: 'none',
  borderRadius: '4px',
  cursor: 'pointer',
  fontSize: '12px'
}

const tableHeaderStyle: React.CSSProperties = {
  padding: '12px',
  textAlign: 'left',
  fontWeight: '600',
  fontSize: '14px',
  borderBottom: '2px solid #ddd'
}

const tableCellStyle: React.CSSProperties = {
  padding: '12px',
  fontSize: '14px'
}
