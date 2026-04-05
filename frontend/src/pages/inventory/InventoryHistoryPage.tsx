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
      message.error('在庫履歴の読み込みに失敗しました')
    } finally {
      setLoading(false)
    }
  }, [pid, dateRange])

  useEffect(() => {
    fetchHistory()
  }, [fetchHistory])

  const columns: ColumnsType<StockTransactionHistory> = [
    {
      title: '取引日',
      dataIndex: 'transactionDate',
      key: 'transactionDate',
      width: 160,
      render: (v: string) => dayjs(v).format('YYYY-MM-DD HH:mm'),
    },
    {
      title: '種別',
      dataIndex: 'transactionType',
      key: 'transactionType',
      width: 80,
      render: (type: string) => (
        <Tag color={type === '入庫' ? 'green' : 'orange'}>{type}</Tag>
      ),
    },
    {
      title: '数量',
      dataIndex: 'quantity',
      key: 'quantity',
      width: 90,
      align: 'right',
    },
    {
      title: '取引前在庫',
      dataIndex: 'stockBefore',
      key: 'stockBefore',
      width: 110,
      align: 'right',
    },
    {
      title: '取引後在庫',
      dataIndex: 'stockAfter',
      key: 'stockAfter',
      width: 110,
      align: 'right',
    },
    {
      title: '担当者',
      dataIndex: 'operatorAccount',
      key: 'operatorAccount',
      width: 120,
    },
    {
      title: '備考',
      dataIndex: 'remark',
      key: 'remark',
    },
  ]

  return (
    <div style={{ padding: 24 }}>
      {monthly.length > 0 && (
        <Row gutter={16} style={{ marginBottom: 24 }}>
          {monthly.map((m) => (
            <Col key={`${m.year}-${m.month}`}>
              <Card size="small">
                <Statistic
                  title={`${m.year}/${String(m.month).padStart(2, '0')}`}
                  value={m.totalShipped}
                  suffix="個"
                />
              </Card>
            </Col>
          ))}
        </Row>
      )}

      <Space style={{ marginBottom: 16 }}>
        <RangePicker
          onChange={(val) =>
            setDateRange(val as [dayjs.Dayjs | null, dayjs.Dayjs | null] | null)
          }
          placeholder={['開始日', '終了日']}
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