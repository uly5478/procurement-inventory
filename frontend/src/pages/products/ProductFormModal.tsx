import { useEffect } from 'react'
import { Modal, Form, Input, InputNumber, message } from 'antd'
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
          boxQty: editingProduct.boxQty ?? 1,
          moq: editingProduct.moq ?? 1,
          safetyStock: editingProduct.safetyStock ?? 0,
          averageShipment: editingProduct.averageShipment ?? 0,
          categoryCode: editingProduct.categoryCode ?? '',
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
        await updateProduct(editingProduct!.id, { 
          name: values.name, 
          unit: values.unit,
          boxQty: values.boxQty,
          moq: values.moq,
          safetyStock: values.safetyStock,
          averageShipment: values.averageShipment,
          categoryCode: values.categoryCode || undefined,
        })
        message.success('商品を更新しました')
      } else {
        await createProduct(values)
        message.success('商品を追加しました')
      }
      onSuccess()
    } catch (err: unknown) {
      if (err && typeof err === 'object' && 'errorFields' in err) return
      const axiosErr = err as { response?: { data?: { message?: string } } }
      message.error(axiosErr?.response?.data?.message ?? '操作に失敗しました')
    }
  }

  return (
    <Modal
      title={isEdit ? '商品編集' : '商品追加'}
      open={open}
      onOk={handleOk}
      onCancel={onClose}
      okText="保存"
      cancelText="キャンセル"
      destroyOnClose
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item
          label="商品コード"
          name="productCode"
          rules={[{ required: true, message: '商品コードを入力してください' }]}
        >
          <Input disabled={isEdit} placeholder="例：P001" />
        </Form.Item>
        <Form.Item
          label="商品名"
          name="name"
          rules={[{ required: true, message: '商品名を入力してください' }]}
        >
          <Input placeholder="例：ネジ M3x10" />
        </Form.Item>
        <Form.Item
          label="単位"
          name="unit"
          rules={[{ required: true, message: '単位を入力してください' }]}
        >
          <Input placeholder="例：個、箱、kg" />
        </Form.Item>
        <Form.Item
          label="箱入数"
          name="boxQty"
          rules={[{ required: true, message: '箱入数を入力してください' }]}
          initialValue={1}
        >
          <InputNumber min={1} style={{ width: '100%' }} placeholder="1箱あたりの入数" />
        </Form.Item>
        <Form.Item
          label="MOQ"
          name="moq"
          rules={[{ required: true, message: 'MOQを入力してください' }]}
          initialValue={1}
        >
          <InputNumber min={1} style={{ width: '100%' }} placeholder="最小発注数量" />
        </Form.Item>
        <Form.Item
          label="安全在庫"
          name="safetyStock"
          rules={[{ required: true, message: '安全在庫を入力してください' }]}
          initialValue={0}
        >
          <InputNumber min={0} style={{ width: '100%' }} placeholder="安全在庫数量" />
        </Form.Item>
        <Form.Item
          label="平均出荷数"
          name="averageShipment"
          rules={[{ required: true, message: '平均出荷数を入力してください' }]}
          initialValue={0}
        >
          <InputNumber min={0} precision={2} style={{ width: '100%' }} placeholder="平均出荷数" />
        </Form.Item>
        <Form.Item
          label="仕入分類コード"
          name="categoryCode"
        >
          <Input placeholder="例: VN, CN, JP" />
        </Form.Item>
      </Form>
    </Modal>
  )
}