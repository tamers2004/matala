import { NavLink, useNavigate } from 'react-router-dom'
import './Layout.css'

interface UserData {
  firstName: string
  lastName: string
  email: string
}

function Layout({ children }: { children: React.ReactNode }) {
  const navigate = useNavigate()
  const stored = localStorage.getItem('user')
  const user: UserData | null = stored ? JSON.parse(stored) : null

  const handleLogout = () => {
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    navigate('/login')
  }

  return (
    <div className="layout">
      <nav className="navbar">
        <div className="navbar-inner">
          <div className="navbar-left">
            <span className="navbar-brand">TimeTracker</span>
            <div className="navbar-links">
              <NavLink to="/" end className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'}>
                Dashboard
              </NavLink>
              <NavLink to="/timeaudits" className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'}>
                Time Audits
              </NavLink>
            </div>
          </div>
          <div className="navbar-right">
            {user && <span className="navbar-user">Hi, {user.firstName}</span>}
            <button className="logout-btn" onClick={handleLogout}>Log Out</button>
          </div>
        </div>
      </nav>
      <main className="layout-content">
        {children}
      </main>
    </div>
  )
}

export default Layout
