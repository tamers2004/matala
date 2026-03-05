import { useEffect, useState, useCallback, useMemo } from 'react'
import { useNavigate } from 'react-router-dom'
import './Dashboard.css'

interface UserData {
  firstName: string
  lastName: string
  email: string
}

interface ClockStatus {
  clockedIn: boolean
  lastTimestamp?: string
}

interface TimeAuditEntry {
  id: number
  type: string
  timestamp: string
}

function getAuthHeaders(): Record<string, string> {
  const token = localStorage.getItem('token')
  return {
    'Content-Type': 'application/json',
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  }
}

function getGreeting(): string {
  const hour = new Date().getHours()
  if (hour < 12) return 'Good morning'
  if (hour < 18) return 'Good afternoon'
  return 'Good evening'
}

function Dashboard() {
  const navigate = useNavigate()
  const stored = localStorage.getItem('user')
  const user: UserData | null = stored ? JSON.parse(stored) : null

  const [now, setNow] = useState(new Date())
  const [status, setStatus] = useState<ClockStatus | null>(null)
  const [todayAudits, setTodayAudits] = useState<TimeAuditEntry[]>([])
  const [error, setError] = useState('')
  const [clocking, setClocking] = useState(false);


  useEffect(() => {

    const interval = setInterval(() => setNow(new Date()), 1000)
    return () => clearInterval(interval)
  }, [])



  const fetchStatus = useCallback(async () => {
    try {
      const res = await fetch('/api/timeaudits/status', { headers: getAuthHeaders() })
      if (!res.ok) throw new Error(`HTTP ${res.status}`)
      setStatus(await res.json())
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load status')
    }
  }, [])

  const fetchTodayAudits = useCallback(async () => {
    const today = new Date()
    const y = today.getFullYear()
    const m = today.getMonth() + 1
    try {
      const res = await fetch(`/api/timeaudits?year=${y}&month=${m}`, { headers: getAuthHeaders() })
      if (!res.ok) throw new Error(`HTTP ${res.status}`)
      const all: TimeAuditEntry[] = await res.json()
      const todayStr = today.toLocaleDateString()
      console.log({all})
      setTodayAudits(all.filter(a => new Date(a.timestamp).toLocaleDateString() === todayStr))
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load audits')
    }
  }, [])


  useEffect(() => {
    fetchStatus()
    fetchTodayAudits()
  }, [fetchStatus, fetchTodayAudits])

  const handleClock = async () => {
    setClocking(true)
    setError('')
    const type = status?.clockedIn ? 'clock_out' : 'clock_in'
    try {
      const res = await fetch('/api/timeaudits', {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify({ type }),
      })
      const data = await res.json()
      if (!res.ok) {
        setError(data.message || 'Clock action failed')
        return
      }
      await fetchStatus()
      await fetchTodayAudits()
    } catch {
      setError('Network error')
    } finally {
      setClocking(false)
    }
  }

  if (!user) {
    navigate('/login')
    return null
  }

  const timeStr = now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' })
  const dateStr = now.toLocaleDateString([], { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })

  return (
    <div className="dashboard">
      <div className="greeting-section">
        <div className="greeting-text">
          <h1>{getGreeting()}, {user.firstName}</h1>
          <p>Here's your overview for today</p>
        </div>
        <div className="live-clock">
          <div className="time">{timeStr}</div>
          <div className="date">{dateStr}</div>
        </div>
      </div>

      {error && <p className="dashboard-error">{error}</p>}

      <div className="cards-row">
        <div className="card status-card">
          <div className="card-label">Clock Status</div>
          <div className="status-display">
            <div className={`status-dot ${status?.clockedIn ? 'clocked-in' : 'clocked-out'}`} />
            <div className="status-info">
              <div className="status-label">
                {status ? (status.clockedIn ? 'Clocked In' : 'Clocked Out') : 'Loading...'}
              </div>
              {status?.lastTimestamp && (
                <div className="status-since">
                  Since {new Date(status.lastTimestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                </div>
              )}
            </div>
          </div>
          <button
            className={`quick-clock-btn ${status?.clockedIn ? 'clock-out' : 'clock-in'}`}
            onClick={handleClock}
            disabled={clocking || !status}
          >
            {clocking ? 'Processing...' : status?.clockedIn ? 'Clock Out' : 'Clock In'}
          </button>
        </div>

        <div className="card">
          <div className="card-label">Today's Summary</div>
          <div style={{ fontSize: '2rem', fontWeight: 700, color: '#111' }}>
            {todayAudits.length}
          </div>
          <div style={{ fontSize: '0.8125rem', color: '#9ca3af' }}>
            {todayAudits.length === 1 ? 'entry' : 'entries'} recorded today
          </div>
        </div>

        <div className="card today-card">
          <div className="card-label">Today's Activity</div>
          {todayAudits.length === 0 ? (
            <p className="today-empty">No activity yet today</p>
          ) : (
            <ul className="today-list">
              {todayAudits.map(a => (
                <li key={a.id} className="today-item">
                  <span className={`today-type ${a.type === 'clock_in' ? 'in' : 'out'}`}>
                    {a.type === 'clock_in' ? 'Clock In' : 'Clock Out'}
                  </span>
                  <span className="today-time">
                    {new Date(a.timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' })}
                  </span>
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>
    </div>
  )
}

export default Dashboard
