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
      message.error('載入廠商報價失敗')
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
        // 更新報價
        const dto: UpdateSupplierPriceDto = {
          unitPrice: values.unitPrice,
          currency: values.currency,
          minOrderQty: values.minOrderQty,
          leadTimeDays: values.leadTimeDays,
        }
        await updateSupplierPrice(editingRecord.id, dto)
        message.success('廠商報價已更新')
        setModalOpen(false)
        fetchPrices()
      } else {
        // 新增報價
        await doAddSupplierPrice(values, false)
      }
    } catch {
      message.error('操作失敗，請稍後再試')
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
      // 需求 2.4：第 5 家廠商警告
      setModalOpen(false)
      Modal.confirm({
        title: '廠商數量警告',
        content: result.warning ?? '此產品已有 4 家廠商，是否確認繼續新增第 5 家廠商？',
        okText: '確認新增',
        cancelText: '取消',
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
      message.success('廠商報價已新增')
      setModalOpen(false)
      fetchPrices()
    }
  }

  const columns: ColumnsType<ProductSupplierPrice> = [
    {
      title: '廠商名稱',
      dataIndex: 'supplierName',
      key: 'supplierName',
    },
    {
      title: '買價',
      dataIndex: 'unitPrice',
      key: 'unitPrice',
      render: (val: number) => val.toLocaleString(),
    },
    {
      title: '幣別',
      dataIndex: 'currency',
      key: 'currency',
      render: (val: string) => <Tag>{val}</Tag>,
    },
    {
      title: '最小訂購量',
      dataIndex: 'minOrderQty',
      key: 'minOrderQty',
    },
    {
      title: '交期（天）',
      dataIndex: 'leadTimeDays',
      key: 'leadTimeDays',
    },
    {
      title: '生效日期',
      dataIndex: 'effectiveDate',
      key: 'effectiveDate',
      render: (val: string) => new Date(val).toLocaleDateString('zh-TW'),
    },
    {
      title: '操作',
      key: 'actions',
      render: (_, record) => (
        <Button type="link" onClick={() => handleEdit(record)}>
          編輯
        </Button>
      ),
    },
  ]

  return (
    <div style={{ padding: 24 }}>
      <Space style={{ marginBottom: 16, justifyContent: 'space-between', width: '100%' }}>
        <Space>
          <Button onClick={() => navigate('/products')}>返回產品清單</Button>
          <h2 style={{ margin: 0 }}>產品 ID: {productId} 的廠商報價</h2>
        </Space>
        <Button type="primary" onClick={handleAdd}>
          新增廠商報價
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
