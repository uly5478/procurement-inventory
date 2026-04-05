import { useState, useEffect } from 'react'
import { Form, Select, InputNumber, DatePicker, Input, Button, message, Card } from 'antd'
import { useNavigate } from 'react-router-dom'
import dayjs from 'dayjs'
import { stockIn } from '../../api/inventory'
import { getProducts } from '../../api/products'
import { getPurchaseOrders } from '../../api/purchaseOrders'
import type { Product, PurchaseOrder } from '../../types'

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
      message.success('入庫が完了しました')
      navigate('/inventory')
    } catch {
      message.error('入庫に失敗しました')
    } finally {
      setSaving(false)
    }
  }

  return (
    <div style={{ padding: 24, maxWidth: 600 }}>
      <Card title="入庫作業">
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
            label="入庫数量"
            name="quantity"
            rules={[
              { required: true, message: '数量を入力してください' },
              { type: 'number', min: 1, message: '数量は1以上で入力してください' },
            ]}
          >
            <InputNumber min={1} style={{ width: '100%' }} />
          </Form.Item>

          <Form.Item
            label="入庫日"
            name="transactionDate"
            rules={[{ required: true, message: '日付を選択してください' }]}
          >
            <DatePicker showTime style={{ width: '100%' }} />
          </Form.Item>

          <Form.Item label="対応する発注（任意）" name="purchaseOrderId">
            <Select
              placeholder="発注を選択"
              allowClear
              showSearch
              optionFilterProp="label"
              options={orders.map((o) => ({
                value: o.id,
                label: `${o.orderNumber} - ${o.supplierName}`,
              }))}
            />
          </Form.Item>

          <Form.Item label="備考" name="remark">
            <Input.TextArea rows={2} maxLength={500} />
          </Form.Item>

          <Form.Item>
            <Button type="primary" onClick={handleSubmit} loading={saving}>
              入庫確認
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