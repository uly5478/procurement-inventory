import { Outlet, useNavigate, useLocation } from 'react-router-dom'
import { Layout, Menu, Button, theme } from 'antd'
import {
  LogoutOutlined, BulbOutlined, BulbFilled,
  AppstoreOutlined, CalculatorOutlined, FileTextOutlined,
  DatabaseOutlined, ImportOutlined, ExportOutlined, LineChartOutlined,
} from '@ant-design/icons'
import useAuthStore from '../store/authStore'

const { Sider, Content, Header } = Layout

const menuItems = [
  { key: '/products', icon: <AppstoreOutlined />, label: '產品管理' },
  { key: '/procurement/suggestions', icon: <CalculatorOutlined />, label: '採購建議' },
  { key: '/procurement/orders', icon: <FileTextOutlined />, label: '採購訂單' },
  { key: '/inventory', icon: <DatabaseOutlined />, label: '庫存總覽' },
  { key: '/inventory/stock-in', icon: <ImportOutlined />, label: '入庫作業' },
  { key: '/inventory/stock-out', icon: <ExportOutlined />, label: '出貨作業' },
  { key: '/forecast', icon: <LineChartOutlined />, label: '需求預測' },
]

interface MainLayoutProps {
  isDark: boolean
  toggleTheme: () => void
}

export default function MainLayout({ isDark, toggleTheme }: MainLayoutProps) {
  const navigate = useNavigate()
  const { pathname } = useLocation()
  const { user, clearToken } = useAuthStore()
  const { token: colorToken } = theme.useToken()

  const handleLogout = () => {
    clearToken()
    navigate('/login', { replace: true })
  }

  const selectedKey =
    menuItems
      .map((item) => item.key)
      .filter((key) => pathname.startsWith(key))
      .sort((a, b) => b.length - a.length)[0] ?? '/products'

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider width={200} theme="dark" style={{ position: 'relative' }}>
        <div style={{
          height: 48, display: 'flex', alignItems: 'center', justifyContent: 'center',
          color: '#fff', fontWeight: 'bold', fontSize: 14,
          borderBottom: '1px solid rgba(255,255,255,0.1)',
        }}>
          採購庫存管理
        </div>
        <Menu
          theme="dark"
          mode="inline"
          selectedKeys={[selectedKey]}
          items={menuItems}
          onClick={({ key }) => navigate(key)}
        />
        {/* 主題切換 — 絕對定位在 Sider 最底部 */}
        <div style={{
          position: 'absolute', bottom: 0, left: 0, right: 0,
          padding: '12px 16px',
          borderTop: '1px solid rgba(255,255,255,0.1)',
          background: '#001529',
        }}>
          <Button
            block
            icon={isDark ? <BulbFilled /> : <BulbOutlined />}
            onClick={toggleTheme}
            size="small"
            style={{ background: 'transparent', color: '#fff', borderColor: 'rgba(255,255,255,0.3)' }}
          >
            {isDark ? '淺色模式' : '深色模式'}
          </Button>
        </div>
      </Sider>
      <Layout>
        <Header style={{
          background: colorToken.colorBgContainer,
          padding: '0 24px',
          borderBottom: `1px solid ${colorToken.colorBorderSecondary}`,
          display: 'flex', alignItems: 'center', justifyContent: 'space-between',
        }}>
          <span style={{ fontWeight: 500 }}>採購下單及庫存管理系統</span>
          <span>
            <span style={{ marginRight: 16, color: colorToken.colorTextSecondary }}>{user?.displayName}</span>
            <Button icon={<LogoutOutlined />} onClick={handleLogout} size="small">登出</Button>
          </span>
        </Header>
        <Content style={{
          margin: 24,
          background: colorToken.colorBgContainer,
          padding: 24,
          borderRadius: 8,
        }}>
          <Outlet />
        </Content>
      </Layout>
    </Layout>
  )
}
