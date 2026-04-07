import { useState, useEffect, useCallback } from 'react'
import {
  Table,
  Button,
  InputNumber,
  Space,
  Modal,
  Form,
  message,
  Tooltip,
  Typography,
  Tag,
  Radio,
} from 'antd'
import { WarningOutlined, CheckOutlined, CloseOutlined, ShoppingCartOutlined, UndoOutlined } from '@ant-design/icons'
import type { ColumnsType } from 'antd/es/table'
import dayjs from 'dayjs'
import { useNavigate } from 'react-router-dom'
import { getSuggestions, manualOverride, getSettings, updateSettings, resetOverride } from '../../api/procurement'
import { getProductSuppliers } from '../../api/suppliers'
import { createPurchaseOrder } from '../../api/purchaseOrders'
import type { ProcurementSuggestion, ProcurementSettings } from '../../types'

const { Text } = Typography

const CURRENCY_COLOR: Record<string, string> = {
  CNY: 'red',
  TWD: 'blue',
  USD: 'green',
  EUR: 'purple',
  JPY: 'orange',
}

const TURNOVER_OPTIONS = [2.5, 3, 3.5, 4, 4.5]

// 今月から6ヶ月分のラベル
const SIX_MONTHS = Array.from({ length: 6 }, (_, i) => {
  const d = dayjs().add(i, 'month')
  return { label: d.format('YYYY/MM'), idx: i }
})

/** 合計発注数を 60:40 + BoxQty丸めで第1・第2に分配 */
function splitOrderQty(total: number, boxQty: number, hasSupplier2: boolean) {
  if (total <= 0) return { s1: 0, s2: 0 }
  const bq = boxQty > 0 ? boxQty : 1
  const raw1 = Math.ceil((total * 0.6) / bq) * bq
  const raw2 = total - raw1
  const s2 = hasSupplier2 && raw2 > 0 ? Math.ceil(raw2 / bq) * bq : 0
  return { s1: raw1, s2 }
}

/** 回転率・リードタイムから発注数を計算（在庫一覧と同じ公式） */
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

/** 6ヶ月分の発注計画を再計算 */
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

export default function SuggestionsPage() {
  const [suggestions, setSuggestions] = useState<ProcurementSuggestion[]>([])
  const [loading, setLoading] = useState(false)
  const [, setSettings] = useState<ProcurementSettings | null>(null)
  const navigate = useNavigate()

  // 選択中の回転率（フロントエンドで管理）
  const [turnover, setTurnover] = useState<number>(2.5)
  // 選択中の発注月
  const [selectedMonth, setSelectedMonth] = useState<string>(SIX_MONTHS[0].label)

  const [editingId, setEditingId] = useState<number | null>(null)
  const [editingQty, setEditingQty] = useState<number>(0)
  const [settingsModalOpen, setSettingsModalOpen] = useState(false)
  const [settingsSaving, setSettingsSaving] = useState(false)
  const [form] = Form.useForm()

  const fetchSuggestions = useCallback(async () => {
    setLoading(true)
    try {
      const data = await getSuggestions()
      setSuggestions(data)
    } catch {
      message.error('調達提案の読み込みに失敗しました')
    } finally {
      setLoading(false)
    }
  }, [])

  const fetchSettings = useCallback(async () => {
    try {
      const data = await getSettings()
      setSettings(data)
      setTurnover(data.defaultTurnoverMonths)
    } catch {
      message.error('設定の読み込みに失敗しました')
    }
  }, [])

  useEffect(() => {
    fetchSuggestions()
    fetchSettings()
  }, [fetchSuggestions, fetchSettings])

  function startEdit(record: ProcurementSuggestion) {
    setEditingId(record.productId)
    setEditingQty(record.manualOverrideQty ?? record.systemSuggestedQty)
  }

  function cancelEdit() { setEditingId(null) }

  async function confirmEdit(productId: number) {
    try {
      await manualOverride(productId, editingQty)
      message.success('発注数量を更新しました')
      setEditingId(null)
      fetchSuggestions()
    } catch {
      message.error('更新に失敗しました')
    }
  }

  async function handleReset(productId: number) {
    try {
      await resetOverride(productId)
      message.success('システム計算値にリセットしました')
      fetchSuggestions()
    } catch {
      message.error('リセットに失敗しました')
    }
  }

  /** 調達提案から直接発注を作成 */
  async function handleCreateOrder(record: ProcurementSuggestion) {
    const total = getCalcOrderQty(record)
    if (total <= 0) {
      message.info('この月の発注は不要です')
      return
    }

    // 第1仕入先の supplierId を取得
    if (!record.recommendedSupplierName) {
      message.warning('仕入先が登録されていません')
      return
    }

    try {
      const result = await getProductSuppliers(record.productId)
      const prices = result.items.filter(s => s.isCurrent).sort((a, b) => a.unitPrice - b.unitPrice)
      if (prices.length === 0) {
        message.warning('仕入先単価が登録されていません')
        return
      }

      const { s1, s2 } = splitOrderQty(total, record.boxQty, !!record.supplier2Name)

      // 第1仕入先で発注作成
      const supplier1 = prices[0]
      await createPurchaseOrder({
        supplierId: supplier1.supplierId,
        items: [{ productId: record.productId, quantity: s1, unitPrice: supplier1.unitPrice }],
      })

      // 第2仕入先がある場合
      if (record.supplier2Name && s2 > 0 && prices.length > 1) {
        const supplier2 = prices[1]
        await createPurchaseOrder({
          supplierId: supplier2.supplierId,
          items: [{ productId: record.productId, quantity: s2, unitPrice: supplier2.unitPrice }],
        })
        message.success(`発注を2件作成しました（第1: ${s1}個, 第2: ${s2}個）`)
      } else {
        message.success(`発注を作成しました（${s1}個）`)
      }

      navigate('/procurement/orders')
    } catch {
      message.error('発注の作成に失敗しました')
    }
  }

  async function saveSettings() {
    const values = await form.validateFields()
    setSettingsSaving(true)
    try {
      await updateSettings({ defaultTurnoverMonths: values.defaultTurnoverMonths })
      message.success('設定を保存しました')
      setSettingsModalOpen(false)
      await fetchSettings()
      await fetchSuggestions()
    } catch {
      message.error('設定の保存に失敗しました')
    } finally {
      setSettingsSaving(false)
    }
  }

  /** 選択月・回転率で再計算した発注数を取得（手動変更がある場合はそちらを優先） */
  function getCalcOrderQty(record: ProcurementSuggestion): number {
    if (record.isManualOverride && record.manualOverrideQty !== undefined) {
      return record.manualOverrideQty
    }
    return getMonthlyCalcQty(record, selectedMonth)
  }

  /** 6ヶ月分の発注数 — 回転率が変わった場合のみ全月再計算、それ以外はサーバー値を使用 */
  function getMonthlyCalcQty(record: ProcurementSuggestion, label: string): number {
    const avg = record.sixMonthAvgShipment
    const leadTimeDays = record.recommendedLeadTimeDays ?? 60
    const effectiveStock = record.currentStock

    // サーバーのデフォルト回転率と現在の回転率が同じ場合はサーバー値をそのまま使用
    // （回転率が変わった場合のみ全月再計算）
    const serverVal = record.monthlyOrderSuggestions?.find(s => s.label === label)
    const serverTurnover = record.turnoverMonths ?? 2.5

    if (Math.abs(turnover - serverTurnover) < 0.01) {
      // 回転率変更なし → サーバー値をそのまま返す
      return serverVal ? serverVal.suggestedQty : 0
    }

    // 回転率変更あり → 全月を新しい回転率で再計算
    const orders = calcMonthlyOrders(avg, turnover, leadTimeDays, effectiveStock, record.moq, record.boxQty)
    const found = orders.find(o => o.label === label)
    return found ? found.qty : 0
  }

  const columns: ColumnsType<ProcurementSuggestion> = [
    {
      title: '商品コード',
      dataIndex: 'productCode',
      key: 'productCode',
      width: 110,
      fixed: 'left',
    },
    {
      title: '商品名',
      dataIndex: 'productName',
      key: 'productName',
      width: 130,
      fixed: 'left',
    },
    {
      title: '平均出荷数',
      dataIndex: 'sixMonthAvgShipment',
      key: 'sixMonthAvgShipment',
      width: 95,
      align: 'right',
      render: (val: number, record) => (
        <Space size={4}>
          <span>{val > 0 ? val.toFixed(1) : '-'}</span>
          {record.dataInsufficient && (
            <Tooltip title={`データ不足：${record.availableMonths ?? '?'}ヶ月分のみ`}>
              <WarningOutlined style={{ color: '#faad14' }} />
            </Tooltip>
          )}
        </Space>
      ),
    },
    // 第1仕入先
    {
      title: <div><div>第1仕入先</div><div style={{ fontSize: 11, color: '#888' }}>（最安値・60%）</div></div>,
      key: 'supplier1',
      width: 170,
      render: (_, record) => {
        if (record.noSupplier || !record.recommendedSupplierName) {
          return <Text type="secondary">仕入先未登録</Text>
        }
        return (
          <Space direction="vertical" size={2}>
            <Text strong>{record.recommendedSupplierName}</Text>
            <Space size={4}>
              <Tag color={CURRENCY_COLOR[record.recommendedCurrency ?? 'CNY'] ?? 'default'}>{record.recommendedCurrency}</Tag>
              <Text type="secondary">{record.recommendedUnitPrice?.toFixed(4)}</Text>
              {record.recommendedLeadTimeDays && <Text type="secondary">{record.recommendedLeadTimeDays}日</Text>}
            </Space>
          </Space>
        )
      },
    },
    {
      title: '第1発注数',
      key: 'supplier1OrderQty',
      width: 90,
      align: 'right',
      render: (_, record) => {
        if (record.noSupplier) return <Text type="secondary">-</Text>
        const total = getCalcOrderQty(record)
        const { s1 } = splitOrderQty(total, record.boxQty, !!record.supplier2Name)
        return s1 > 0
          ? <Text strong style={{ color: '#1677ff' }}>{s1.toLocaleString()}</Text>
          : <Tag color="success" style={{ margin: 0 }}>不要</Tag>
      },
    },
    // 第2仕入先
    {
      title: <div><div>第2仕入先</div><div style={{ fontSize: 11, color: '#888' }}>（2番目・40%）</div></div>,
      key: 'supplier2',
      width: 170,
      render: (_, record) => {
        if (!record.supplier2Name) return <Text type="secondary">-</Text>
        return (
          <Space direction="vertical" size={2}>
            <Text strong>{record.supplier2Name}</Text>
            <Space size={4}>
              <Tag color={CURRENCY_COLOR[record.supplier2Currency ?? 'CNY'] ?? 'default'}>{record.supplier2Currency}</Tag>
              <Text type="secondary">{record.supplier2UnitPrice?.toFixed(4)}</Text>
              {record.supplier2LeadTimeDays && <Text type="secondary">{record.supplier2LeadTimeDays}日</Text>}
            </Space>
          </Space>
        )
      },
    },
    {
      title: '第2発注数',
      key: 'supplier2OrderQty',
      width: 90,
      align: 'right',
      render: (_, record) => {
        if (!record.supplier2Name) return <Text type="secondary">-</Text>
        const total = getCalcOrderQty(record)
        const { s2 } = splitOrderQty(total, record.boxQty, true)
        return s2 > 0
          ? <Text strong style={{ color: '#1677ff' }}>{s2.toLocaleString()}</Text>
          : <Tag color="success" style={{ margin: 0 }}>不要</Tag>
      },
    },
    // 合計提案発注数（選択月）
    {
      title: (
        <div>
          <div>合計提案発注数</div>
          <div style={{ fontSize: 11, color: '#888' }}>（{selectedMonth}）</div>
        </div>
      ),
      key: 'suggestedQty',
      width: 145,
      render: (_, record) => {
        if (editingId === record.productId) {
          return (
            <Space>
              <InputNumber min={0} value={editingQty} onChange={(v) => setEditingQty(v ?? 0)} style={{ width: 90 }} autoFocus />
              <Button type="primary" size="small" icon={<CheckOutlined />} onClick={() => confirmEdit(record.productId)} />
              <Button size="small" icon={<CloseOutlined />} onClick={cancelEdit} />
            </Space>
          )
        }
        const qty = getCalcOrderQty(record)
        if (record.isManualOverride) {
          return (
            <Space size={6}>
              <Text strong style={{ color: '#1677ff' }}>{record.manualOverrideQty}</Text>
              <Text type="secondary" style={{ fontSize: 12 }}>(計算: {qty})</Text>
            </Space>
          )
        }
        return qty > 0
          ? <Text strong style={{ color: '#cf1322' }}>{qty.toLocaleString()}</Text>
          : <Tag color="success">発注不要</Tag>
      },
    },
    // 6ヶ月分の発注数列
    ...SIX_MONTHS.map(({ label, idx }) => ({
      title: (
        <div style={{ textAlign: 'center' as const }}>
          <div style={{ fontSize: 11, color: label === selectedMonth ? '#1677ff' : '#888' }}>
            {idx === 0 ? '今月' : `+${idx}M`}
          </div>
          <div style={{ color: label === selectedMonth ? '#1677ff' : undefined, fontWeight: label === selectedMonth ? 'bold' : undefined }}>
            {label}
          </div>
        </div>
      ),
      key: `month_${label}`,
      width: 85,
      align: 'right' as const,
      onHeaderCell: () => ({
        style: { background: label === selectedMonth ? '#e6f4ff' : undefined, cursor: 'pointer' },
        onClick: () => setSelectedMonth(label),
      }),
      render: (_: unknown, record: ProcurementSuggestion) => {
        const qty = getMonthlyCalcQty(record, label)
        if (qty === 0) return <Tag color="success" style={{ margin: 0 }}>不要</Tag>
        return (
          <Text strong style={{ color: label === selectedMonth ? '#cf1322' : '#1677ff' }}>
            {qty.toLocaleString()}
          </Text>
        )
      },
    })),
    {
      title: '操作',
      key: 'actions',
      width: 170,
      fixed: 'right',
      render: (_, record) =>
        editingId === record.productId ? null : (
          <Space size={4}>
            <Button size="small" onClick={() => startEdit(record)}>手動変更</Button>
            <Button
              size="small"
              icon={<UndoOutlined />}
              onClick={() => handleReset(record.productId)}
              title="システム計算値に戻す"
            >
              リセット
            </Button>
            <Button
              size="small"
              type="primary"
              icon={<ShoppingCartOutlined />}
              onClick={() => handleCreateOrder(record)}
              disabled={getCalcOrderQty(record) <= 0}
            >
              発注
            </Button>
          </Space>
        ),
    },
  ]

  return (
    <div style={{ padding: 24 }}>
      {/* コントロールバー */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16, flexWrap: 'wrap', gap: 8 }}>
        <Space wrap>
          {/* 発注月選択 */}
          <Text type="secondary" style={{ fontSize: 13 }}>発注月：</Text>
          <Radio.Group value={selectedMonth} onChange={(e) => setSelectedMonth(e.target.value)} optionType="button" buttonStyle="solid" size="small">
            {SIX_MONTHS.map(({ label, idx }) => (
              <Radio.Button key={label} value={label}>
                {idx === 0 ? `今月 (${label})` : label}
              </Radio.Button>
            ))}
          </Radio.Group>
        </Space>

        <Space wrap>
          {/* 回転率選択 */}
          <Text type="secondary" style={{ fontSize: 13 }}>在庫回転率：</Text>
          <Radio.Group
            value={turnover}
            onChange={(e) => setTurnover(e.target.value)}
            optionType="button"
            buttonStyle="solid"
            size="small"
          >
            {TURNOVER_OPTIONS.map(v => (
              <Radio.Button key={v} value={v}>{v}ヶ月</Radio.Button>
            ))}
          </Radio.Group>
          <Button
            size="small"
            onClick={() => {
              form.setFieldsValue({ defaultTurnoverMonths: turnover })
              setSettingsModalOpen(true)
            }}
          >
            デフォルト保存
          </Button>
        </Space>
      </div>

      <Table
        rowKey="productId"
        columns={columns}
        dataSource={suggestions}
        loading={loading}
        pagination={{ pageSize: 20, showSizeChanger: false }}
        scroll={{ x: 1900 }}
        size="small"
      />

      <Modal
        title="デフォルト在庫回転率を保存"
        open={settingsModalOpen}
        onOk={saveSettings}
        onCancel={() => setSettingsModalOpen(false)}
        okText="保存"
        cancelText="キャンセル"
        confirmLoading={settingsSaving}
      >
        <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
          <Form.Item label="在庫回転率（月）" name="defaultTurnoverMonths"
            rules={[{ required: true }, { type: 'number', min: 1.0, max: 6.0 }]}
          >
            <InputNumber min={1.0} max={6.0} step={0.5} precision={1} style={{ width: 160 }} />
          </Form.Item>
        </Form>
        <Text type="secondary">現在の選択値 {turnover}ヶ月 をデフォルトとして保存します。</Text>
      </Modal>
    </div>
  )
}