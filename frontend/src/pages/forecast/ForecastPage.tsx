import { useState, useEffect, useCallback } from 'react'
import {
  Table,
  message,
  Modal,
  Alert,
  Typography,
  Spin,
} from 'antd'
import type { ColumnsType } from 'antd/es/table'
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts'
import { getAllForecasts, getProductForecast } from '../../api/forecast'
import type { DemandForecastDto, ProductForecastDetail } from '../../api/forecast'

const { Text } = Typography

export default function ForecastPage() {
  const [forecasts, setForecasts] = useState<DemandForecastDto[]>([])
  const [loading, setLoading] = useState(false)
  const [detailOpen, setDetailOpen] = useState(false)
  const [detail, setDetail] = useState<ProductForecastDetail | null>(null)
  const [detailLoading, setDetailLoading] = useState(false)

  const fetchForecasts = useCallback(async () => {
    setLoading(true)
    try {
      const data = await getAllForecasts()
      setForecasts(data)
    } catch {
      message.error('載入需求預測失敗')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchForecasts()
  }, [fetchForecasts])

  async function openDetail(productId: number) {
    setDetailOpen(true)
    setDetail(null)
    setDetailLoading(true)
    try {
      const data = await getProductForecast(productId)
      setDetail(data)
    } catch {
      message.error('載入預測詳情失敗')
    } finally {
      setDetailLoading(false)
    }
  }

  const columns: ColumnsType<DemandForecastDto> = [
    {
      title: '產品編號',
      dataIndex: 'productCode',
      key: 'productCode',
      width: 120,
    },
    {
      title: '產品名稱',
      dataIndex: 'productName',
      key: 'productName',
    },
    {
      title: '預測月份',
      key: 'forecastPeriod',
      width: 120,
      render: (_, r) => `${r.forecastYear}/${String(r.forecastMonth).padStart(2, '0')}`,
    },
    {
      title: '預測需求量',
      dataIndex: 'forecastQty',
      key: 'forecastQty',
      width: 120,
      align: 'right',
      render: (v: number) => v.toFixed(0),
    },
    {
      title: '信賴區間',
      key: 'confidence',
      width: 160,
      align: 'right',
      render: (_, r) => `${r.confidenceLower.toFixed(0)} ~ ${r.confidenceUpper.toFixed(0)}`,
    },
    {
      title: '操作',
      key: 'actions',
      width: 100,
      render: (_, r) => (
        <a onClick={() => openDetail(r.productId)}>查看詳情</a>
      ),
    },
  ]

  // 組合圖表資料：歷史 + 預測
  function buildChartData(d: ProductForecastDetail) {
    const historical = d.historicalShipments.map((m) => ({
      label: `${m.year}/${String(m.month).padStart(2, '0')}`,
      actual: m.totalShipped,
      forecast: undefined as number | undefined,
      lower: undefined as number | undefined,
      upper: undefined as number | undefined,
    }))

    if (d.forecast) {
      historical.push({
        label: `${d.forecast.forecastYear}/${String(d.forecast.forecastMonth).padStart(2, '0')}`,
        actual: undefined as unknown as number,
        forecast: Number(d.forecast.forecastQty),
        lower: Number(d.forecast.confidenceLower),
        upper: Number(d.forecast.confidenceUpper),
      })
    }

    return historical
  }

  return (
    <div style={{ padding: 24 }}>
      <Table
        rowKey="productId"
        columns={columns}
        dataSource={forecasts}
        loading={loading}
        pagination={{ pageSize: 20, showSizeChanger: false }}
      />

      <Modal
        title={detail ? `${detail.productCode} ${detail.productName} — 需求預測詳情` : '需求預測詳情'}
        open={detailOpen}
        onCancel={() => setDetailOpen(false)}
        footer={null}
        width={760}
      >
        {detailLoading ? (
          <div style={{ textAlign: 'center', padding: 40 }}>
            <Spin />
          </div>
        ) : detail ? (
          <>
            {detail.errorMessage ? (
              <Alert type="warning" message={detail.errorMessage} showIcon />
            ) : (
              <>
                {detail.forecast && (
                  <div style={{ marginBottom: 16 }}>
                    <Text>
                      預測需求量：<Text strong>{detail.forecast.forecastQty.toFixed(0)}</Text> 件
                      （信賴區間：{detail.forecast.confidenceLower.toFixed(0)} ~{' '}
                      {detail.forecast.confidenceUpper.toFixed(0)}）
                    </Text>
                  </div>
                )}
                <ResponsiveContainer width="100%" height={300}>
                  <LineChart data={buildChartData(detail)} margin={{ top: 5, right: 20, bottom: 5, left: 0 }}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="label" />
                    <YAxis />
                    <Tooltip />
                    <Legend />
                    <Line
                      type="monotone"
                      dataKey="actual"
                      name="歷史出貨量"
                      stroke="#1677ff"
                      strokeWidth={2}
                      dot={{ r: 4 }}
                      connectNulls={false}
                    />
                    <Line
                      type="monotone"
                      dataKey="forecast"
                      name="預測需求量"
                      stroke="#52c41a"
                      strokeWidth={2}
                      strokeDasharray="5 5"
                      dot={{ r: 5 }}
                      connectNulls={false}
                    />
                    <Line
                      type="monotone"
                      dataKey="upper"
                      name="信賴上界"
                      stroke="#faad14"
                      strokeWidth={1}
                      strokeDasharray="3 3"
                      dot={false}
                      connectNulls={false}
                    />
                    <Line
                      type="monotone"
                      dataKey="lower"
                      name="信賴下界"
                      stroke="#faad14"
                      strokeWidth={1}
                      strokeDasharray="3 3"
                      dot={false}
                      connectNulls={false}
                    />
                  </LineChart>
                </ResponsiveContainer>
              </>
            )}
          </>
        ) : null}
      </Modal>
    </div>
  )
}
