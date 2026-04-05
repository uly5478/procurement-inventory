import { useState, useEffect, useCallback } from 'react'
import { Table, Button, Space, message, Tooltip, theme, Modal, Tag, Typography } from 'antd'
import { DownloadOutlined, WarningOutlined } from '@ant-design/icons'
import type { ColumnsType } from 'antd/es/table'
import { useNavigate } from 'react-router-dom'
import { getInventoryOverview } from '../../api/inventory'
import type { InventoryOverview } from '../../api/inventory'
import { getMonthlyInventory } from '../../api/monthlyInventory'
import type { MonthlyInventory } from '../../types'
import dayjs from 'dayjs'

const { Text } = Typography

// 今月から6ヶ月分のラベルを生成
function getNextSixMonthLabels(): { label: string; key: string }[] {
  return Array.from({ length: 6 }, (_, i) => {
    const d = dayjs().add(i, 'month')
    return { label: d.format('YYYY/MM'), key: d.format('YYYY-MM') }
  })
}

const SIX_MONTHS = getNextSixMonthLabels()

export default function InventoryPage() {
  const [data, setData] = useState<InventoryOverview[]>([])
  const [loading, setLoading] = useState(false)
  const navigate = useNavigate()
  const { token: colorToken } = theme.useToken()
  const isDark = colorToken.colorBgContainer === '#141414' || colorToken.colorBgBase === '#000'

  const [monthlyData, setMonthlyData] = useState<MonthlyInventory[]>([])
  const [monthlyLoading, setMonthlyLoading] = useState(false)
  const [selectedProductId, setSelectedProductId] = useState<number | null>(null)

  const fetchData = useCallback(async () => {
    setLoading(true)
    try {
      const result = await getInventoryOverview()
      setData(result)
    } catch {
      message.error('在庫一覧の読み込みに失敗しました')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchData()
  }, [fetchData])

  const fetchMonthlyData = useCallback(async (productId: number) => {
    setMonthlyLoading(true)
    try {
      const result = await getMonthlyInventory(productId)
      setMonthlyData(result)
    } catch {
      message.error('月次在庫の読み込みに失敗しました')
    } finally {
      setMonthlyLoading(false)
    }
  }, [])

  async function handleExport() {
    try {
      const res = await import('../../api/client').then(m => m.default.get('/inventory/export', { responseType: 'blob' }))
      const url = URL.createObjectURL(res.data as Blob)
      const link = document.createElement('a')
      link.href = url
      link.setAttribute('download', `inventory_export_${new Date().toISOString().slice(0, 10)}.xlsx`)
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
      URL.revokeObjectURL(url)
    } catch {
      message.error('Excel出力に失敗しました')
    }
  }

  // 月ラベルから発注数を取得するヘルパー
  function getOrderQty(record: InventoryOverview, label: string): number | null {
    const suggestions = record.monthlyOrderSuggestions
    if (!suggestions || suggestions.length === 0) return null
    const found = suggestions.find(s => s.label === label)
    return found ? found.suggestedQty : null
  }

  // 6ヶ月分の発注数列を動的生成
  const monthlyOrderColumns: ColumnsType<InventoryOverview> = SIX_MONTHS.map(({ label, key }, idx) => ({
    title: (
      <div style={{ textAlign: 'center' }}>
        <div style={{ fontSize: 11, color: '#888' }}>{idx === 0 ? '今月' : `+${idx}ヶ月`}</div>
        <div>{label}</div>
      </div>
    ),
    key: `order_${key}`,
    width: 100,
    align: 'right' as const,
    render: (_: unknown, record: InventoryOverview) => {
      const qty = getOrderQty(record, label)
      if (qty === null) return <Text type="secondary">-</Text>
      if (qty === 0) return <Tag color="success" style={{ margin: 0 }}>不要</Tag>
      return <Text strong style={{ color: idx === 0 ? '#cf1322' : '#1677ff' }}>{qty.toLocaleString()}</Text>
    },
  }))

  const baseColumns: ColumnsType<InventoryOverview> = [
    {
      title: '商品コード',
      dataIndex: 'productCode',
      key: 'productCode',
      width: 120,
      fixed: 'left',
      sorter: (a, b) => a.productCode.localeCompare(b.productCode),
    },
    {
      title: '商品名',
      dataIndex: 'productName',
      key: 'productName',
      width: 150,
      fixed: 'left',
      sorter: (a, b) => a.productName.localeCompare(b.productName),
    },
    {
      title: '単位',
      dataIndex: 'unit',
      key: 'unit',
      width: 60,
    },
    {
      title: '89倉庫',
      dataIndex: 'warehouse89',
      key: 'warehouse89',
      width: 80,
      align: 'right',
      render: (val: number) => val.toLocaleString(),
    },
    {
      title: '81倉庫',
      dataIndex: 'warehouse81',
      key: 'warehouse81',
      width: 80,
      align: 'right',
      render: (val: number) => val.toLocaleString(),
    },
    {
      title: '検査倉庫',
      dataIndex: 'warehouseInspection',
      key: 'warehouseInspection',
      width: 80,
      align: 'right',
      render: (val: number) => val.toLocaleString(),
    },
    {
      title: '第四倉庫',
      dataIndex: 'warehouse4th',
      key: 'warehouse4th',
      width: 80,
      align: 'right',
      render: (val: number) => val.toLocaleString(),
    },
    {
      title: '在庫合計',
      dataIndex: 'totalWarehouseStock',
      key: 'totalWarehouseStock',
      width: 90,
      align: 'right',
      sorter: (a, b) => a.totalWarehouseStock - b.totalWarehouseStock,
      render: (val: number, record) => {
        const isLow = record.safetyStock > 0 && val < record.safetyStock
        return (
          <Space size={4}>
            <span style={{ color: isLow ? '#cf1322' : undefined, fontWeight: isLow ? 'bold' : undefined }}>
              {val.toLocaleString()}
            </span>
            {isLow && (
              <Tooltip title={`安全在庫以下 (${record.safetyStock})`}>
                <WarningOutlined style={{ color: '#faad14' }} />
              </Tooltip>
            )}
          </Space>
        )
      },
    },
    {
      title: '未引当',
      dataIndex: 'unallocatedQty',
      key: 'unallocatedQty',
      width: 75,
      align: 'right',
      render: (val: number) => val.toLocaleString(),
    },
    {
      title: '出荷数',
      dataIndex: 'shippedQty',
      key: 'shippedQty',
      width: 75,
      align: 'right',
      render: (val: number) => val.toLocaleString(),
    },
    {
      title: '安全在庫',
      dataIndex: 'safetyStock',
      key: 'safetyStock',
      width: 80,
      align: 'right',
      render: (val: number) => val.toLocaleString(),
    },
    {
      title: '平均出荷数',
      dataIndex: 'sixMonthAvgShipment',
      key: 'sixMonthAvgShipment',
      width: 90,
      align: 'right',
      sorter: (a, b) => a.sixMonthAvgShipment - b.sixMonthAvgShipment,
      render: (val: number) => val.toFixed(1),
    },
  ]

  const actionColumn: ColumnsType<InventoryOverview> = [
    {
      title: '操作',
      key: 'actions',
      width: 130,
      fixed: 'right',
      render: (_, record) => (
        <Space size={4}>
          <Button size="small" onClick={() => navigate(`/inventory/${record.productId}/history`)}>
            履歴
          </Button>
          <Button size="small" onClick={() => {
            setSelectedProductId(record.productId)
            fetchMonthlyData(record.productId)
          }}>
            月次
          </Button>
        </Space>
      ),
    },
  ]

  const columns = [...baseColumns, ...monthlyOrderColumns, ...actionColumn]

  // 月次在庫テーブル
  const monthlyColumns: ColumnsType<MonthlyInventory> = [
    {
      title: '年月',
      key: 'period',
      width: 90,
      render: (_, r) => `${r.year}/${String(r.month).padStart(2, '0')}`,
    },
    {
      title: '発注数',
      dataIndex: 'orderQty',
      key: 'orderQty',
      width: 90,
      align: 'right',
      render: (val: number) => val.toLocaleString(),
    },
    {
      title: '在庫数',
      dataIndex: 'stockQty',
      key: 'stockQty',
      width: 90,
      align: 'right',
      render: (val: number) => val.toLocaleString(),
    },
    {
      title: '在庫金額',
      dataIndex: 'stockAmount',
      key: 'stockAmount',
      width: 110,
      align: 'right',
      render: (val: number) => val.toLocaleString('ja-JP', { minimumFractionDigits: 2 }),
    },
    {
      title: '回転率',
      dataIndex: 'turnoverRate',
      key: 'turnoverRate',
      width: 80,
      align: 'right',
      render: (val: number) => val.toFixed(2),
    },
  ]

  return (
    <div style={{ padding: 24 }}>
      <div style={{ display: 'flex', justifyContent: 'flex-end', marginBottom: 16 }}>
        <Button icon={<DownloadOutlined />} onClick={handleExport}>
          Excel出力
        </Button>
      </div>
      <Table
        rowKey="productId"
        columns={columns}
        dataSource={data}
        loading={loading}
        rowClassName={(record) => {
          const isLow = record.safetyStock > 0 && record.totalWarehouseStock < record.safetyStock
          return isLow ? 'row-low-stock' : ''
        }}
        pagination={{ pageSize: 20, showSizeChanger: false }}
        scroll={{ x: 1800 }}
        size="small"
      />

      {/* 月次在庫モーダル */}
      <Modal
        title={`月次在庫 - ${data.find(d => d.productId === selectedProductId)?.productName ?? ''}`}
        open={selectedProductId !== null}
        onCancel={() => setSelectedProductId(null)}
        footer={null}
        width={600}
      >
        <Table
          rowKey="id"
          columns={monthlyColumns}
          dataSource={monthlyData}
          loading={monthlyLoading}
          pagination={false}
          size="small"
        />
      </Modal>

      <style>{`
        .row-low-stock td { background-color: ${isDark ? 'rgba(250,173,20,0.08)' : '#fffbe6'} !important; }
      `}</style>
    </div>
  )
}