import { useState, useEffect } from 'react'
import { Form, Select, InputNumber, DatePicker, Input, Button, message, Card, Modal } from 'antd'
import { WarningOutlined } from '@ant-design/icons'
import { useNavigate } from 'react-router-dom'
import dayjs from 'dayjs'
import { stockOut } from '../../api/inventory'
import { getProducts } from '../../api/products'
import type { Product } from '../../types'

export default function StockOutPage() {
  const [form] = Form.useForm()
  const [saving, setSaving] = useState(false)
  const [products, setProducts] = useState<Product[]>([])
  const navigate = useNavigate()

  useEffect(() => {
    getProducts(undefined, true).then(setProducts).catch(() => {})
  }, [])

  async function handleSubmit(forceConfirm = false) {
    const values = await form.validateFields()
    setSaving(true)
    try {
      const result = await stockOut({
        productId: values.productId,
        quantity: values.quantity,
        transactionDate: (values.transactionDate as dayjs.Dayjs).toISOString(),
        remark: values.remark,
        forceConfirm,
      })

      if (result.requireConfirmation) {
        Modal.confirm({
          title: '在庫不足の警告',
          icon: <WarningOutlined style={{ color: '#faad14' }} />,
          content: result.warning,
          okText: '出荷する',
          cancelText: 'キャンセル',
          onOk: () => handleSubmit(true),
        })
        return
      }

      message.success('出荷が完了しました')
      navigate('/inventory')
    } catch {
      message.error('出荷に失敗しました')
    } finally {
      setSaving(false)
    }
  }

  return (
    <div style={{ padding: 24, maxWidth: 600 }}>
      <Card title="出荷作業">
        <Form form={form} layout="vertical" initialValues={{ transactionDate: dayjs() }}>
          <Form.Item
            label="商品"
            name="productId"
            rules={[{ required: true, message: '商品を選択してください' }]}
          >
            <Select
              placeholder="商品を選択"
              showSearch
              optionFilterProp="label"
              options={products.map((p) => ({
                value: p.id,
                label: `${p.productCode} ${p.name}`,
              }))}
            />
          </Form.Item>

          <Form.Item
            label="出荷数量"
            name="quantity"
            rules={[
              { required: true, message: '数量を入力してください' },
              { type: 'number', min: 1, message: '数量は1以上で入力してください' },
            ]}
          >
            <InputNumber min={1} style={{ width: '100%' }} />
          </Form.Item>

          <Form.Item
            label="出荷日"
            name="transactionDate"
            rules={[{ required: true, message: '日付を選択してください' }]}
          >
            <DatePicker showTime style={{ width: '100%' }} />
          </Form.Item>

          <Form.Item label="備考" name="remark">
            <Input.TextArea rows={2} maxLength={500} />
          </Form.Item>

          <Form.Item>
            <Button type="primary" onClick={() => handleSubmit(false)} loading={saving}>
              出荷確認
            </Button>
            <Button style={{ marginLeft: 8 }} onClick={() => navigate('/inventory')}>
              キャンセル
            </Button>
          </Form.Item>
        </Form>
      </Card>
    </div>
  )
}