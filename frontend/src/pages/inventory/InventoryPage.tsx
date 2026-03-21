import { useState, useEffect, useCallback } from 'react'
import { Table, Button, Space, Tag, message, Tooltip, theme } from 'antd'
import { DownloadOutlined, WarningOutlined } from '@ant-design/icons'
import type { ColumnsType } from 'antd/es/table'
import { useNavigate } from 'react-router-dom'
import { getInventoryOverview, getExportUrl } from '../../api/inventory'
import type { InventoryOverview } from '../../api/inventory'

export default function InventoryPage() {
  const [data, setData] = useState<InventoryOverview[]>([])
  const [loading, setLoading] = useState(false)
  const navigate = useNavigate()
  const { token: colorToken } = theme.useToken()
  const isDark = colorToken.colorBgContainer === '#141414' || colorToken.colorBgBase === '#000'

  const fetchData = useCallback(async () => {
    setLoading(true)
    try {
      const result = await getInventoryOverview()
      setData(result)
    } catch {
      message.error('載入庫存總覽失敗')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchData()
  }, [fetchData])

  function handleExport() {
    const url = getExportUrl()
    // 帶 token 下載
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', '')
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
  }

  const columns: ColumnsType<InventoryOverview> = [
    {
      title: '產品編號',
      dataIndex: 'productCode',
      key: 'productCode',
      width: 120,
      sorter: (a, b) => a.productCode.localeCompare(b.productCode),
    },
    {
      title: '產品名稱',
      dataIndex: 'productName',
      key: 'productName',
      sorter: (a, b) => a.productName.localeCompare(b.productName),
    },
    {
      title: '單位',
      dataIndex: 'unit',
      key: 'unit',
      width: 80,
    },
    {
      title: '當前庫存',
      dataIndex: 'currentStock',
      key: 'currentStock',
      width: 110,
      align: 'right',
      sorter: (a, b) => a.currentStock - b.currentStock,
      render: (val: number, record) => (
        <Space size={4}>
          <span>{val}</span>
          {record.stockStatus === 'Low' && (
            <Tooltip title="庫存低於六個月平均出貨量">
              <WarningOutlined style={{ color: '#faad14' }} />
            </Tooltip>
          )}
        </Space>
      ),
    },
    {
      title: '六個月平均出貨量',
      dataIndex: 'sixMonthAvgShipment',
      key: 'sixMonthAvgShipment',
      width: 160,
      align: 'right',
      sorter: (a, b) => a.sixMonthAvgShipment - b.sixMonthAvgShipment,
      render: (val: number) => val.toFixed(1),
    },
    {
      title: '建議採購量',
      dataIndex: 'suggestedProcurementQty',
      key: 'suggestedProcurementQty',
      width: 120,
      align: 'right',
      sorter: (a, b) => a.suggestedProcurementQty - b.suggestedProcurementQty,
    },
    {
      title: '庫存狀態',
      dataIndex: 'stockStatus',
      key: 'stockStatus',
      width: 100,
      render: (status: string) =>
        status === 'Low' ? (
          <Tag color="warning" style={{ color: '#7c4a00' }}>庫存不足</Tag>
        ) : (
          <Tag color="success">正常</Tag>
        ),
    },
    {
      title: '操作',
      key: 'actions',
      width: 120,
      render: (_, record) => (
        <Button size="small" onClick={() => navigate(`/inventory/${record.productId}/history`)}>
          查看歷程
        </Button>
      ),
    },
  ]

  return (
    <div style={{ padding: 24 }}>
      <div style={{ display: 'flex', justifyContent: 'flex-end', marginBottom: 16 }}>
        <Button icon={<DownloadOutlined />} onClick={handleExport}>
          匯出 Excel
        </Button>
      </div>
      <Table
        rowKey="productId"
        columns={columns}
        dataSource={data}
        loading={loading}
        rowClassName={(record) => (record.stockStatus === 'Low' ? 'row-low-stock' : '')}
        pagination={{ pageSize: 20, showSizeChanger: false }}
      />
      <style>{`.row-low-stock td { background-color: ${isDark ? 'rgba(250,173,20,0.08)' : '#fffbe6'} !important; }`}</style>
    </div>
  )
}
