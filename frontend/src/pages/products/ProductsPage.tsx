import { useState, useEffect, useCallback } from 'react'
import { Table, Button, Input, Switch, Space, Tag, Modal, message } from 'antd'
import { PlusOutlined, SearchOutlined, ShopOutlined } from '@ant-design/icons'
import { useNavigate } from 'react-router-dom'
import type { ColumnsType } from 'antd/es/table'
import { getProducts, deactivateProduct } from '../../api/products'
import type { Product } from '../../types'
import ProductFormModal from './ProductFormModal'

export default function ProductsPage() {
  const [products, setProducts] = useState<Product[]>([])
  const [loading, setLoading] = useState(false)
  const [keyword, setKeyword] = useState('')
  const [showInactive, setShowInactive] = useState(false)
  const [categoryCode, setCategoryCode] = useState('')
  const [modalOpen, setModalOpen] = useState(false)
  const [editingProduct, setEditingProduct] = useState<Product | null>(null)
  const navigate = useNavigate()

  const fetchProducts = useCallback(async () => {
    setLoading(true)
    try {
      const isActive = showInactive ? undefined : true
      const data = await getProducts(keyword || undefined, isActive, categoryCode || undefined)
      setProducts(data)
    } catch {
      message.error('商品一覧の読み込みに失敗しました')
    } finally {
      setLoading(false)
    }
  }, [keyword, showInactive, categoryCode])

  useEffect(() => {
    fetchProducts()
  }, [fetchProducts])

  function openCreate() {
    setEditingProduct(null)
    setModalOpen(true)
  }

  function openEdit(product: Product) {
    setEditingProduct(product)
    setModalOpen(true)
  }

  function handleDeactivate(product: Product) {
    Modal.confirm({
      title: '無効化の確認',
      content: `商品「${product.name}」を無効化しますか？無効化後は通常の一覧に表示されなくなります。`,
      okText: '無効化',
      okType: 'danger',
      cancelText: 'キャンセル',
      onOk: async () => {
        try {
          await deactivateProduct(product.id)
          message.success('商品を無効化しました')
          fetchProducts()
        } catch {
          message.error('無効化に失敗しました')
        }
      },
    })
  }

  const columns: ColumnsType<Product> = [
    { title: '商品コード', dataIndex: 'productCode', key: 'productCode', width: 120 },
    { title: '商品名', dataIndex: 'name', key: 'name' },
    { title: '単位', dataIndex: 'unit', key: 'unit', width: 80 },
    { title: '仕入分類', dataIndex: 'categoryCode', key: 'categoryCode', width: 100, render: (val?: string) => val ? <Tag color="blue">{val}</Tag> : '-' },
    { title: '箱入数', dataIndex: 'boxQty', key: 'boxQty', width: 80 },
    { title: 'MOQ', dataIndex: 'moq', key: 'moq', width: 80 },
    { title: '安全在庫', dataIndex: 'safetyStock', key: 'safetyStock', width: 90 },
    { title: '平均出荷数', dataIndex: 'averageShipment', key: 'averageShipment', width: 100 },
    {
      title: 'ステータス',
      dataIndex: 'isActive',
      key: 'isActive',
      width: 100,
      render: (isActive: boolean) =>
        isActive ? <Tag color="green">有効</Tag> : <Tag color="default">無効</Tag>,
    },
    {
      title: '作成日時',
      dataIndex: 'createdAt',
      key: 'createdAt',
      width: 160,
      render: (val: string) => new Date(val).toLocaleString('ja-JP'),
    },
    {
      title: '操作',
      key: 'actions',
      width: 200,
      render: (_, record) => (
        <Space>
          <Button size="small" icon={<ShopOutlined />} onClick={() => navigate(`/products/${record.id}/suppliers`)}>
            仕入先単価
          </Button>
          <Button size="small" onClick={() => openEdit(record)}>
            編集
          </Button>
          {record.isActive && (
            <Button size="small" danger onClick={() => handleDeactivate(record)}>
              無効化
            </Button>
          )}
        </Space>
      ),
    },
  ]

  return (
    <div style={{ padding: 24 }}>
      <div style={{ display: 'flex', gap: 12, marginBottom: 16, flexWrap: 'wrap' }}>
        <Input
          placeholder="商品コードまたは商品名で検索"
          prefix={<SearchOutlined />}
          value={keyword}
          onChange={(e) => setKeyword(e.target.value)}
          allowClear
          style={{ width: 260 }}
        />
        <Input
          placeholder="仕入分類コード"
          value={categoryCode}
          onChange={(e) => setCategoryCode(e.target.value)}
          allowClear
          style={{ width: 140 }}
        />
        <Space>
          <span>無効な商品を表示</span>
          <Switch checked={showInactive} onChange={setShowInactive} />
        </Space>
        <Button type="primary" icon={<PlusOutlined />} onClick={openCreate} style={{ marginLeft: 'auto' }}>
          商品を追加
        </Button>
      </div>

      <Table
        rowKey="id"
        columns={columns}
        dataSource={products}
        loading={loading}
        pagination={{ pageSize: 20, showSizeChanger: false }}
      />

      <ProductFormModal
        open={modalOpen}
        editingProduct={editingProduct}
        onClose={() => setModalOpen(false)}
        onSuccess={() => {
          setModalOpen(false)
          fetchProducts()
        }}
      />
    </div>
  )
}