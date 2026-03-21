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
} from 'antd'
import { WarningOutlined, SettingOutlined, CheckOutlined, CloseOutlined } from '@ant-design/icons'
import type { ColumnsType } from 'antd/es/table'
import { getSuggestions, manualOverride, getSettings, updateSettings } from '../../api/procurement'
import type { ProcurementSuggestion, ProcurementSettings } from '../../types'

const { Text } = Typography

export default function SuggestionsPage() {
  const [suggestions, setSuggestions] = useState<ProcurementSuggestion[]>([])
  const [loading, setLoading] = useState(false)
  const [settings, setSettings] = useState<ProcurementSettings | null>(null)
  const [settingsModalOpen, setSettingsModalOpen] = useState(false)
  const [settingsSaving, setSettingsSaving] = useState(false)

  // inline edit state: productId -> pending qty
  const [editingId, setEditingId] = useState<number | null>(null)
  const [editingQty, setEditingQty] = useState<number>(0)

  const [form] = Form.useForm()

  const fetchSuggestions = useCallback(async () => {
    setLoading(true)
    try {
      const data = await getSuggestions()
      setSuggestions(data)
    } catch {
      message.error('載入採購建議失敗')
    } finally {
      setLoading(false)
    }
  }, [])

  const fetchSettings = useCallback(async () => {
    try {
      const data = await getSettings()
      setSettings(data)
    } catch {
      message.error('載入設定失敗')
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

  function cancelEdit() {
    setEditingId(null)
  }

  async function confirmEdit(productId: number) {
    try {
      await manualOverride(productId, editingQty)
      message.success('已更新手動採購量')
      setEditingId(null)
      fetchSuggestions()
    } catch {
      message.error('更新失敗，請稍後再試')
    }
  }

  function openSettingsModal() {
    form.setFieldsValue({ defaultTurnoverMonths: settings?.defaultTurnoverMonths ?? 2.5 })
    setSettingsModalOpen(true)
  }

  async function saveSettings() {
    const values = await form.validateFields()
    setSettingsSaving(true)
    try {
      await updateSettings({ defaultTurnoverMonths: values.defaultTurnoverMonths })
      message.success('設定已儲存')
      setSettingsModalOpen(false)
      await fetchSettings()
      await fetchSuggestions()
    } catch {
      message.error('儲存設定失敗')
    } finally {
      setSettingsSaving(false)
    }
  }

  const columns: ColumnsType<ProcurementSuggestion> = [
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
      title: '當前庫存',
      dataIndex: 'currentStock',
      key: 'currentStock',
      width: 100,
      align: 'right',
    },
    {
      title: '六個月平均出貨量',
      dataIndex: 'sixMonthAvgShipment',
      key: 'sixMonthAvgShipment',
      width: 160,
      align: 'right',
      render: (val: number, record) => (
        <Space size={4}>
          <span>{val.toFixed(1)}</span>
          {record.dataInsufficient && (
            <Tooltip
              title={`資料不足：僅有 ${record.availableMonths ?? '?'} 個月出貨記錄，以現有月份平均計算`}
            >
              <WarningOutlined style={{ color: '#faad14' }} />
            </Tooltip>
          )}
        </Space>
      ),
    },
    {
      title: '建議採購量',
      key: 'suggestedQty',
      width: 200,
      render: (_, record) => {
        if (editingId === record.productId) {
          return (
            <Space>
              <InputNumber
                min={0}
                value={editingQty}
                onChange={(v) => setEditingQty(v ?? 0)}
                style={{ width: 90 }}
                autoFocus
              />
              <Button
                type="primary"
                size="small"
                icon={<CheckOutlined />}
                onClick={() => confirmEdit(record.productId)}
              />
              <Button size="small" icon={<CloseOutlined />} onClick={cancelEdit} />
            </Space>
          )
        }

        if (record.isManualOverride) {
          return (
            <Space size={6}>
              <Text strong style={{ color: '#1677ff' }}>
                {record.manualOverrideQty}
              </Text>
              <Text type="secondary" style={{ fontSize: 12 }}>
                (系統: {record.systemSuggestedQty})
              </Text>
            </Space>
          )
        }

        return <span>{record.systemSuggestedQty}</span>
      },
    },
    {
      title: '操作',
      key: 'actions',
      width: 110,
      render: (_, record) =>
        editingId === record.productId ? null : (
          <Button size="small" onClick={() => startEdit(record)}>
            手動修改
          </Button>
        ),
    },
  ]

  return (
    <div style={{ padding: 24 }}>
      <div style={{ display: 'flex', justifyContent: 'flex-end', marginBottom: 16 }}>
        <Button icon={<SettingOutlined />} onClick={openSettingsModal}>
          庫存迴轉率設定
          {settings ? `（${settings.defaultTurnoverMonths} 個月）` : ''}
        </Button>
      </div>

      <Table
        rowKey="productId"
        columns={columns}
        dataSource={suggestions}
        loading={loading}
        pagination={{ pageSize: 20, showSizeChanger: false }}
      />

      <Modal
        title="庫存迴轉率設定"
        open={settingsModalOpen}
        onOk={saveSettings}
        onCancel={() => setSettingsModalOpen(false)}
        okText="儲存"
        cancelText="取消"
        confirmLoading={settingsSaving}
      >
        <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
          <Form.Item
            label="庫存迴轉率（月）"
            name="defaultTurnoverMonths"
            rules={[
              { required: true, message: '請輸入迴轉率' },
              {
                type: 'number',
                min: 1.0,
                max: 6.0,
                message: '迴轉率範圍為 1.0 至 6.0 個月',
              },
            ]}
          >
            <InputNumber min={1.0} max={6.0} step={0.5} precision={1} style={{ width: 160 }} />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  )
}
