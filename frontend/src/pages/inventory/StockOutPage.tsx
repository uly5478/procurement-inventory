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

      // 超庫存警告，需要使用者確認
      if (result.requireConfirmation) {
        Modal.confirm({
          title: '庫存不足警告',
          icon: <WarningOutlined style={{ color: '#faad14' }} />,
          content: result.warning,
          okText: '確認出貨',
          cancelText: '取消',
          onOk: () => handleSubmit(true),
        })
        return
      }

      message.success('出貨成功')
      navigate('/inventory')
    } catch {
      message.error('出貨失敗，請稍後再試')
    } finally {
      setSaving(false)
    }
  }

  return (
    <div style={{ padding: 24, maxWidth: 600 }}>
      <Card title="出貨作業">
        <Form form={form} layout="vertical" initialValues={{ transactionDate: dayjs() }}>
          <Form.Item
            label="產品"
            name="productId"
            rules={[{ required: true, message: '請選擇產品' }]}
          >
            <Select
              placeholder="選擇產品"
              showSearch
              optionFilterProp="label"
              options={products.map((p) => ({
                value: p.id,
                label: `${p.productCode} ${p.name}`,
              }))}
            />
          </Form.Item>

          <Form.Item
            label="出貨數量"
            name="quantity"
            rules={[
              { required: true, message: '請輸入數量' },
              { type: 'number', min: 1, message: '數量必須大於 0' },
            ]}
          >
            <InputNumber min={1} style={{ width: '100%' }} />
          </Form.Item>

          <Form.Item
            label="出貨日期"
            name="transactionDate"
            rules={[{ required: true, message: '請選擇日期' }]}
          >
            <DatePicker showTime style={{ width: '100%' }} />
          </Form.Item>

          <Form.Item label="備註" name="remark">
            <Input.TextArea rows={2} maxLength={500} />
          </Form.Item>

          <Form.Item>
            <Button type="primary" onClick={() => handleSubmit(false)} loading={saving}>
              確認出貨
            </Button>
            <Button style={{ marginLeft: 8 }} onClick={() => navigate('/inventory')}>
              取消
            </Button>
          </Form.Item>
        </Form>
      </Card>
    </div>
  )
}
