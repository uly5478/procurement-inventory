import { Outlet, useNavigate, useLocation } from 'react-router-dom'
import { Layout, Menu, Button, theme } from 'antd'
import {
  LogoutOutlined, BulbOutlined, BulbFilled,
  AppstoreOutlined, CalculatorOutlined, FileTextOutlined,
  DatabaseOutlined, LineChartOutlined,
} from '@ant-design/icons'
import useAuthStore from '../store/authStore'

const { Sider, Content, Header } = Layout

const menuItems = [
  { key: '/products', icon: <AppstoreOutlined />, label: '商品管理' },
  { key: '/procurement/suggestions', icon: <CalculatorOutlined />, label: '調達提案' },
  { key: '/procurement/orders', icon: <FileTextOutlined />, label: '発注管理' },
  { key: '/inventory', icon: <DatabaseOutlined />, label: '在庫一覧' },
  { key: '/forecast', icon: <LineChartOutlined />, label: '需要予測' },
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
          調達在庫管理
        </div>
        <Menu
          theme="dark"
          mode="inline"
          selectedKeys={[selectedKey]}
          items={menuItems}
          onClick={({ key }) => navigate(key)}
        />
        <div style={{
          position: 'absolute', bottom: 24, left: 0, right: 0,
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
            {isDark ? 'ライトモード' : 'ダークモード'}
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
          <span style={{ fontWeight: 500 }}>調達発注・在庫管理システム</span>
          <span>
            <span style={{ marginRight: 16, color: colorToken.colorTextSecondary }}>{user?.displayName}</span>
            <Button icon={<LogoutOutlined />} onClick={handleLogout} size="small">ログアウト</Button>
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