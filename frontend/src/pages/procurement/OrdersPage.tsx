import { useState, useEffect, useCallback } from 'react'
import {
  Table, Button, Space, Modal, Form, Select, InputNumber, DatePicker,
  Input, message, Tag, Typography, Divider, Statistic, Row, Col, Card,
} from 'antd'
import {
  PlusOutlined, DeleteOutlined, WarningOutlined,
  DownloadOutlined, BarChartOutlined,
} from '@ant-design/icons'
import type { ColumnsType } from 'antd/es/table'
import dayjs from 'dayjs'
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend,
  ResponsiveContainer, PieChart, Pie, Cell,
} from 'recharts'
import {
  getPurchaseOrders, createPurchaseOrder,
  getOrderStats, downloadOrderExcel, downloadSupplierExcel,
} from '../../api/purchaseOrders'
import type { PurchaseOrderStats } from '../../api/purchaseOrders'
import { getProducts } from '../../api/products'
import { getProductSuppliers, getAllSuppliers } from '../../api/suppliers'
import type { SupplierInfo, SupplierOrderPreviewItem } from '../../api/suppliers'
import type { PurchaseOrder, Product, ProductSupplierPrice } from '../../types'

const { Text } = Typography
const { RangePicker } = DatePicker

const CURRENCY_COLOR: Record<string, string> = {
  CNY: 'red', TWD: 'blue', USD: 'green', EUR: 'purple', JPY: 'orange',
}
const PIE_COLORS = ['#faad14', '#1677ff', '#52c41a', '#ff4d4f']

interface OrderItemForm {
  productId?: number; quantity?: number; unitPrice?: number
  currency?: string; leadTimeDays?: number; moq?: number; boxQty?: number
}

// 調達提案から渡される初期データ
export interface SuggestionOrderInit {
  supplierId: number
  supplierName: string
  items: { productId: number; productName: string; quantity: number; unitPrice: number; currency: string }[]
}

export default function OrdersPage({ initOrder }: { initOrder?: SuggestionOrderInit }) {
  const [orders, setOrders] = useState<PurchaseOrder[]>([])
  const [loading, setLoading] = useState(false)
  const [modalOpen, setModalOpen] = useState(false)
  const [saving, setSaving] = useState(false)
  const [statsOpen, setStatsOpen] = useState(false)
  const [stats, setStats] = useState<PurchaseOrderStats | null>(null)
  const [statsLoading, setStatsLoading] = useState(false)

  // 仕入先発注書モーダル
  const [supplierOrderOpen, setSupplierOrderOpen] = useState(false)
  const [allSuppliers, setAllSuppliers] = useState<SupplierInfo[]>([])
  const [selectedSupplierId2, setSelectedSupplierId2] = useState<number | null>(null)
  const [previewItems, setPreviewItems] = useState<(SupplierOrderPreviewItem & { orderQty: number })[]>([])
  const [previewLoading, setPreviewLoading] = useState(false)
  const [exporting, setExporting] = useState(false)

  const [dateRange, setDateRange] = useState<[dayjs.Dayjs | null, dayjs.Dayjs | null] | null>(null)
  const [filterSupplier, setFilterSupplier] = useState('')

  const [form] = Form.useForm()
  const [products, setProducts] = useState<Product[]>([])
  const [suppliers, setSuppliers] = useState<ProductSupplierPrice[]>([])
  const [, setSelectedSupplierId] = useState<number | null>(null)
  const [items, setItems] = useState<OrderItemForm[]>([{}])
  const [, setProductSuppliersMap] = useState<Map<number, ProductSupplierPrice[]>>(new Map())

  const fetchOrders = useCallback(async () => {
    setLoading(true)
    try {
      const params: Record<string, string> = {}
      if (dateRange?.[0]) params.startDate = dateRange[0].toISOString()
      if (dateRange?.[1]) params.endDate = dateRange[1].toISOString()
      if (filterSupplier) params.supplierName = filterSupplier
      const data = await getPurchaseOrders(params)
      setOrders(data)
    } catch {
      message.error('発注一覧の読み込みに失敗しました')
    } finally {
      setLoading(false)
    }
  }, [dateRange, filterSupplier])

  useEffect(() => {
    fetchOrders()
  }, [fetchOrders])

  // 調達提案からの初期データがあれば自動でモーダルを開く
  useEffect(() => {
    if (initOrder) {
      openCreateModalWithInit(initOrder)
    }
  }, [initOrder])

  async function openCreateModal() {
    form.resetFields()
    setItems([{}])
    setSelectedSupplierId(null)
    setSuppliers([])
    try {
      const data = await getProducts(undefined, true)
      setProducts(data)
    } catch {
      message.error('商品一覧の読み込みに失敗しました')
    }
    setModalOpen(true)
  }

  async function openCreateModalWithInit(init: SuggestionOrderInit) {
    form.resetFields()
    form.setFieldValue('supplierId', init.supplierId)
    setItems(init.items.map(i => ({
      productId: i.productId,
      quantity: i.quantity,
      unitPrice: i.unitPrice,
      currency: i.currency,
    })))
    setSelectedSupplierId(init.supplierId)
    try {
      const data = await getProducts(undefined, true)
      setProducts(data)
    } catch {}
    setModalOpen(true)
  }

  async function handleSupplierProductChange(productId: number, index: number) {
    try {
      const result = await getProductSuppliers(productId)
      const currentPrices = result.items.filter(s => s.isCurrent)
      setProductSuppliersMap(prev => { const next = new Map(prev); next.set(productId, currentPrices); return next })
      if (currentPrices.length === 1) {
        updateItem(index, 'unitPrice', currentPrices[0].unitPrice)
        updateItem(index, 'currency', currentPrices[0].currency)
        updateItem(index, 'leadTimeDays', currentPrices[0].leadTimeDays)
      }
      setSuppliers(currentPrices)
    } catch { setSuppliers([]) }
  }

  function updateItem(index: number, field: keyof OrderItemForm, value: number | string | undefined) {
    setItems(prev => { const next = [...prev]; next[index] = { ...next[index], [field]: value }; return next })
  }

  async function handleCreate() {
    await form.validateFields()
    const supplierId = form.getFieldValue('supplierId')
    if (!supplierId) { message.error('仕入先を選択してください'); return }
    const validItems = items.filter(i => i.productId && i.quantity && i.quantity > 0 && i.unitPrice && i.unitPrice > 0)
    if (validItems.length === 0) { message.error('有効な明細を追加してください'); return }
    setSaving(true)
    try {
      await createPurchaseOrder({ supplierId, items: validItems.map(i => ({ productId: i.productId!, quantity: i.quantity!, unitPrice: i.unitPrice! })) })
      message.success('発注を作成しました')
      setModalOpen(false)
      fetchOrders()
    } catch {
      message.error('発注の作成に失敗しました')
    } finally { setSaving(false) }
  }

  function triggerDownload(blob: Blob, filename: string) {
    const url = URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', filename)
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    URL.revokeObjectURL(url)
  }

  async function handleExport(id: number, orderNumber: string) {
    try {
      const { blob, filename } = await downloadOrderExcel(id, orderNumber)
      triggerDownload(blob, filename)
    } catch {
      message.error('Excel出力に失敗しました')
    }
  }

  async function openSupplierOrderModal() {
    setSupplierOrderOpen(true)
    setSelectedSupplierId2(null)
    setPreviewItems([])
    try {
      const data = await getAllSuppliers()
      setAllSuppliers(data)
    } catch {
      message.error('仕入先一覧の読み込みに失敗しました')
    }
  }

  async function handleSupplierSelect(supplierId: number) {
    setSelectedSupplierId2(supplierId)
    setPreviewLoading(true)
    try {
      const supplier = allSuppliers.find(s => s.id === supplierId)
      if (!supplier) return

      // 発注管理からこの仕入先の発注を取得
      const orders = await getPurchaseOrders({ supplierName: supplier.name })

      // 発注明細を商品ごとに集計してプレビュー表示
      const itemMap = new Map<number, SupplierOrderPreviewItem & { orderQty: number }>()
      for (const order of orders) {
        for (const item of order.items) {
          const existing = itemMap.get(item.productId)
          if (existing) {
            existing.orderQty += item.quantity
          } else {
            itemMap.set(item.productId, {
              productId: item.productId,
              productCode: '',
              productName: item.productName,
              unitPrice: item.unitPrice,
              currency: 'CNY',
              leadTimeDays: 0,
              moq: 1,
              boxQty: 1,
              averageShipment: 0,
              safetyStock: 0,
              orderQty: item.quantity,
            })
          }
        }
      }

      setPreviewItems(Array.from(itemMap.values()))
    } catch {
      message.error('発注データの読み込みに失敗しました')
    } finally {
      setPreviewLoading(false)
    }
  }

  async function handleSupplierExcelExport() {
    if (!selectedSupplierId2) return
    const supplier = allSuppliers.find(s => s.id === selectedSupplierId2)
    if (!supplier) return
    setExporting(true)
    try {
      const { blob, filename } = await downloadSupplierExcel(supplier.name)
      triggerDownload(blob, filename)
    } catch {
      message.error('Excel出力に失敗しました')
    } finally {
      setExporting(false)
    }
  }

  async function openStats() {    setStatsOpen(true)
    setStatsLoading(true)
    try {
      const data = await getOrderStats()
      setStats(data)
    } catch {
      message.error('統計の読み込みに失敗しました')
    } finally { setStatsLoading(false) }
  }

  const columns: ColumnsType<PurchaseOrder> = [
    { title: '発注番号', dataIndex: 'orderNumber', key: 'orderNumber', width: 180 },
    { title: '仕入先', dataIndex: 'supplierName', key: 'supplierName', width: 120 },
    {
      title: '合計金額', dataIndex: 'totalAmount', key: 'totalAmount', width: 130, align: 'right',
      render: (val: number) => val.toLocaleString('ja-JP', { minimumFractionDigits: 2 }),
    },
    {
      title: '発注日', dataIndex: 'orderDate', key: 'orderDate', width: 120,
      render: (val: string) => dayjs(val).format('YYYY-MM-DD'),
    },
    { title: '作成者', dataIndex: 'createdBy', key: 'createdBy', width: 120 },
    {
      title: '操作', key: 'actions', width: 100,
      render: (_, record) => (
        <Button size="small" icon={<DownloadOutlined />}
          onClick={() => handleExport(record.id, record.orderNumber)}>Excel</Button>
      ),
    },
  ]

  const detailColumns: ColumnsType<PurchaseOrder['items'][0]> = [
    { title: '商品名', dataIndex: 'productName', key: 'productName' },
    { title: '数量', dataIndex: 'quantity', key: 'quantity', align: 'right' },
    { title: '単価', dataIndex: 'unitPrice', key: 'unitPrice', align: 'right', render: (v: number) => v.toFixed(4) },
    { title: '小計', dataIndex: 'subtotal', key: 'subtotal', align: 'right', render: (v: number) => v.toLocaleString('ja-JP', { minimumFractionDigits: 2 }) },
  ]

  const totalAmount = items.reduce((sum, i) => sum + (i.quantity ?? 0) * (i.unitPrice ?? 0), 0)

  const pieData = stats ? [
    { name: '待確認', value: stats.pendingOrders },
    { name: '已確認', value: stats.confirmedOrders },
    { name: '已入荷', value: stats.receivedOrders },
    { name: 'キャンセル', value: stats.cancelledOrders },
  ].filter(d => d.value > 0) : []

  return (
    <div style={{ padding: 24 }}>
      <Space wrap style={{ marginBottom: 16 }}>
        <RangePicker onChange={(val) => setDateRange(val as [dayjs.Dayjs | null, dayjs.Dayjs | null] | null)} placeholder={['開始日', '終了日']} />
        <Input placeholder="仕入先名" value={filterSupplier} onChange={(e) => setFilterSupplier(e.target.value)} style={{ width: 160 }} allowClear />
        <Button onClick={fetchOrders}>検索</Button>
        <Button type="primary" icon={<PlusOutlined />} onClick={openCreateModal}>発注を追加</Button>
        <Button icon={<BarChartOutlined />} onClick={openStats}>統計</Button>
        <Button type="primary" icon={<DownloadOutlined />} onClick={openSupplierOrderModal}>
          仕入先発注書作成
        </Button>
      </Space>

      <Table
        rowKey="id" columns={columns} dataSource={orders} loading={loading}
        expandable={{ expandedRowRender: (record) => <Table rowKey="id" size="small" pagination={false} dataSource={record.items} columns={detailColumns} /> }}
        pagination={{ pageSize: 20, showSizeChanger: false }}
        tableLayout="auto"
      />

      {/* 発注作成モーダル */}
      <Modal title="発注を追加" open={modalOpen} onOk={handleCreate} onCancel={() => setModalOpen(false)}
        okText="発注作成" cancelText="キャンセル" confirmLoading={saving} width={720}>
        <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
          <Form.Item label="仕入先" name="supplierId" rules={[{ required: true, message: '仕入先を選択してください' }]}>
            <Select placeholder="仕入先を選択" onChange={(v) => setSelectedSupplierId(v)} showSearch optionFilterProp="label"
              options={Array.from(new Map(suppliers.map(s => [s.supplierId, { value: s.supplierId, label: s.supplierName }])).values())} />
          </Form.Item>
        </Form>
        <Divider orientation="left">発注明細</Divider>
        <Text type="secondary" style={{ display: 'block', marginBottom: 8 }}>商品を選択すると仕入先単価が自動的に読み込まれます</Text>
        {items.map((item, index) => {
          const product = products.find(p => p.id === item.productId)
          const showMoqWarning = item.quantity && product?.moq && item.quantity < product.moq
          return (
            <Space key={index} align="start" style={{ display: 'flex', marginBottom: 8 }} wrap>
              <Select placeholder="商品を選択" style={{ width: 200 }} value={item.productId}
                onChange={(v) => { updateItem(index, 'productId', v); handleSupplierProductChange(v, index); const p = products.find(pr => pr.id === v); if (p) { updateItem(index, 'moq', p.moq); updateItem(index, 'boxQty', p.boxQty) } }}
                showSearch optionFilterProp="label" options={products.map(p => ({ value: p.id, label: `${p.productCode} ${p.name}` }))} />
              <Space direction="vertical" size={0}>
                <InputNumber placeholder="数量" min={1} value={item.quantity} onChange={(v) => updateItem(index, 'quantity', v ?? undefined)} style={{ width: 100 }} />
                {showMoqWarning && <Text type="warning" style={{ fontSize: 12 }}><WarningOutlined /> MOQ以下 ({product?.moq})</Text>}
              </Space>
              <InputNumber placeholder="単価" min={0} precision={4} value={item.unitPrice} onChange={(v) => updateItem(index, 'unitPrice', v ?? undefined)} style={{ width: 120 }} />
              {item.currency && <Tag color={CURRENCY_COLOR[item.currency] ?? 'default'}>{item.currency}</Tag>}
              {item.leadTimeDays && <Text type="secondary" style={{ lineHeight: '32px' }}>{item.leadTimeDays}日</Text>}
              <Text style={{ lineHeight: '32px' }}>小計：{((item.quantity ?? 0) * (item.unitPrice ?? 0)).toFixed(2)}</Text>
              {items.length > 1 && <Button danger icon={<DeleteOutlined />} onClick={() => setItems(prev => prev.filter((_, i) => i !== index))} size="small" />}
            </Space>
          )
        })}
        <Button type="dashed" icon={<PlusOutlined />} onClick={() => setItems(prev => [...prev, {}])} style={{ marginTop: 8, width: '100%' }}>明細を追加</Button>
        <Divider />
        <div style={{ textAlign: 'right' }}>
          <Text strong>合計金額：{totalAmount.toLocaleString('ja-JP', { minimumFractionDigits: 2 })}</Text>
        </div>
      </Modal>

      {/* 統計モーダル */}
      <Modal title="発注統計" open={statsOpen} onCancel={() => setStatsOpen(false)} footer={null} width={900} loading={statsLoading}>
        {stats && (
          <>
            <Row gutter={16} style={{ marginBottom: 24 }}>
              <Col span={4}><Card size="small"><Statistic title="総発注数" value={stats.totalOrders} /></Card></Col>
              <Col span={4}><Card size="small"><Statistic title="待確認" value={stats.pendingOrders} valueStyle={{ color: '#faad14' }} /></Card></Col>
              <Col span={4}><Card size="small"><Statistic title="已確認" value={stats.confirmedOrders} valueStyle={{ color: '#1677ff' }} /></Card></Col>
              <Col span={4}><Card size="small"><Statistic title="已入荷" value={stats.receivedOrders} valueStyle={{ color: '#52c41a' }} /></Card></Col>
              <Col span={4}><Card size="small"><Statistic title="キャンセル" value={stats.cancelledOrders} valueStyle={{ color: '#ff4d4f' }} /></Card></Col>
              <Col span={4}><Card size="small"><Statistic title="総金額" value={stats.totalAmount.toFixed(0)} suffix="CNY" /></Card></Col>
            </Row>

            <Row gutter={16}>
              <Col span={14}>
                <Text strong>月別発注金額</Text>
                <ResponsiveContainer width="100%" height={220}>
                  <BarChart data={stats.monthlyStats} margin={{ top: 5, right: 10, bottom: 5, left: 0 }}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="label" tick={{ fontSize: 11 }} />
                    <YAxis />
                    <Tooltip />
                    <Legend />
                    <Bar dataKey="totalAmount" name="金額" fill="#1677ff" />
                    <Bar dataKey="orderCount" name="件数" fill="#95de64" />
                  </BarChart>
                </ResponsiveContainer>
              </Col>
              <Col span={10}>
                <Text strong>ステータス分布</Text>
                <ResponsiveContainer width="100%" height={220}>
                  <PieChart>
                    <Pie data={pieData} dataKey="value" nameKey="name" cx="50%" cy="50%" outerRadius={80} label={({ name, value }) => `${name}: ${value}`}>
                      {pieData.map((_, i) => <Cell key={i} fill={PIE_COLORS[i % PIE_COLORS.length]} />)}
                    </Pie>
                    <Tooltip />
                  </PieChart>
                </ResponsiveContainer>
              </Col>
            </Row>

            <Divider />
            <Text strong>仕入先別発注実績</Text>
            <Table
              size="small" pagination={false} style={{ marginTop: 8 }}
              dataSource={stats.supplierStats} rowKey="supplierName"
              columns={[
                { title: '仕入先', dataIndex: 'supplierName', key: 'supplierName' },
                { title: '発注件数', dataIndex: 'orderCount', key: 'orderCount', align: 'right' },
                { title: '合計金額', dataIndex: 'totalAmount', key: 'totalAmount', align: 'right', render: (v: number) => v.toLocaleString('ja-JP', { minimumFractionDigits: 2 }) },
              ]}
            />
          </>
        )}
      </Modal>

      {/* 仕入先発注書作成モーダル */}
      <Modal
        title="仕入先発注書作成"
        open={supplierOrderOpen}
        onCancel={() => setSupplierOrderOpen(false)}
        width={800}
        footer={
          <Space>
            <Button onClick={() => setSupplierOrderOpen(false)}>閉じる</Button>
            <Button
              type="primary"
              icon={<DownloadOutlined />}
              disabled={!selectedSupplierId2}
              loading={exporting}
              onClick={handleSupplierExcelExport}
            >
              発注書 Excel 出力
            </Button>
          </Space>
        }
      >
        <div style={{ marginBottom: 16 }}>
          <Text strong style={{ marginRight: 8 }}>仕入先を選択：</Text>
          <Select
            style={{ width: 200 }}
            placeholder="仕入先を選択"
            value={selectedSupplierId2 ?? undefined}
            onChange={handleSupplierSelect}
            options={allSuppliers.map(s => ({ value: s.id, label: s.name }))}
          />
        </div>

        {selectedSupplierId2 && (
          <Table
            rowKey="productId"
            size="small"
            loading={previewLoading}
            pagination={false}
            dataSource={previewItems}
            columns={[
              { title: '商品名', dataIndex: 'productName', key: 'productName' },
              {
                title: '単価', dataIndex: 'unitPrice', key: 'unitPrice', width: 100, align: 'right',
                render: (v: number) => v.toFixed(4),
              },
              {
                title: '発注数量',
                key: 'orderQty',
                width: 130,
                align: 'right',
                render: (_, record) => (
                  <InputNumber
                    min={0}
                    value={record.orderQty}
                    size="small"
                    style={{ width: 110 }}
                    onChange={(v) => {
                      setPreviewItems(prev => prev.map(item =>
                        item.productId === record.productId ? { ...item, orderQty: v ?? 0 } : item
                      ))
                    }}
                  />
                ),
              },
              {
                title: '小計',
                key: 'subtotal',
                width: 120,
                align: 'right',
                render: (_, r) => (r.orderQty * r.unitPrice).toLocaleString('ja-JP', { minimumFractionDigits: 2 }),
              },
            ]}
            summary={() => {
              const total = previewItems.reduce((sum, r) => sum + r.orderQty * r.unitPrice, 0)
              return (
                <Table.Summary.Row>
                  <Table.Summary.Cell index={0} colSpan={6}><Text strong>合計</Text></Table.Summary.Cell>
                  <Table.Summary.Cell index={1} align="right">
                    <Text strong style={{ color: '#1677ff' }}>{total.toLocaleString('ja-JP', { minimumFractionDigits: 2 })}</Text>
                  </Table.Summary.Cell>
                </Table.Summary.Row>
              )
            }}
          />
        )}
      </Modal>
    </div>
  )
}