import { useEffect, useState, useCallback, useMemo } from 'react'
import './TimeAudits.css'

interface TimeAuditEntry {
  id: number
  type: string
  timestamp: string
}

interface ClockStatus {
  clockedIn: boolean
  lastTimestamp?: string
}

interface DayGroup {
  date: string
  entries: TimeAuditEntry[]
  hours: number
}

const MONTH_NAMES = [
  'January', 'February', 'March', 'April', 'May', 'June',
  'July', 'August', 'September', 'October', 'November', 'December',
]

function getAuthHeaders(): Record<string, string> {
  const token = localStorage.getItem('token')
  return {
    'Content-Type': 'application/json',
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  }
}

function calcHoursWorked(entries: TimeAuditEntry[]): number {
  const sorted = [...entries].sort(
    (a, b) => new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime()
  )
  let total = 0
  let clockInTime: number | null = null

  for (const entry of sorted) {
    if (entry.type === 'clock_in') {
      clockInTime = new Date(entry.timestamp).getTime()
    } else if (entry.type === 'clock_out' && clockInTime !== null) {
      total += new Date(entry.timestamp).getTime() - clockInTime
      clockInTime = null
    }
  }

  return total / (1000 * 60 * 60)
}

function formatHours(h: number): string {
  const hrs = Math.floor(h)
  const mins = Math.round((h - hrs) * 60)
  if (hrs === 0 && mins === 0) return '0m'
  if (hrs === 0) return `${mins}m`
  if (mins === 0) return `${hrs}h`
  return `${hrs}h ${mins}m`
}

function TimeAudits() {
  const now = new Date()
  const [year, setYear] = useState(now.getFullYear())
  const [month, setMonth] = useState(now.getMonth() + 1)
  const [audits, setAudits] = useState<TimeAuditEntry[]>([])
  const [status, setStatus] = useState<ClockStatus | null>(null)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const fetchAudits = useCallback(async () => {
    try {
      const res = await fetch(`/api/timeaudits?year=${year}&month=${month}`, {
        headers: getAuthHeaders(),
      })
      if (!res.ok) throw new Error(`HTTP ${res.status}`)
      setAudits(await res.json())
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load audits')
    }
  }, [year, month])

  const fetchStatus = useCallback(async () => {
    try {
      const res = await fetch('/api/timeaudits/status', { headers: getAuthHeaders() })
      if (!res.ok) throw new Error(`HTTP ${res.status}`)
      setStatus(await res.json())
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load status')
    }
  }, [])

  useEffect(() => {
    fetchAudits()
    fetchStatus()
  }, [fetchAudits, fetchStatus])

  const handleClock = async () => {
    setError('')
    setLoading(true)
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
      await fetchAudits()
    } catch {
      setError('Network error')
    } finally {
      setLoading(false)
    }
  }

  const prevMonth = () => {
    if (month === 1) { setMonth(12); setYear(year - 1) }
    else setMonth(month - 1)
  }

  const nextMonth = () => {
    if (month === 12) { setMonth(1); setYear(year + 1) }
    else setMonth(month + 1)
  }

  const dayGroups: DayGroup[] = useMemo(() => {
    const grouped = new Map<string, TimeAuditEntry[]>()
    for (const a of audits) {
      const d = new Date(a.timestamp)
      const key = d.toLocaleDateString([], { weekday: 'short', year: 'numeric', month: 'short', day: 'numeric' })
      if (!grouped.has(key)) grouped.set(key, [])
      grouped.get(key)!.push(a)
    }
    const result: DayGroup[] = []
    for (const [date, entries] of grouped) {
      result.push({ date, entries, hours: calcHoursWorked(entries) })
    }
    return result
  }, [audits])

  const monthTotal = useMemo(() => calcHoursWorked(audits), [audits])

  return (
    <div className="timeaudits">
      <div className="timeaudits-top">
        <div>
          <h1>Time Audits</h1>
          <p>Track and review your clock-in history</p>
        </div>
      </div>

      {error && <p className="ta-error">{error}</p>}

      <div className="ta-cards-row">
        <div className="ta-card ta-clock-card">
          <div className="ta-card-label">Clock Status</div>
          <div className="ta-status-row">
            <div className={`ta-dot ${status?.clockedIn ? 'in' : 'out'}`} />
            <span className="ta-status-text">
              {status ? (status.clockedIn ? 'Clocked In' : 'Clocked Out') : 'Loading...'}
            </span>
          </div>
          <button
            className={`ta-clock-btn ${status?.clockedIn ? 'clock-out' : 'clock-in'}`}
            onClick={handleClock}
            disabled={loading || !status}
          >
            {loading ? 'Processing...' : status?.clockedIn ? 'Clock Out' : 'Clock In'}
          </button>
        </div>

        <div className="ta-card">
          <div className="ta-card-label">Monthly Hours</div>
          <div className="ta-summary-value">{formatHours(monthTotal)}</div>
          <div className="ta-summary-sub">{MONTH_NAMES[month - 1]} {year}</div>
        </div>
      </div>

      <div className="month-nav">
        <button onClick={prevMonth}>&larr;</button>
        <span>{MONTH_NAMES[month - 1]} {year}</span>
        <button onClick={nextMonth}>&rarr;</button>
      </div>

      <div className="ta-audit-section">
        {dayGroups.length === 0 ? (
          <p className="ta-empty">No records for this month</p>
        ) : (
          dayGroups.map(group => (
            <div key={group.date} className="ta-day-group">
              <div className="ta-day-header">
                <span className="ta-day-date">{group.date}</span>
                <span className="ta-day-hours">{formatHours(group.hours)}</span>
              </div>
              {group.entries.map(a => (
                <div key={a.id} className="ta-entry">
                  <span className={`ta-entry-type ${a.type === 'clock_in' ? 'in' : 'out'}`}>
                    {a.type === 'clock_in' ? 'Clock In' : 'Clock Out'}
                  </span>
                  <span className="ta-entry-time">
                    {new Date(a.timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' })}
                  </span>
                </div>
              ))}
            </div>
          ))
        )}
      </div>
    </div>
  )
}

export default TimeAudits
