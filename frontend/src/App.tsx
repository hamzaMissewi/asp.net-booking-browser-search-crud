import React, { useState } from 'react'
import { Chatbot } from './components/Chatbot'
import { BooksManager } from './components/BooksManager'

export default function App() {
  const [activeTab, setActiveTab] = useState<'chat' | 'manage'>('chat')

  return (
    <div style={{
      fontFamily: 'system-ui, -apple-system, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif',
      minHeight: '100vh',
      backgroundColor: '#f5f7fa'
    }}>
      {/* Header */}
      <header style={{
        background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
        color: 'white',
        padding: '24px',
        boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
      }}>
        <div style={{ maxWidth: '1200px', margin: '0 auto' }}>
          <h1 style={{ margin: 0, fontSize: '28px', fontWeight: '700' }}>
            📚 Books Management System
          </h1>
          <p style={{ margin: '8px 0 0 0', opacity: 0.9, fontSize: '14px' }}>
            AI-powered book discovery and management
          </p>
        </div>
      </header>

      {/* Navigation Tabs */}
      <div style={{
        borderBottom: '1px solid #ddd',
        backgroundColor: 'white',
        position: 'sticky',
        top: 0,
        zIndex: 10,
        boxShadow: '0 2px 4px rgba(0,0,0,0.05)'
      }}>
        <div style={{
          maxWidth: '1200px',
          margin: '0 auto',
          display: 'flex',
          gap: '4px',
          padding: '0 24px'
        }}>
          <button
            onClick={() => setActiveTab('chat')}
            style={{
              padding: '16px 24px',
              border: 'none',
              backgroundColor: 'transparent',
              borderBottom: activeTab === 'chat' ? '3px solid #667eea' : '3px solid transparent',
              color: activeTab === 'chat' ? '#667eea' : '#666',
              fontWeight: activeTab === 'chat' ? '600' : '400',
              cursor: 'pointer',
              fontSize: '15px',
              transition: 'all 0.2s'
            }}
          >
            💬 AI Chat Assistant
          </button>
          <button
            onClick={() => setActiveTab('manage')}
            style={{
              padding: '16px 24px',
              border: 'none',
              backgroundColor: 'transparent',
              borderBottom: activeTab === 'manage' ? '3px solid #667eea' : '3px solid transparent',
              color: activeTab === 'manage' ? '#667eea' : '#666',
              fontWeight: activeTab === 'manage' ? '600' : '400',
              cursor: 'pointer',
              fontSize: '15px',
              transition: 'all 0.2s'
            }}
          >
            📖 Manage Books
          </button>
        </div>
      </div>

      {/* Main Content */}
      <main style={{
        maxWidth: '1200px',
        margin: '0 auto',
        padding: '32px 24px'
      }}>
        {activeTab === 'chat' && (
          <div>
            <div style={{ marginBottom: '20px' }}>
              <h2 style={{ margin: '0 0 8px 0', fontSize: '24px', fontWeight: '600' }}>
                AI Book Assistant
              </h2>
              <p style={{ margin: 0, color: '#666', fontSize: '14px' }}>
                Ask me anything about books! I can help you find recommendations, search for specific titles, or chat about what you'd like to read.
              </p>
            </div>
            <Chatbot />
          </div>
        )}

        {activeTab === 'manage' && (
          <div>
            <div style={{ marginBottom: '20px' }}>
              <h2 style={{ margin: '0 0 8px 0', fontSize: '24px', fontWeight: '600' }}>
                Manage Your Library
              </h2>
              <p style={{ margin: 0, color: '#666', fontSize: '14px' }}>
                Create, edit, search, and organize your book collection.
              </p>
            </div>
            <BooksManager />
          </div>
        )}
      </main>

      {/* Footer */}
      <footer style={{
        borderTop: '1px solid #ddd',
        backgroundColor: 'white',
        padding: '24px',
        marginTop: '48px',
        textAlign: 'center',
        color: '#666',
        fontSize: '14px'
      }}>
        <div style={{ maxWidth: '1200px', margin: '0 auto' }}>
          <p style={{ margin: 0 }}>
            Built with ASP.NET Core + React + OpenAI
          </p>
        </div>
      </footer>
    </div>
  )
}
