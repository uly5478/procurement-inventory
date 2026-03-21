import { Outlet, useNavigate, useLocation } from 'react-router-dom'
import { Layout, Menu, Button } from 'antd'
import { LogoutOutlined } from '@ant-design/icons'
import useAuthStore from '../store/authStore'
import {
  AppstoreOutlined,
  CalculatorOutlined,
  FileTextOutlined,
  DatabaseOutlined,
  ImportOutlined,
  ExportOutlined,
  LineChartOutlined,
} from '@ant-design/icons'

const { Sider, Content, Header } = Layout

const menuItems = [
  {
    key: '/products',
    icon: <AppstoreOutlined />,
    label: '產品管理',
  },
  {
    key: '/procurement/suggestions',
    icon: <CalculatorOutlined />,
    label: '採購建議',
  },
  {
    key: '/procurement/orders',
    icon: <FileTextOutlined />,
    label: '採購訂單',
  },
  {
    key: '/inventory',
    icon: <DatabaseOutlined />,
    label: '庫存總覽',
  },
  {
    key: '/inventory/stock-in',
    icon: <ImportOutlined />,
    label: '入庫作業',
  },
  {
    key: '/inventory/stock-out',
    icon: <ExportOutlined />,
    label: '出貨作業',
  },
  {
    key: '/forecast',
    icon: <LineChartOutlined />,
    label: '需求預測',
  },
]

export default function MainLayout() {
  const navigate = useNavigate()
  const { pathname } = useLocation()
  const { user, clearToken } = useAuthStore()

  const handleLogout = () => {
    clearToken()
    navigate('/login', { replace: true })
  }

  // Match the most specific menu key
  const selectedKey =
    menuItems
      .map((item) => item.key)
      .filter((key) => pathname.startsWith(key))
      .sort((a, b) => b.length - a.length)[0] ?? '/products'

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider width={200} theme="dark">
        <div
          style={{
            height: 48,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: '#fff',
            fontWeight: 'bold',
            fontSize: 14,
            borderBottom: '1px solid rgba(255,255,255,0.1)',
          }}
        >
          採購庫存管理
        </div>
        <Menu
          theme="dark"
          mode="inline"
          selectedKeys={[selectedKey]}
          items={menuItems}
          onClick={({ key }) => navigate(key)}
        />
      </Sider>
      <Layout>
        <Header style={{ background: '#fff', padding: '0 24px', borderBottom: '1px solid #f0f0f0', display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
          <span style={{ fontWeight: 500 }}>採購下單及庫存管理系統</span>
          <span>
            <span style={{ marginRight: 16, color: '#666' }}>{user?.displayName}</span>
            <Button icon={<LogoutOutlined />} onClick={handleLogout} size="small">登出</Button>
          </span>
        </Header>
        <Content style={{ margin: 24, background: '#fff', padding: 24, borderRadius: 8 }}>
          <Outlet />
        </Content>
      </Layout>
    </Layout>
  )
}
