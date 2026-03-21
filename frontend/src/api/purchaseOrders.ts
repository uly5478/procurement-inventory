import client from './client'
import type { ApiResponse, PurchaseOrder } from '../types'

export interface CreatePurchaseOrderItemDto {
  productId: number
  quantity: number
  unitPrice: number
}

export interface CreatePurchaseOrderDto {
  supplierId: number
  items: CreatePurchaseOrderItemDto[]
}

export interface PurchaseOrderQueryParams {
  startDate?: string
  endDate?: string
  supplierName?: string
  status?: string
}

export async function getPurchaseOrders(params?: PurchaseOrderQueryParams) {
  const res = await client.get<ApiResponse<PurchaseOrder[]>>('/purchase-orders', { params })
  return res.data.data
}

export async function getPurchaseOrder(id: number) {
  const res = await client.get<ApiResponse<PurchaseOrder>>(`/purchase-orders/${id}`)
  return res.data.data
}

export async function createPurchaseOrder(dto: CreatePurchaseOrderDto) {
  const res = await client.post<ApiResponse<PurchaseOrder>>('/purchase-orders', dto)
  return res.data.data
}
