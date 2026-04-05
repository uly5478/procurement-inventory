import { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { Button, Table, Modal, message, Space, Tag } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import type { ProductSupplierPrice } from '../../types'
import {
  getProductSuppliers,
  addSupplierPrice,
  updateSupplierPrice,
} from '../../api/suppliers'
import type { CreateSupplierPriceDto, UpdateSupplierPriceDto } from '../../api/suppliers'
import SupplierPriceFormModal from './SupplierPriceFormModal'

export default function SupplierPricePage() {
  const { id } = useParams<{ id: string }>()
  const productId = Number(id)
  const navigate = useNavigate()

  const [prices, setPrices] = useState<ProductSupplierPrice[]>([])
  const [loading, setLoading] = useState(false)
  const [modalOpen, setModalOpen] = useState(false)
  const [editingRecord, setEditingRecord] = useState<ProductSupplierPrice | null>(null)
  const [submitting, setSubmitting] = useState(false)

  const fetchPrices = async () => {
    setLoading(true)
    try {
      const result = await getProductSuppliers(productId)
      setPrices(result.items)
    } catch {
      message.error('仕入先単価の読み込みに失敗しました')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchPrices()
  }, [productId])

  const handleAdd = () => {
    setEditingRecord(null)
    setModalOpen(true)
  }

  const handleEdit = (record: ProductSupplierPrice) => {
    setEditingRecord(record)
    setModalOpen(true)
  }

  const handleSubmit = async (values: {
    supplierName: string
    unitPrice: number
    currency: string
    minOrderQty: number
    leadTimeDays: number
  }) => {
    setSubmitting(true)
    try {
      if (editingRecord) {
        const dto: UpdateSupplierPriceDto = {
          unitPrice: values.unitPrice,
          currency: values.currency,
          minOrderQty: values.minOrderQty,
          leadTimeDays: values.leadTimeDays,
        }
        await updateSupplierPrice(editingRecord.id, dto)
        message.success('仕入先単価を更新しました')
        setModalOpen(false)
        fetchPrices()
      } else {
        await doAddSupplierPrice(values, false)
      }
    } catch {
      message.error('操作に失敗しました')
    } finally {
      setSubmitting(false)
    }
  }

  const doAddSupplierPrice = async (
    values: {
      supplierName: string
      unitPrice: number
      currency: string
      minOrderQty: number
      leadTimeDays: number
    },
    forceCreate: boolean,
  ) => {
    const dto: CreateSupplierPriceDto = { ...values, forceCreate }
    const result = await addSupplierPrice(productId, dto)

    if (result.requireConfirmation) {
      setModalOpen(false)
      Modal.confirm({
        title: '仕入先数の警告',
        content: result.warning ?? 'この商品には既に4社の仕入先が登録されています。5社目を追加しますか？',
        okText: '追加する',
        cancelText: 'キャンセル',
        onOk: async () => {
          setSubmitting(true)
          try {
            await doAddSupplierPrice(values, true)
          } finally {
            setSubmitting(false)
          }
        },
      })
    } else {
      message.success('仕入先単価を追加しました')
      setModalOpen(false)
      fetchPrices()
    }
  }

  const columns: ColumnsType<ProductSupplierPrice> = [
    {
      title: '仕入先名',
      dataIndex: 'supplierName',
      key: 'supplierName',
    },
    {
      title: '単価',
      dataIndex: 'unitPrice',
      key: 'unitPrice',
      render: (val: number) => val.toLocaleString(),
    },
    {
      title: '通貨',
      dataIndex: 'currency',
      key: 'currency',
      render: (val: string) => <Tag>{val}</Tag>,
    },
    {
      title: '最小発注数',
      dataIndex: 'minOrderQty',
      key: 'minOrderQty',
    },
    {
      title: 'リードタイム（日）',
      dataIndex: 'leadTimeDays',
      key: 'leadTimeDays',
    },
    {
      title: '適用開始日',
      dataIndex: 'effectiveDate',
      key: 'effectiveDate',
      render: (val: string) => new Date(val).toLocaleDateString('ja-JP'),
    },
    {
      title: '操作',
      key: 'actions',
      render: (_, record) => (
        <Button type="link" onClick={() => handleEdit(record)}>
          編集
        </Button>
      ),
    },
  ]

  return (
    <div style={{ padding: 24 }}>
      <Space style={{ marginBottom: 16, justifyContent: 'space-between', width: '100%' }}>
        <Space>
          <Button onClick={() => navigate('/products')}>商品一覧に戻る</Button>
          <h2 style={{ margin: 0 }}>商品ID: {productId} の仕入先単価</h2>
        </Space>
        <Button type="primary" onClick={handleAdd}>
          仕入先単価を追加
        </Button>
      </Space>

      <Table
        rowKey="id"
        columns={columns}
        dataSource={prices}
        loading={loading}
        pagination={false}
      />

      <SupplierPriceFormModal
        open={modalOpen}
        editingRecord={editingRecord}
        onCancel={() => setModalOpen(false)}
        onSubmit={handleSubmit}
        confirmLoading={submitting}
      />
    </div>
  )
}