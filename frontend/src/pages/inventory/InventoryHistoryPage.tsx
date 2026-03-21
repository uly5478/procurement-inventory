import { useState, useEffect, useCallback } from 'react'
import { Table, DatePicker, Space, message, Tag, Statistic, Row, Col, Card } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { useParams } from 'react-router-dom'
import dayjs from 'dayjs'
import { getTransactionHistory, getMonthlySummary } from '../../api/inventory'
import type { StockTransactionHistory, MonthlyShipment } from '../../api/inventory'

const { RangePicker } = DatePicker

export default function InventoryHistoryPage() {
  const { productId } = useParams<{ productId: string }>()
  const pid = Number(productId)

  const [history, setHistory] = useState<StockTransactionHistory[]>([])
  const [monthly, setMonthly] = useState<MonthlyShipment[]>([])
  const [loading, setLoading] = useState(false)
  const [dateRange, setDateRange] = useState<[dayjs.Dayjs | null, dayjs.Dayjs | null] | null>(null)

  const fetchHistory = useCallback(async () => {
    if (!pid) return
    setLoading(true)
    try {
      const [hist, mon] = await Promise.all([
        getTransactionHistory(
          pid,
          dateRange?.[0]?.toISOString(),
          dateRange?.[1]?.toISOString(),
        ),
        getMonthlySummary(pid, 6),
      ])
      setHistory(hist)
      setMonthly(mon)
    } catch {
      message.error('載入庫存歷程失敗')
    } finally {
      setLoading(false)
    }
  }, [pid, dateRange])

  useEffect(() => {
    fetchHistory()
  }, [fetchHistory])

  const columns: ColumnsType<StockTransactionHistory> = [
    {
      title: '異動日期',
      dataIndex: 'transactionDate',
      key: 'transactionDate',
      width: 160,
      render: (v: string) => dayjs(v).format('YYYY-MM-DD HH:mm'),
    },
    {
      title: '類型',
      dataIndex: 'transactionType',
      key: 'transactionType',
      width: 80,
      render: (type: string) => (
        <Tag color={type === '入庫' ? 'green' : 'orange'}>{type}</Tag>
      ),
    },
    {
      title: '數量',
      dataIndex: 'quantity',
      key: 'quantity',
      width: 90,
      align: 'right',
    },
    {
      title: '異動前庫存',
      dataIndex: 'stockBefore',
      key: 'stockBefore',
      width: 110,
      align: 'right',
    },
    {
      title: '異動後庫存',
      dataIndex: 'stockAfter',
      key: 'stockAfter',
      width: 110,
      align: 'right',
    },
    {
      title: '操作人員',
      dataIndex: 'operatorAccount',
      key: 'operatorAccount',
      width: 120,
    },
    {
      title: '備註',
      dataIndex: 'remark',
      key: 'remark',
    },
  ]

  return (
    <div style={{ padding: 24 }}>
      {/* 每月出貨統計 */}
      {monthly.length > 0 && (
        <Row gutter={16} style={{ marginBottom: 24 }}>
          {monthly.map((m) => (
            <Col key={`${m.year}-${m.month}`}>
              <Card size="small">
                <Statistic
                  title={`${m.year}/${String(m.month).padStart(2, '0')}`}
                  value={m.totalShipped}
                  suffix="件"
                />
              </Card>
            </Col>
          ))}
        </Row>
      )}

      {/* 篩選 */}
      <Space style={{ marginBottom: 16 }}>
        <RangePicker
          onChange={(val) =>
            setDateRange(val as [dayjs.Dayjs | null, dayjs.Dayjs | null] | null)
          }
          placeholder={['開始日期', '結束日期']}
        />
      </Space>

      <Table
        rowKey="id"
        columns={columns}
        dataSource={history}
        loading={loading}
        pagination={{ pageSize: 30, showSizeChanger: false }}
      />
    </div>
  )
}
