import { useState, useEffect } from 'react'
import { Form, Select, InputNumber, DatePicker, Input, Button, message, Card } from 'antd'
import { useNavigate } from 'react-router-dom'
import dayjs from 'dayjs'
import { stockIn } from '../../api/inventory'
import { getProducts } from '../../api/products'
import { getPurchaseOrders } from '../../api/purchaseOrders'
import type { Product } from '../../types'
import type { PurchaseOrder } from '../../types'

export default function StockInPage() {
  const [form] = Form.useForm()
  const [saving, setSaving] = useState(false)
  const [products, setProducts] = useState<Product[]>([])
  const [orders, setOrders] = useState<PurchaseOrder[]>([])
  const navigate = useNavigate()

  useEffect(() => {
    getProducts(undefined, true).then(setProducts).catch(() => {})
    getPurchaseOrders({ status: '已確認' }).then(setOrders).catch(() => {})
  }, [])

  async function handleSubmit() {
    const values = await form.validateFields()
    setSaving(true)
    try {
      await stockIn({
        productId: values.productId,
        quantity: values.quantity,
        transactionDate: (values.transactionDate as dayjs.Dayjs).toISOString(),
        purchaseOrderId: values.purchaseOrderId,
        remark: values.remark,
      })
      message.success('入庫成功')
      navigate('/inventory')
    } catch {
      message.error('入庫失敗，請稍後再試')
    } finally {
      setSaving(false)
    }
  }

  return (
    <div style={{ padding: 24, maxWidth: 600 }}>
      <Card title="入庫作業">
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
            label="入庫數量"
            name="quantity"
            rules={[
              { required: true, message: '請輸入數量' },
              { type: 'number', min: 1, message: '數量必須大於 0' },
            ]}
          >
            <InputNumber min={1} style={{ width: '100%' }} />
          </Form.Item>

          <Form.Item
            label="入庫日期"
            name="transactionDate"
            rules={[{ required: true, message: '請選擇日期' }]}
          >
            <DatePicker showTime style={{ width: '100%' }} />
          </Form.Item>

          <Form.Item label="對應採購訂單（選填）" name="purchaseOrderId">
            <Select
              placeholder="選擇採購訂單"
              allowClear
              showSearch
              optionFilterProp="label"
              options={orders.map((o) => ({
                value: o.id,
                label: `${o.orderNumber} - ${o.supplierName}`,
              }))}
            />
          </Form.Item>

          <Form.Item label="備註" name="remark">
            <Input.TextArea rows={2} maxLength={500} />
          </Form.Item>

          <Form.Item>
            <Button type="primary" onClick={handleSubmit} loading={saving}>
              確認入庫
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
