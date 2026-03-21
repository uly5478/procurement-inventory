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

const CURRENCIES = ['TWD', 'USD', 'EUR', 'JPY']

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
        form.setFieldValue('currency', 'TWD')
      }
    }
  }, [open, editingRecord, form])

  const handleOk = async () => {
    const values = await form.validateFields()
    onSubmit(values)
  }

  return (
    <Modal
      title={isEdit ? '編輯廠商報價' : '新增廠商報價'}
      open={open}
      onOk={handleOk}
      onCancel={onCancel}
      confirmLoading={confirmLoading}
      okText="確認"
      cancelText="取消"
      destroyOnClose
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item
          name="supplierName"
          label="廠商名稱"
          rules={[{ required: true, message: '請輸入廠商名稱' }]}
        >
          <Input disabled={isEdit} placeholder="請輸入廠商名稱" />
        </Form.Item>

        <Form.Item
          name="unitPrice"
          label="買價"
          rules={[{ required: true, message: '請輸入買價' }]}
        >
          <InputNumber
            min={0}
            precision={2}
            style={{ width: '100%' }}
            placeholder="請輸入買價"
          />
        </Form.Item>

        <Form.Item
          name="currency"
          label="幣別"
          rules={[{ required: true, message: '請選擇幣別' }]}
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
          label="最小訂購量"
          rules={[{ required: true, message: '請輸入最小訂購量' }]}
        >
          <InputNumber min={1} style={{ width: '100%' }} placeholder="請輸入最小訂購量" />
        </Form.Item>

        <Form.Item
          name="leadTimeDays"
          label="交期天數"
          rules={[{ required: true, message: '請輸入交期天數' }]}
        >
          <InputNumber min={1} style={{ width: '100%' }} placeholder="請輸入交期天數" />
        </Form.Item>
      </Form>
    </Modal>
  )
}
