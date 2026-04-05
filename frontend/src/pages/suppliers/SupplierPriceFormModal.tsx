import { useEffect } from 'react'
import { Modal, Form, Input, InputNumber, Select } from 'antd'
import type { ProductSupplierPrice } from '../../types'

interface Props {
  open: boolean
  editingRecord: ProductSupplierPrice | null
  onCancel: () => void
  onSubmit: (values: {
    supplierName: string
    unitPrice: number
    currency: string
    minOrderQty: number
    leadTimeDays: number
  }) => void
  confirmLoading?: boolean
}

const CURRENCIES = ['CNY', 'TWD', 'USD', 'EUR', 'JPY']

export default function SupplierPriceFormModal({
  open,
  editingRecord,
  onCancel,
  onSubmit,
  confirmLoading,
}: Props) {
  const [form] = Form.useForm()
  const isEdit = editingRecord !== null

  useEffect(() => {
    if (open) {
      if (editingRecord) {
        form.setFieldsValue({
          supplierName: editingRecord.supplierName,
          unitPrice: editingRecord.unitPrice,
          currency: editingRecord.currency,
          minOrderQty: editingRecord.minOrderQty,
          leadTimeDays: editingRecord.leadTimeDays,
        })
      } else {
        form.resetFields()
        form.setFieldValue('currency', 'CNY')
      }
    }
  }, [open, editingRecord, form])

  const handleOk = async () => {
    const values = await form.validateFields()
    onSubmit(values)
  }

  return (
    <Modal
      title={isEdit ? '仕入先単価の編集' : '仕入先単価の追加'}
      open={open}
      onOk={handleOk}
      onCancel={onCancel}
      confirmLoading={confirmLoading}
      okText="保存"
      cancelText="キャンセル"
      destroyOnClose
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item
          name="supplierName"
          label="仕入先名"
          rules={[{ required: true, message: '仕入先名を入力してください' }]}
        >
          <Input disabled={isEdit} placeholder="仕入先名を入力" />
        </Form.Item>

        <Form.Item
          name="unitPrice"
          label="単価"
          rules={[{ required: true, message: '単価を入力してください' }]}
        >
          <InputNumber
            min={0}
            precision={2}
            style={{ width: '100%' }}
            placeholder="単価を入力"
          />
        </Form.Item>

        <Form.Item
          name="currency"
          label="通貨"
          rules={[{ required: true, message: '通貨を選択してください' }]}
        >
          <Select>
            {CURRENCIES.map((c) => (
              <Select.Option key={c} value={c}>
                {c}
              </Select.Option>
            ))}
          </Select>
        </Form.Item>

        <Form.Item
          name="minOrderQty"
          label="最小発注数"
          rules={[{ required: true, message: '最小発注数を入力してください' }]}
        >
          <InputNumber min={1} style={{ width: '100%' }} placeholder="最小発注数" />
        </Form.Item>

        <Form.Item
          name="leadTimeDays"
          label="リードタイム（日）"
          rules={[{ required: true, message: 'リードタイムを入力してください' }]}
        >
          <InputNumber min={1} style={{ width: '100%' }} placeholder="リードタイム" />
        </Form.Item>
      </Form>
    </Modal>
  )
}