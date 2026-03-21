import { useState, useEffect, useCallback } from 'react'
import {
  Table,
  Button,
  Space,
  Modal,
  Form,
  Select,
  InputNumber,
  DatePicker,
  Input,
  message,
  Tag,
  Typography,
  Divider,
} from 'antd'
import { PlusOutlined, DeleteOutlined } from '@ant-design/icons'
import type { ColumnsType } from 'antd/es/table'
import dayjs from 'dayjs'
import { getPurchaseOrders, createPurchaseOrder } from '../../api/purchaseOrders'
import { getProducts } from '../../api/products'
import { getProductSuppliers } from '../../api/suppliers'
import type { PurchaseOrder, Product, ProductSupplierPrice } from '../../types'

const { Text } = Typography
const { RangePicker } = DatePicker

// 訂單狀態對應顏色
const STATUS_COLOR: Record<string, string> = {
  待確認: 'orange',
  已確認: 'blue',
  已收貨: 'green',
  已取消: 'red',
}

interface OrderItemForm {
  productId?: number
  quantity?: number
  unitPrice?: number
}

export default function OrdersPage() {
  const [orders, setOrders] = useState<PurchaseOrder[]>([])
  const [loading, setLoading] = useState(false)
  const [modalOpen, setModalOpen] = useState(false)
  const [saving, setSaving] = useState(false)

  // 篩選條件
  const [dateRange, setDateRange] = useState<[dayjs.Dayjs | null, dayjs.Dayjs | null] | null>(null)
  const [filterSupplier, setFilterSupplier] = useState<string>('')
  const [filterStatus, setFilterStatus] = useState<string>('')

  // 新增訂單表單
  const [form] = Form.useForm()
  const [products, setProducts] = useState<Product[]>([])
  const [suppliers, setSuppliers] = useState<ProductSupplierPrice[]>([])
  const [, setSelectedSupplierId] = useState<number | null>(null)
  const [items, setItems] = useState<OrderItemForm[]>([{}])

  const fetchOrders = useCallback(async () => {
    setLoading(true)
    try {
      const params: Record<string, string> = {}
      if (dateRange?.[0]) params.startDate = dateRange[0].toISOString()
      if (dateRange?.[1]) params.endDate = dateRange[1].toISOString()
      if (filterSupplier) params.supplierName = filterSupplier
      if (filterStatus) params.status = filterStatus
      const data = await getPurchaseOrders(params)
      setOrders(data)
    } catch {
      message.error('載入採購訂單失敗')
    } finally {
      setLoading(false)
    }
  }, [dateRange, filterSupplier, filterStatus])

  useEffect(() => {
    fetchOrders()
  }, [fetchOrders])

  async function openCreateModal() {
    form.resetFields()
    setItems([{}])
    setSelectedSupplierId(null)
    setSuppliers([])
    // 載入產品清單
    try {
      const data = await getProducts(undefined, true)
      setProducts(data)
    } catch {
      message.error('載入產品清單失敗')
    }
    setModalOpen(true)
  }

  async function handleSupplierProductChange(productId: number) {
    // 當選擇產品時，載入該產品的廠商報價供選擇
    try {
      const result = await getProductSuppliers(productId)
      setSuppliers(result.items.filter((s) => s.isCurrent))
    } catch {
      setSuppliers([])
    }
  }

  function updateItem(index: number, field: keyof OrderItemForm, value: number | undefined) {
    setItems((prev) => {
      const next = [...prev]
      next[index] = { ...next[index], [field]: value }
      return next
    })
  }

  function addItem() {
    setItems((prev) => [...prev, {}])
  }

  function removeItem(index: number) {
    setItems((prev) => prev.filter((_, i) => i !== index))
  }

  async function handleCreate() {
    await form.validateFields()
    const supplierId = form.getFieldValue('supplierId')
    if (!supplierId) {
      message.error('請選擇廠商')
      return
    }
    const validItems = items.filter(
      (i) => i.productId && i.quantity && i.quantity > 0 && i.unitPrice && i.unitPrice > 0,
    )
    if (validItems.length === 0) {
      message.error('請至少新增一筆有效的採購明細')
      return
    }

    setSaving(true)
    try {
      await createPurchaseOrder({
        supplierId,
        items: validItems.map((i) => ({
          productId: i.productId!,
          quantity: i.quantity!,
          unitPrice: i.unitPrice!,
        })),
      })
      message.success('採購訂單已建立')
      setModalOpen(false)
      fetchOrders()
    } catch {
      message.error('建立採購訂單失敗')
    } finally {
      setSaving(false)
    }
  }

  const columns: ColumnsType<PurchaseOrder> = [
    {
      title: '訂單編號',
      dataIndex: 'orderNumber',
      key: 'orderNumber',
      width: 180,
    },
    {
      title: '廠商',
      dataIndex: 'supplierName',
      key: 'supplierName',
    },
    {
      title: '狀態',
      dataIndex: 'status',
      key: 'status',
      width: 100,
      render: (status: string) => (
        <Tag color={STATUS_COLOR[status] ?? 'default'}>{status}</Tag>
      ),
    },
    {
      title: '總金額',
      dataIndex: 'totalAmount',
      key: 'totalAmount',
      width: 130,
      align: 'right',
      render: (val: number) => val.toLocaleString('zh-TW', { minimumFractionDigits: 2 }),
    },
    {
      title: '訂單日期',
      dataIndex: 'orderDate',
      key: 'orderDate',
      width: 160,
      render: (val: string) => dayjs(val).format('YYYY-MM-DD HH:mm'),
    },
    {
      title: '建立者',
      dataIndex: 'createdBy',
      key: 'createdBy',
      width: 120,
    },
  ]

  // 計算明細小計
  const totalAmount = items.reduce((sum, i) => sum + (i.quantity ?? 0) * (i.unitPrice ?? 0), 0)

  return (
    <div style={{ padding: 24 }}>
      {/* 篩選列 */}
      <Space wrap style={{ marginBottom: 16 }}>
        <RangePicker
          onChange={(val) => setDateRange(val as [dayjs.Dayjs | null, dayjs.Dayjs | null] | null)}
          placeholder={['開始日期', '結束日期']}
        />
        <Input
          placeholder="廠商名稱"
          value={filterSupplier}
          onChange={(e) => setFilterSupplier(e.target.value)}
          style={{ width: 160 }}
          allowClear
        />
        <Select
          placeholder="訂單狀態"
          value={filterStatus || undefined}
          onChange={(v) => setFilterStatus(v ?? '')}
          allowClear
          style={{ width: 120 }}
          options={[
            { value: '待確認', label: '待確認' },
            { value: '已確認', label: '已確認' },
            { value: '已收貨', label: '已收貨' },
            { value: '已取消', label: '已取消' },
          ]}
        />
        <Button onClick={fetchOrders}>查詢</Button>
        <Button type="primary" icon={<PlusOutlined />} onClick={openCreateModal}>
          新增採購訂單
        </Button>
      </Space>

      <Table
        rowKey="id"
        columns={columns}
        dataSource={orders}
        loading={loading}
        expandable={{
          expandedRowRender: (record) => (
            <Table
              rowKey="id"
              size="small"
              pagination={false}
              dataSource={record.items}
              columns={[
                { title: '產品名稱', dataIndex: 'productName', key: 'productName' },
                { title: '數量', dataIndex: 'quantity', key: 'quantity', align: 'right' },
                {
                  title: '單價',
                  dataIndex: 'unitPrice',
                  key: 'unitPrice',
                  align: 'right',
                  render: (v: number) => v.toFixed(4),
                },
                {
                  title: '小計',
                  dataIndex: 'subtotal',
                  key: 'subtotal',
                  align: 'right',
                  render: (v: number) =>
                    v.toLocaleString('zh-TW', { minimumFractionDigits: 2 }),
                },
              ]}
            />
          ),
        }}
        pagination={{ pageSize: 20, showSizeChanger: false }}
      />

      {/* 新增訂單 Modal */}
      <Modal
        title="新增採購訂單"
        open={modalOpen}
        onOk={handleCreate}
        onCancel={() => setModalOpen(false)}
        okText="建立訂單"
        cancelText="取消"
        confirmLoading={saving}
        width={700}
      >
        <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
          <Form.Item
            label="廠商"
            name="supplierId"
            rules={[{ required: true, message: '請選擇廠商' }]}
          >
            <Select
              placeholder="選擇廠商"
              onChange={(v) => setSelectedSupplierId(v)}
              showSearch
              optionFilterProp="label"
              options={[
                // 從已載入的廠商報價中取得不重複廠商
                ...Array.from(
                  new Map(
                    suppliers.map((s) => [s.supplierId, { value: s.supplierId, label: s.supplierName }]),
                  ).values(),
                ),
              ]}
            />
          </Form.Item>
        </Form>

        <Divider orientation="left">採購明細</Divider>

        {/* 先選產品以載入廠商 */}
        <div style={{ marginBottom: 8 }}>
          <Text type="secondary">請先選擇產品以載入廠商報價，再填寫各明細</Text>
        </div>

        {items.map((item, index) => (
          <Space key={index} align="start" style={{ display: 'flex', marginBottom: 8 }} wrap>
            <Select
              placeholder="選擇產品"
              style={{ width: 200 }}
              value={item.productId}
              onChange={(v) => {
                updateItem(index, 'productId', v)
                handleSupplierProductChange(v)
              }}
              showSearch
              optionFilterProp="label"
              options={products.map((p) => ({
                value: p.id,
                label: `${p.productCode} ${p.name}`,
              }))}
            />
            <InputNumber
              placeholder="數量"
              min={1}
              value={item.quantity}
              onChange={(v) => updateItem(index, 'quantity', v ?? undefined)}
              style={{ width: 100 }}
            />
            <InputNumber
              placeholder="單價"
              min={0}
              precision={4}
              value={item.unitPrice}
              onChange={(v) => updateItem(index, 'unitPrice', v ?? undefined)}
              style={{ width: 120 }}
            />
            <Text style={{ lineHeight: '32px' }}>
              小計：{((item.quantity ?? 0) * (item.unitPrice ?? 0)).toFixed(2)}
            </Text>
            {items.length > 1 && (
              <Button
                danger
                icon={<DeleteOutlined />}
                onClick={() => removeItem(index)}
                size="small"
              />
            )}
          </Space>
        ))}

        <Button
          type="dashed"
          icon={<PlusOutlined />}
          onClick={addItem}
          style={{ marginTop: 8, width: '100%' }}
        >
          新增明細
        </Button>

        <Divider />
        <div style={{ textAlign: 'right' }}>
          <Text strong>總金額：{totalAmount.toLocaleString('zh-TW', { minimumFractionDigits: 2 })}</Text>
        </div>
      </Modal>
    </div>
  )
}
