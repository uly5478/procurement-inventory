import { useEffect } from 'react'
import { Modal, Form, Input, message } from 'antd'
import { createProduct, updateProduct } from '../../api/products'
import type { Product } from '../../types'

interface Props {
  open: boolean
  editingProduct: Product | null
  onClose: () => void
  onSuccess: () => void
}

export default function ProductFormModal({ open, editingProduct, onClose, onSuccess }: Props) {
  const [form] = Form.useForm()
  const isEdit = editingProduct !== null

  useEffect(() => {
    if (open) {
      if (editingProduct) {
        form.setFieldsValue({
          productCode: editingProduct.productCode,
          name: editingProduct.name,
          unit: editingProduct.unit,
        })
      } else {
        form.resetFields()
      }
    }
  }, [open, editingProduct, form])

  async function handleOk() {
    try {
      const values = await form.validateFields()
      if (isEdit) {
        await updateProduct(editingProduct!.id, { name: values.name, unit: values.unit })
        message.success('產品已更新')
      } else {
        await createProduct(values)
        message.success('產品已新增')
      }
      onSuccess()
    } catch (err: unknown) {
      if (err && typeof err === 'object' && 'errorFields' in err) return // form validation error
      const axiosErr = err as { response?: { data?: { message?: string } } }
      message.error(axiosErr?.response?.data?.message ?? '操作失敗，請稍後再試')
    }
  }

  return (
    <Modal
      title={isEdit ? '編輯產品' : '新增產品'}
      open={open}
      onOk={handleOk}
      onCancel={onClose}
      okText="儲存"
      cancelText="取消"
      destroyOnClose
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item
          label="產品編號"
          name="productCode"
          rules={[{ required: true, message: '請輸入產品編號' }]}
        >
          <Input disabled={isEdit} placeholder="例：P001" />
        </Form.Item>
        <Form.Item
          label="產品名稱"
          name="name"
          rules={[{ required: true, message: '請輸入產品名稱' }]}
        >
          <Input placeholder="例：螺絲 M3x10" />
        </Form.Item>
        <Form.Item
          label="單位"
          name="unit"
          rules={[{ required: true, message: '請輸入單位' }]}
        >
          <Input placeholder="例：個、箱、公斤" />
        </Form.Item>
      </Form>
    </Modal>
  )
}
