import { useState, useEffect, useCallback } from 'react'
import {
  Table,
  message,
  Modal,
  Alert,
  Typography,
  Spin,
  Tag,
  Space,
  Descriptions,
  Tooltip,
} from 'antd'
import type { ColumnsType } from 'antd/es/table'
import {
  BarChart,
  Bar,
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip as RechartsTooltip,
  Legend,
  ResponsiveContainer,
  ReferenceLine,
  Cell,
} from 'recharts'
import dayjs from 'dayjs'
import { getAllForecasts, getProductForecast, getMonthlyDetail } from '../../api/forecast'
import type { DemandForecastDto, ProductForecastDetail, MonthlyShipmentDetailDto } from '../../api/forecast'
import { getInventoryOverview } from '../../api/inventory'
import type { InventoryOverview } from '../../api/inventory'
import useTurnoverStore from '../../store/turnoverStore'

const { Text, Title } = Typography

const SIX_MONTHS = Array.from({ length: 6 }, (_, i) => {
  const d = dayjs().add(i, 'month')
  return { label: d.format('YYYY/MM'), idx: i }
})

/** 回転率・リードタイムから発注数を計算（SuggestionsPageと同じロジック） */
function calcOrderQty(
  avg: number,
  turnover: number,
  leadTimeDays: number,
  effectiveStock: number,
  moq: number,
  boxQty: number,
): number {
  const leadTimeMonths = leadTimeDays / 30
  const needed = avg * (turnover + leadTimeMonths) - effectiveStock
  if (needed <= 0) return 0
  let qty = Math.ceil(needed)
  if (qty < moq) qty = moq
  const bq = boxQty > 0 ? boxQty : 1
  if (bq > 1 && qty % bq !== 0) qty = Math.floor(qty / bq + 1) * bq
  return qty
}

/** 6ヶ月分の発注計画を再計算（SuggestionsPageと同じロジック） */
function calcMonthlyOrders(
  avg: number,
  turnover: number,
  leadTimeDays: number,
  effectiveStock: number,
  moq: number,
  boxQty: number,
): { label: string; qty: number; estimatedStock: number }[] {
  const result: { label: string; qty: number; estimatedStock: number }[] = []
  let stock = effectiveStock
  for (let i = 0; i < 6; i++) {
    if (i > 0) stock = Math.max(0, stock - avg)
    const qty = calcOrderQty(avg, turnover, leadTimeDays, stock, moq, boxQty)
    result.push({ label: dayjs().add(i, 'month').format('YYYY/MM'), qty, estimatedStock: Math.round(stock) })
    if (qty > 0) stock += qty
  }
  return result
}

// カスタム Dot: 平均超過点を赤くする
function CustomDot(props: {
  cx?: number; cy?: number; payload?: { actual?: number; avg?: number; year?: number; month?: number }
  onClickAbove?: (year: number, month: number) => void
}) {
  const { cx, cy, payload, onClickAbove } = props
  if (!cx || !cy || payload?.actual === undefined) return null
  const isAbove = payload.avg !== undefined && payload.actual > payload.avg
  return (
    <circle
      cx={cx}
      cy={cy}
      r={isAbove ? 7 : 4}
      fill={isAbove ? '#ff4d4f' : '#1677ff'}
      stroke="#fff"
      strokeWidth={1}
      style={{ cursor: isAbove ? 'pointer' : 'default' }}
      onClick={() => {
        if (isAbove && payload.year && payload.month && onClickAbove) {
          onClickAbove(payload.year, payload.month)
        }
      }}
    />
  )
}

export default function ForecastPage() {
  const [forecasts, setForecasts] = useState<DemandForecastDto[]>([])
  const [inventoryData, setInventoryData] = useState<InventoryOverview[]>([])
  const [loading, setLoading] = useState(false)
  const [detailOpen, setDetailOpen] = useState(false)
  const [detail, setDetail] = useState<ProductForecastDetail | null>(null)
  const [detailLoading, setDetailLoading] = useState(false)
  const [currentProductId, setCurrentProductId] = useState<number | null>(null)

  // Zustandストアから回転率を取得
  const { turnover } = useTurnoverStore()

  // 月別詳細ポップアップ
  const [monthlyDetail, setMonthlyDetail] = useState<MonthlyShipmentDetailDto | null>(null)
  const [monthlyDetailLoading, setMonthlyDetailLoading] = useState(false)

  const fetchData = useCallback(async () => {
    setLoading(true)
    try {
      const [forecastData, invData] = await Promise.all([
        getAllForecasts(),
        getInventoryOverview(),
      ])
      setForecasts(forecastData)
      setInventoryData(invData)
    } catch {
      message.error('データの読み込みに失敗しました')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchData()
  }, [fetchData])

  async function openDetail(productId: number) {
    setDetailOpen(true)
    setDetail(null)
    setCurrentProductId(productId)
    setDetailLoading(true)
    try {
      const data = await getProductForecast(productId)
      setDetail(data)
    } catch {
      message.error('予測詳細の読み込みに失敗しました')
    } finally {
      setDetailLoading(false)
    }
  }

  async function handleDotClick(year: number, month: number) {
    if (!currentProductId) return
    setMonthlyDetailLoading(true)
    setMonthlyDetail(null)
    try {
      const data = await getMonthlyDetail(currentProductId, year, month)
      setMonthlyDetail(data)
    } catch {
      message.error('出荷詳細の読み込みに失敗しました')
    } finally {
      setMonthlyDetailLoading(false)
    }
  }

  function getOrderQty(productId: number, label: string): number | null {
    const inv = inventoryData.find(d => d.productId === productId)
    if (!inv) return null
    
    // 現在の回転率で再計算
    const forecast = forecasts.find(f => f.productId === productId)
    if (!forecast) return null
    
    // 在庫データから必要な情報を取得
    const avg = inv.sixMonthAvgShipment ?? 0
    const leadTimeDays = inv.recommendedLeadTimeDays ?? 60
    const effectiveStock = inv.currentStock ?? 0
    const moq = inv.moq ?? 0
    const boxQty = inv.boxQty ?? 1
    
    // 6ヶ月分の発注計画を再計算
    const orders = calcMonthlyOrders(avg, turnover, leadTimeDays, effectiveStock, moq, boxQty)
    const found = orders.find(o => o.label === label)
    return found ? found.qty : null
  }

  const monthlyOrderColumns: ColumnsType<DemandForecastDto> = SIX_MONTHS.map(({ label, idx }) => ({
    title: (
      <div style={{ textAlign: 'center' }}>
        <div style={{ fontSize: 11, color: '#888' }}>{idx === 0 ? '今月' : `+${idx}M`}</div>
        <div>{label}</div>
      </div>
    ),
    key: `order_${label}`,
    width: 90,
    align: 'right' as const,
    render: (_: unknown, r: DemandForecastDto) => {
      const qty = getOrderQty(r.productId, label)
      if (qty === null) return <Text type="secondary">-</Text>
      if (qty === 0) return <Tag color="success" style={{ margin: 0 }}>不要</Tag>
      return <Text strong style={{ color: idx === 0 ? '#cf1322' : '#1677ff' }}>{qty.toLocaleString()}</Text>
    },
  }))

  const baseColumns: ColumnsType<DemandForecastDto> = [
    { title: '商品コード', dataIndex: 'productCode', key: 'productCode', width: 110, fixed: 'left' },
    { title: '商品名', dataIndex: 'productName', key: 'productName', width: 120, fixed: 'left' },
    {
      title: '予測月', key: 'forecastPeriod', width: 90,
      render: (_, r) => `${r.forecastYear}/${String(r.forecastMonth).padStart(2, '0')}`,
    },
    {
      title: '信頼区間', key: 'confidence', width: 130, align: 'right',
      render: (_, r) => <Text type="secondary">{Math.round(r.confidenceLower)} ~ {Math.round(r.confidenceUpper)}</Text>,
    },
    { title: '操作', key: 'actions', width: 80, render: (_, r) => <a onClick={() => openDetail(r.productId)}>詳細</a> },
  ]

  const columns = [...baseColumns.slice(0, -1), ...monthlyOrderColumns, baseColumns[baseColumns.length - 1]]

  function buildChartData(d: ProductForecastDetail) {
    const avg = d.historicalShipments.length > 0
      ? d.historicalShipments.reduce((s, m) => s + m.totalShipped, 0) / d.historicalShipments.length
      : 0

    const historical = d.historicalShipments.map((m) => ({
      label: `${m.year}/${String(m.month).padStart(2, '0')}`,
      actual: m.totalShipped,
      avg: Math.round(avg),
      year: m.year,
      month: m.month,
      forecast: undefined as number | undefined,
      lower: undefined as number | undefined,
      upper: undefined as number | undefined,
    }))

    if (d.forecast) {
      historical.push({
        label: `${d.forecast.forecastYear}/${String(d.forecast.forecastMonth).padStart(2, '0')}`,
        actual: undefined as unknown as number,
        avg: Math.round(avg),
        year: d.forecast.forecastYear,
        month: d.forecast.forecastMonth,
        forecast: Number(d.forecast.forecastQty),
        lower: Number(d.forecast.confidenceLower),
        upper: Number(d.forecast.confidenceUpper),
      })
    }
    return { data: historical, avg: Math.round(avg) }
  }

  const detailOrderSuggestions = detail
    ? (() => {
        const inv = inventoryData.find(d => d.productId === detail.productId)
        if (!inv) return []
        
        const avg = inv.sixMonthAvgShipment ?? 0
        const leadTimeDays = inv.recommendedLeadTimeDays ?? 60
        const effectiveStock = inv.currentStock ?? 0
        const moq = inv.moq ?? 0
        const boxQty = inv.boxQty ?? 1
        
        // 現在の回転率で6ヶ月分を再計算
        return calcMonthlyOrders(avg, turnover, leadTimeDays, effectiveStock, moq, boxQty).map(o => ({
          label: o.label,
          suggestedQty: o.qty,
          estimatedStock: o.estimatedStock,
        }))
      })()
    : []

  const chartInfo = detail ? buildChartData(detail) : null

  return (
    <div style={{ padding: 24 }}>
      {/* 回転率表示 */}
      <div style={{ marginBottom: 12, padding: '8px 12px', background: '#e6f4ff', borderRadius: 6, border: '1px solid #91caff' }}>
        <Space>
          <Text strong>在庫回転率:</Text>
          <Text style={{ fontSize: 16, color: '#1677ff' }}>{turnover}ヶ月</Text>
          <Text type="secondary" style={{ fontSize: 12 }}>（調達提案ページで設定された値を使用）</Text>
        </Space>
      </div>

      {/* 計算方法の説明 */}
      <div style={{ marginBottom: 12, padding: '8px 12px', background: '#f0f5ff', borderRadius: 6, fontSize: 13, color: '#555' }}>
        <Text strong>予測需要数の計算方法：</Text>
        加重移動平均（WMA）— 最新月ほど重みが大きく（重み n）、古い月ほど小さい（重み 1）。
        信頼区間は標準偏差 × 1.5 で算出。
        <Text type="secondary" style={{ marginLeft: 8 }}>※ 列ヘッダーの ℹ にカーソルを合わせると詳細を確認できます</Text>
      </div>

      <Table
        rowKey="productId"
        columns={columns}
        dataSource={forecasts}
        loading={loading}
        pagination={{ pageSize: 20, showSizeChanger: false }}
        scroll={{ x: 1400 }}
        size="small"
      />

      {/* 詳細モーダル */}
      <Modal
        title={detail ? `${detail.productCode} ${detail.productName} — 需要予測詳細` : '需要予測詳細'}
        open={detailOpen}
        onCancel={() => { setDetailOpen(false); setMonthlyDetail(null) }}
        footer={null}
        width={980}
      >
        {detailLoading ? (
          <div style={{ textAlign: 'center', padding: 40 }}><Spin /></div>
        ) : detail ? (
          <>
            {detail.errorMessage ? (
              <Alert type="warning" message={detail.errorMessage} showIcon />
            ) : (
              <>
                {detail.forecast && (
                  <div style={{ marginBottom: 12, padding: '8px 12px', background: '#f6ffed', borderRadius: 6, border: '1px solid #b7eb8f' }}>
                    <Space size={16}>
                      <Text>予測需要数：<Text strong style={{ color: '#52c41a', fontSize: 16 }}>{Math.round(detail.forecast.forecastQty).toLocaleString()}</Text> 個</Text>
                      <Text type="secondary">信頼区間：{Math.round(detail.forecast.confidenceLower)} ~ {Math.round(detail.forecast.confidenceUpper)}</Text>
                    </Space>
                  </div>
                )}

                <div style={{ marginBottom: 8 }}>
                  <Text strong>履歴出荷数と予測</Text>
                  <Text type="secondary" style={{ marginLeft: 8, fontSize: 12 }}>
                    🔴 平均超過の点をクリックすると出荷詳細を確認できます
                  </Text>
                </div>

                {chartInfo && (
                  <div style={{ marginBottom: 20 }}>
                    <ResponsiveContainer width="100%" height={240}>
                      <LineChart data={chartInfo.data} margin={{ top: 5, right: 20, bottom: 5, left: 0 }}>
                        <CartesianGrid strokeDasharray="3 3" />
                        <XAxis dataKey="label" tick={{ fontSize: 11 }} />
                        <YAxis />
                        <RechartsTooltip
                          content={({ active, payload }) => {
                            if (!active || !payload?.length) return null
                            const d = payload[0]?.payload
                            return (
                              <div style={{ background: '#fff', border: '1px solid #ddd', padding: '8px 12px', borderRadius: 4, fontSize: 12 }}>
                                <div><strong>{d.label}</strong></div>
                                {d.actual !== undefined && <div>出荷数: <strong>{d.actual}</strong></div>}
                                {d.avg !== undefined && <div>平均: {d.avg}</div>}
                                {d.actual > d.avg && <div style={{ color: '#ff4d4f' }}>⚠ 平均超過 (+{d.actual - d.avg})</div>}
                                {d.forecast !== undefined && <div>予測: <strong style={{ color: '#52c41a' }}>{d.forecast}</strong></div>}
                                {d.actual > d.avg && <div style={{ color: '#888', marginTop: 4 }}>クリックで詳細を表示</div>}
                              </div>
                            )
                          }}
                        />
                        <Legend />
                        <ReferenceLine y={chartInfo.avg} stroke="#faad14" strokeDasharray="4 4" label={{ value: `平均 ${chartInfo.avg}`, position: 'right', fontSize: 11 }} />
                        <Line
                          type="monotone"
                          dataKey="actual"
                          name="履歴出荷数"
                          stroke="#1677ff"
                          strokeWidth={2}
                          connectNulls={false}
                          dot={(props) => (
                            <CustomDot
                              key={`dot-${props.index}`}
                              {...props}
                              onClickAbove={handleDotClick}
                            />
                          )}
                        />
                        <Line type="monotone" dataKey="forecast" name="予測需要数" stroke="#52c41a" strokeWidth={2} strokeDasharray="5 5" dot={{ r: 5 }} connectNulls={false} />
                        <Line type="monotone" dataKey="upper" name="信頼上界" stroke="#faad14" strokeWidth={1} strokeDasharray="3 3" dot={false} connectNulls={false} />
                        <Line type="monotone" dataKey="lower" name="信頼下界" stroke="#faad14" strokeWidth={1} strokeDasharray="3 3" dot={false} connectNulls={false} />
                      </LineChart>
                    </ResponsiveContainer>
                  </div>
                )}

                {/* 月別詳細パネル */}
                {monthlyDetailLoading && <div style={{ textAlign: 'center', padding: 16 }}><Spin size="small" /></div>}
                {monthlyDetail && !monthlyDetailLoading && (
                  <div style={{ marginBottom: 20, padding: '12px 16px', background: '#fff7e6', borderRadius: 6, border: '1px solid #ffd591' }}>
                    <Title level={5} style={{ margin: '0 0 8px' }}>
                      {monthlyDetail.year}年{monthlyDetail.month}月 出荷詳細
                      <Tag color="red" style={{ marginLeft: 8 }}>平均超過 +{monthlyDetail.totalShipped - Math.round(monthlyDetail.average)}</Tag>
                    </Title>
                    <Descriptions size="small" column={3} style={{ marginBottom: 8 }}>
                      <Descriptions.Item label="合計出荷数">{monthlyDetail.totalShipped.toLocaleString()}</Descriptions.Item>
                      <Descriptions.Item label="月平均">{monthlyDetail.average.toFixed(1)}</Descriptions.Item>
                      <Descriptions.Item label="件数">{monthlyDetail.transactions.length}件</Descriptions.Item>
                    </Descriptions>
                    {monthlyDetail.transactions.length > 0 ? (
                      <Table
                        size="small"
                        pagination={false}
                        dataSource={monthlyDetail.transactions}
                        rowKey="id"
                        columns={[
                          { title: '出荷日', dataIndex: 'transactionDate', key: 'date', width: 160, render: (v: string) => dayjs(v).format('YYYY-MM-DD HH:mm') },
                          { title: '数量', dataIndex: 'quantity', key: 'qty', width: 80, align: 'right', render: (v: number) => v.toLocaleString() },
                          { title: '担当者', dataIndex: 'operatorAccount', key: 'op', width: 100 },
                          { title: '備考', dataIndex: 'remark', key: 'remark', render: (v?: string) => v || '-' },
                        ]}
                      />
                    ) : (
                      <Text type="secondary">この月の出荷トランザクション記録がありません（MonthlyShipmentsのみ）</Text>
                    )}
                  </div>
                )}

                {/* 6ヶ月発注計画 */}
                {detailOrderSuggestions.length > 0 && (
                  <div>
                    <Text strong>今後6ヶ月の発注計画</Text>
                    <ResponsiveContainer width="100%" height={180}>
                      <BarChart
                        data={detailOrderSuggestions.map(s => ({ label: s.label, 発注数: s.suggestedQty, 推定在庫: s.estimatedStock }))}
                        margin={{ top: 5, right: 20, bottom: 5, left: 0 }}
                      >
                        <CartesianGrid strokeDasharray="3 3" />
                        <XAxis dataKey="label" tick={{ fontSize: 11 }} />
                        <YAxis />
                        <RechartsTooltip />
                        <Legend />
                        <Bar dataKey="発注数" fill="#1677ff">
                          {detailOrderSuggestions.map((_, i) => (
                            <Cell key={i} fill={i === 0 ? '#ff4d4f' : '#1677ff'} />
                          ))}
                        </Bar>
                        <Bar dataKey="推定在庫" fill="#95de64" />
                      </BarChart>
                    </ResponsiveContainer>
                  </div>
                )}
              </>
            )}
          </>
        ) : null}
      </Modal>
    </div>
  )
}