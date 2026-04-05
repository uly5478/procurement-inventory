import client from './client'
import type { ApiResponse, Product } from '../types'

export interface CreateProductDto {
  productCode: string
  name: string
  unit: string
  boxQty?: number
  moq?: number
  safetyStock?: number
  averageShipment?: number
}

export interface UpdateProductDto {
  name: string
  unit: string
  boxQty?: number
  moq?: number
  safetyStock?: number
  averageShipment?: number
  categoryCode?: string
}

export async function getProducts(keyword?: string, isActive?: boolean, categoryCode?: string) {
  const params: Record<string, unknown> = {}
  if (keyword) params.keyword = keyword
  if (isActive !== undefined) params.isActive = isActive
  if (categoryCode) params.categoryCode = categoryCode
  const res = await client.get<ApiResponse<Product[]>>('/products', { params })
  return res.data.data
}

export async function createProduct(dto: CreateProductDto) {
  const res = await client.post<ApiResponse<Product>>('/products', dto)
  return res.data.data
}

export async function updateProduct(id: number, dto: UpdateProductDto) {
  const res = await client.put<ApiResponse<Product>>(`/products/${id}`, dto)
  return res.data.data
}

export async function deactivateProduct(id: number) {
  await client.patch(`/products/${id}/deactivate`)
}
