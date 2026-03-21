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
  const [modalOpen, setModalOpen] = useState(false)
  const [editingProduct, setEditingProduct] = useState<Product | null>(null)
  const navigate = useNavigate()

  const fetchProducts = useCallback(async () => {
    setLoading(true)
    try {
      // when showInactive is true, pass isActive=undefined to get all; otherwise only active
      const isActive = showInactive ? undefined : true
      const data = await getProducts(keyword || undefined, isActive)
      setProducts(data)
    } catch {
      message.error('載入產品清單失敗')
    } finally {
      setLoading(false)
    }
  }, [keyword, showInactive])

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
      title: '確認停用',
      content: `確定要停用產品「${product.name}」嗎？停用後將不會出現在一般清單中。`,
      okText: '確認停用',
      okType: 'danger',
      cancelText: '取消',
      onOk: async () => {
        try {
          await deactivateProduct(product.id)
          message.success('產品已停用')
          fetchProducts()
        } catch {
          message.error('停用失敗，請稍後再試')
        }
      },
    })
  }

  const columns: ColumnsType<Product> = [
    { title: '產品編號', dataIndex: 'productCode', key: 'productCode', width: 120 },
    { title: '產品名稱', dataIndex: 'name', key: 'name' },
    { title: '單位', dataIndex: 'unit', key: 'unit', width: 80 },
    {
      title: '狀態',
      dataIndex: 'isActive',
      key: 'isActive',
      width: 90,
      render: (isActive: boolean) =>
        isActive ? <Tag color="green">啟用</Tag> : <Tag color="default">停用</Tag>,
    },
    {
      title: '建立時間',
      dataIndex: 'createdAt',
      key: 'createdAt',
      width: 160,
      render: (val: string) => new Date(val).toLocaleString('zh-TW'),
    },
    {
      title: '操作',
      key: 'actions',
      width: 200,
      render: (_, record) => (
        <Space>
          <Button size="small" icon={<ShopOutlined />} onClick={() => navigate(`/products/${record.id}/suppliers`)}>
            廠商報價
          </Button>
          <Button size="small" onClick={() => openEdit(record)}>
            編輯
          </Button>
          {record.isActive && (
            <Button size="small" danger onClick={() => handleDeactivate(record)}>
              停用
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
          placeholder="搜尋產品編號或名稱"
          prefix={<SearchOutlined />}
          value={keyword}
          onChange={(e) => setKeyword(e.target.value)}
          allowClear
          style={{ width: 240 }}
        />
        <Space>
          <span>顯示停用產品</span>
          <Switch checked={showInactive} onChange={setShowInactive} />
        </Space>
        <Button type="primary" icon={<PlusOutlined />} onClick={openCreate} style={{ marginLeft: 'auto' }}>
          新增產品
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
