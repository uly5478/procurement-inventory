import { Form, Input, Button, Card, message } from 'antd'
import { UserOutlined, LockOutlined } from '@ant-design/icons'
import { useNavigate } from 'react-router-dom'
import { login } from '../../api/auth'
import useAuthStore from '../../store/authStore'

export default function LoginPage() {
  const navigate = useNavigate()
  const setToken = useAuthStore((s) => s.setToken)
  const [form] = Form.useForm()

  const handleSubmit = async (values: { username: string; password: string }) => {
    try {
      const data = await login(values.username, values.password)
      setToken(data.token, { account: data.username, displayName: data.displayName })
      navigate('/', { replace: true })
    } catch {
      message.error('アカウントまたはパスワードが正しくありません')
    }
  }

  return (
    <div style={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center', background: '#f0f2f5' }}>
      <Card title="調達発注・在庫管理システム" style={{ width: 360 }}>
        <Form form={form} onFinish={handleSubmit} layout="vertical">
          <Form.Item name="username" rules={[{ required: true, message: 'アカウントを入力してください' }]}>
            <Input prefix={<UserOutlined />} placeholder="アカウント" />
          </Form.Item>
          <Form.Item name="password" rules={[{ required: true, message: 'パスワードを入力してください' }]}>
            <Input.Password prefix={<LockOutlined />} placeholder="パスワード" />
          </Form.Item>
          <Form.Item>
            <Button type="primary" htmlType="submit" block>ログイン</Button>
          </Form.Item>
        </Form>
      </Card>
    </div>
  )
}