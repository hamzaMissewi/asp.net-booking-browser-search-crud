import type { Book, AiSearchResult } from './types'

const base = '' // proxied via Vite to backend

export async function listBooks(params?: { q?: string; genre?: string; page?: number; pageSize?: number }): Promise<Book[]> {
  const url = new URL('/api/books', window.location.origin)
  if (params?.q) url.searchParams.set('q', params.q)
  if (params?.genre) url.searchParams.set('genre', params.genre)
  if (params?.page) url.searchParams.set('page', String(params.page))
  if (params?.pageSize) url.searchParams.set('pageSize', String(params.pageSize))
  const res = await fetch(url.pathname + url.search)
  if (!res.ok) throw new Error('Failed to load books')
  return res.json()
}

export async function getBook(id: number): Promise<Book> {
  const res = await fetch(`/api/books/${id}`)
  if (!res.ok) throw new Error('Book not found')
  return res.json()
}

export async function createBook(input: Omit<Book, 'id'>): Promise<Book> {
  const res = await fetch('/api/books', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(input) })
  if (!res.ok) throw new Error('Create failed')
  return res.json()
}

export async function updateBook(id: number, input: Omit<Book, 'id'>): Promise<Book> {
  const res = await fetch(`/api/books/${id}`, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ ...input, id }) })
  if (!res.ok) throw new Error('Update failed')
  return res.json()
}

export async function deleteBook(id: number): Promise<void> {
  const res = await fetch(`/api/books/${id}`, { method: 'DELETE' })
  if (!res.ok) throw new Error('Delete failed')
}

export async function aiSearch(query: string): Promise<AiSearchResult[]> {
  const res = await fetch('/api/ai/search', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ query }) })
  if (!res.ok) throw new Error('AI search failed')
  return res.json()
}
