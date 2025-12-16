export type Book = {
  id: number;
  title: string;
  author: string;
  isbn?: string | null;
  description?: string | null;
  year?: number | null;
  genre?: string | null;
};

export type AiSearchResult = {
  book: Book;
  score: number;
  reason?: string | null;
};
