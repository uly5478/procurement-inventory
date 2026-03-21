import client from './client'
import type { ApiResponse, Product } from '../types'

export async function getProducts(keyword?: string, isActive?: boolean) {
  const params: Record<string, unknown> = {}
  if (keyword) params.keyword = keyword
  if (isActive !== undefined) params.isActive = isActive
  const res = await client.get<ApiResponse<Product[]>>('/products', { params })
  return res.data.data
}

export async function createProduct(dto: { productCode: string; name: string; unit: string }) {
  const res = await client.post<ApiResponse<Product>>('/products', dto)
  return res.data.data
}

export async function updateProduct(id: number, dto: { name: string; unit: string }) {
  const res = await client.put<ApiResponse<Product>>(`/products/${id}`, dto)
  return res.data.data
}

export async function deactivateProduct(id: number) {
  await client.patch(`/products/${id}/deactivate`)
}
